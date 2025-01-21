using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


//               ___====-_  _-====___
//         _--^^^#####//      \\#####^^^--_
//      _-^##########// (    ) \\##########^-_
//     -############//  |\^^/|  \\############-
//   _/############//   (@::@)   \\############\_
//  /#############((     \\//     ))#############\
// -###############\\    (oo)    //###############-
//-#################\\  / VV \  //#################-
//-###################\\/      \//###################-
//_#/|##########/\######(   /\   )######/\##########|\#_
// |/ |#/\#/\#/\/  \#/\##\  |  |  /##/\#/  \/\#/\#/\| \
// '  |/  _lerpSpeed  _lerpSpeed '   _lerpSpeed  \\#\| |  | |/#/  _lerpSpeed   '  _lerpSpeed  _lerpSpeed  \|
//    '   '  '      '   / | |  | | \   '      '  '   '
//                     (  | |  | |  )
//                    __\ | |  | | /__
//                   (vvv(VVV)(VVV)vvv)
//||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
//                                                              ||
//                   SYNTAX PARSING HELL                        ||    
//                                                              ||
//||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


public class CoroutineTransformer : MonoBehaviour
{
    private float _codeRate;
    [SerializeField]
    Slider _slider;

    private int _lineOffset;
    private HashSet<string> _methodsToTransform = new HashSet<string>();
    [SerializeField]
    private float _minCodeRate;
    [SerializeField]
    private float _maxCodeRate;

    public void Initialize(int lineOffset)
    {
        _lineOffset = lineOffset;
        _methodsToTransform = new HashSet<string>();
    }

    private StatementSyntax TransformMethodInvocationsToYieldReturn(StatementSyntax statement)
    {
        // Find method invocations in the statement
        var invocationExpressions = statement.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToList();

        StatementSyntax transformedStatement;
        foreach (var invocation in invocationExpressions)
        {
            string methodName = null;

            if (invocation.Expression is IdentifierNameSyntax identifier)
            {
                methodName = identifier.Identifier.Text;
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                methodName = memberAccess.Name.Identifier.Text;
            }

            if (methodName != null && _methodsToTransform.Contains(methodName))
            {
                // Create a new yield return statement
                var arguments = invocation.ArgumentList.Arguments;
                var argList = string.Join(", ", arguments.Select(arg => arg.ToString()));

                // Construct the yield return statement
                var yieldReturnStatement = SyntaxFactory.ParseStatement($"yield return COR_{methodName}({argList});");

                return yieldReturnStatement;
            }
        }
        return statement; // Return the original if no method calls were found
    }

    public string TransformToCoroutine(string sourceCode)
    {
        _codeRate = Mathf.Lerp(_minCodeRate, _maxCodeRate, (1 - _slider.value));

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot();

        // Get all method declarations in the class
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        if (methods.Count == 0) return sourceCode;

        // Get the parent class of the methods
        var parentClass = methods.First().Parent as ClassDeclarationSyntax;
        if (parentClass == null) return sourceCode;

        // Create a list to store modified methods
        var transformedMethods = new List<MethodDeclarationSyntax>();

        //Pruned Methods have their Debug Log statements removed and stuff;
        var prunedMethods = new List<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            _methodsToTransform.Add(method.Identifier.Text);

            var prunedMethod = RemoveConsoleStatements(method);
            prunedMethods.Add(prunedMethod);
        }

        foreach (var method in methods)
        {
            // Transform each method into a coroutine with a unique name
            var coroutineMethod = TransformToCoroutine(method)
                                                       .WithIdentifier(SyntaxFactory.Identifier("COR_" + method.Identifier.Text));

            // Add the transformed method to the list
            transformedMethods.Add(coroutineMethod);

            // TODO: Modify the original method to call the coroutine instead
            // You could do this by adding a call to the coroutine in the method body,
            // or by replacing the method's body entirely depending on the intended behavior.
        }


        // Replace the original methods with the modified methods
        var prunedClass = parentClass.ReplaceNodes(
    parentClass.Members.OfType<MethodDeclarationSyntax>(),
    (original, rewritten) => prunedMethods.First(m => m.Identifier.Text == original.Identifier.Text)
);

        // Add both the original and coroutine-transformed methods to the class
        var modifiedClass = prunedClass.AddMembers(transformedMethods.ToArray());

        // Add the RunCoroutineMethod to the class
        modifiedClass = AddRunCoroutineMethod(modifiedClass);
        modifiedClass = AddStopCoroutinesMethod(modifiedClass);
        // Replace the modified class in the root syntax tree
        var newRoot = root.ReplaceNode(parentClass, modifiedClass);

        return newRoot.NormalizeWhitespace().ToFullString();
    }

    private static MethodDeclarationSyntax RemoveConsoleStatements(MethodDeclarationSyntax method)
    {
        // Create a new method with the same identifier, return type, and _testCaseCalls
        var newMethod = method.WithBody(RemoveConsoleFromBlock(method.Body));

        return newMethod;
    }

    private static BlockSyntax RemoveConsoleFromBlock(BlockSyntax block)
    {
        // Filter out statements that contain Console.
        var statements = block.Statements
            .Where(statement => !ContainsConsoleCall(statement))
            .ToList();

        // Return a new block with the filtered statements
        return block.WithStatements(SyntaxFactory.List(statements));
    }

    private static bool ContainsConsoleCall(StatementSyntax statement)
    {
        // Check if the statement contains a Console call
        return statement.ToString().Contains("Console.");
    }

    public MethodDeclarationSyntax TransformToCoroutine(MethodDeclarationSyntax method)
    {
        var newReturnType = SyntaxFactory.ParseTypeName("System.Collections.IEnumerator");
        var parameters = method.ParameterList.Parameters;
        var originalStatements = method.Body?.Statements ?? new SyntaxList<StatementSyntax>();

        // Transform all statements, including nested blocks
        var transformedStatements = TransformStatements(originalStatements);

        // DisableMiniGame highlight line after all statements
        var disableRunningLine = SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.DisableHighlightLine();");
        transformedStatements.Add(disableRunningLine);

        var newBody = SyntaxFactory.Block(transformedStatements);

        return method
            .WithReturnType(newReturnType)
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
            .WithBody(newBody)
            .WithModifiers(method.Modifiers)
            .WithAttributeLists(method.AttributeLists);
    }
    // Recursive method to transform statements, handling nested blocks
    private List<StatementSyntax> TransformStatements(SyntaxList<StatementSyntax> statements)
    {
        var newStatements = new List<StatementSyntax>();
        foreach (var statement in statements)
        {
            var location = statement.GetLocation();
            var lineSpan = location.GetLineSpan();
            var lineNumber = lineSpan.StartLinePosition.Line;


            newStatements.Add(SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.HighlightLine({lineNumber - _lineOffset});"));
            newStatements.Add(SyntaxFactory.ParseStatement($"yield return new UnityEngine.WaitForSeconds({_codeRate}f);"));




            // Recursively handle nested blocks
            if (statement is BlockSyntax block)
            {
                var transformedNestedStatements = TransformStatements(block.Statements);
                var newBlock = SyntaxFactory.Block(transformedNestedStatements);
                newStatements.Add(newBlock);
            }
            else if (statement is IfStatementSyntax ifStatement)
            {
                // Recursively transform the 'then' and 'else' blocks of the if statement
                var transformedIfStatement = TransformIfStatement(ifStatement);
                newStatements.Add(transformedIfStatement);
            }
            else if (statement is ForStatementSyntax forStatement)
            {
                // Recursively transform the body of the for statement
                var transformedForStatement = TransformForStatement(forStatement);
                newStatements.Add(transformedForStatement);
            }
            else if (statement is WhileStatementSyntax whileStatement)
            {
                // Recursively transform the body of the while statement
                var transformedWhileStatement = TransformWhileStatement(whileStatement);
                newStatements.Add(transformedWhileStatement);
            }
            else if (ContainsMethodInvocation(statement))
            {
                var transformedStatement = TransformMethodInvocationsToYieldReturn(statement);
                newStatements.Add(transformedStatement);

                // Check if the statement is an assignment of any type
                if (statement is ExpressionStatementSyntax expressionStatement && expressionStatement.Expression is AssignmentExpressionSyntax)
                {
                    newStatements.Add(statement);
                }
                else if (statement is LocalDeclarationStatementSyntax)
                {
                    // It's a variable declaration like "int i = 5;"
                    newStatements.Add(statement);
                }
            }
            else if (statement is ReturnStatementSyntax returnStatement)
            {
                if (returnStatement.Expression != null)
                {
                    //var logReturnValue = SyntaxFactory.ParseStatement(
                    //    $"Debug.Log($\"Line {lineNumber - _lineOffset}: RETURN \" + ({returnStatement.Expression}).ToString());");
                    //newStatements.Add(logReturnValue);
                }
                //newStatements.Add(statement); // add the actual return statement
            }
            else
            {
                newStatements.Add(statement); // Add the statement as is
            }
            var corEnd = SyntaxFactory.EmptyStatement();
            newStatements.Add(corEnd);
        }

        return newStatements;
    }
    // Helper method for transforming If statements
    private IfStatementSyntax TransformIfStatement(IfStatementSyntax ifStatement)
    {
        var transformedCondition = ifStatement.Condition;

        // Wrap single statements in a block if braces are missing
        var transformedThenStatement = ifStatement.Statement is BlockSyntax block
            ? SyntaxFactory.Block(TransformStatements(block.Statements))
            : SyntaxFactory.Block(TransformStatements(new SyntaxList<StatementSyntax> { ifStatement.Statement }));

        ElseClauseSyntax? transformedElse = null;
        if (ifStatement.Else != null)
        {
            // Wrap single statements in a block if braces are missing in the else clause
            var elseStatements = ifStatement.Else.Statement is BlockSyntax elseBlock
                ? elseBlock.Statements
                : new SyntaxList<StatementSyntax> { ifStatement.Else.Statement };

            var transformedElseStatements = SyntaxFactory.Block(TransformStatements(elseStatements));
            transformedElse = SyntaxFactory.ElseClause(transformedElseStatements);
        }

        return ifStatement
            .WithCondition(transformedCondition)
            .WithStatement(transformedThenStatement)
            .WithElse(transformedElse);
    }



    // Helper method for transforming For statements
    private ForStatementSyntax TransformForStatement(ForStatementSyntax forStatement)
    {
        var transformedBody = SyntaxFactory.Block(TransformStatements(forStatement.Statement is BlockSyntax block ? block.Statements : new SyntaxList<StatementSyntax> { forStatement.Statement }));
        return forStatement.WithStatement(transformedBody);
    }

    // Helper method for transforming While statements
    private WhileStatementSyntax TransformWhileStatement(WhileStatementSyntax whileStatement)
    {
        var transformedBody = SyntaxFactory.Block(TransformStatements(whileStatement.Statement is BlockSyntax block ? block.Statements : new SyntaxList<StatementSyntax> { whileStatement.Statement }));
        return whileStatement.WithStatement(transformedBody);
    }
    private bool ContainsMethodInvocation(StatementSyntax statement)
    {
        // Check if the statement contains any method invocations that need to be transformed
        return statement.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation =>
            {
                if (invocation.Expression is IdentifierNameSyntax identifier)
                {
                    return _methodsToTransform.Contains(identifier.Identifier.ValueText);
                }
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    return _methodsToTransform.Contains(memberAccess.Name.Identifier.ValueText);
                }
                return false;
            });
    }

    private StatementSyntax TransformMethodInvocations(StatementSyntax statement)
    {
        // Create a new syntax rewriter to transform method calls
        var rewriter = new MethodInvocationRewriter(_methodsToTransform);
        var transformedStatement = rewriter.Visit(statement);
        return transformedStatement as StatementSyntax;
    }

    public ClassDeclarationSyntax AddRunCoroutineMethod(ClassDeclarationSyntax classDeclaration)
    {
        var runCoroutineMethod = GenerateRunCoroutineMethod();
        return classDeclaration.AddMembers(runCoroutineMethod);
    }



    private MethodDeclarationSyntax GenerateRunCoroutineMethod()
    {
        var runCoroutineMethodCode = @"
        public void RunCoroutineMethod(string methodName, params object[] args)
        {
            var method = this.GetType().GetMethod(methodName);
            if (method != null)
            {
                CodeInspector.RuntimeManager.Instance.CoroutineRunner.StartCoroutine((System.Collections.IEnumerator)method.Invoke(this, args));
            }
            else
            {
                throw new System.Exception($""Coroutine '{methodName}' not found."");
            }
        }";

        return SyntaxFactory.ParseMemberDeclaration(runCoroutineMethodCode) as MethodDeclarationSyntax;
    }

    public ClassDeclarationSyntax AddStopCoroutinesMethod(ClassDeclarationSyntax classDeclaration)
    {
        var stopCoroutinesMethod = GenerateStopCoroutinesMethod();
        return classDeclaration.AddMembers(stopCoroutinesMethod);
    }

    private MethodDeclarationSyntax GenerateStopCoroutinesMethod()
    {
        var runCoroutineMethodCode = @"
        public void StopCoroutinesMethod()
        {

                CodeInspector.RuntimeManager.Instance.CoroutineRunner.StopAllCoroutines();

        }";

        return SyntaxFactory.ParseMemberDeclaration(runCoroutineMethodCode) as MethodDeclarationSyntax;
    }

}

// Syntax rewriter to transform method invocations into coroutine calls
public class MethodInvocationRewriter : CSharpSyntaxRewriter
{
    private readonly HashSet<string> _methodsToTransform;

    public MethodInvocationRewriter(HashSet<string> methodsToTransform)
    {
        _methodsToTransform = methodsToTransform;
    }

    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        string methodName = null;

        if (node.Expression is IdentifierNameSyntax identifier)
        {
            methodName = identifier.Identifier.Text;
        }
        else if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            methodName = memberAccess.Name.Identifier.Text;
        }

        if (methodName != null && _methodsToTransform.Contains(methodName))
        {
            // Transform the method call into a yield return StartCoroutine
            var arguments = node.ArgumentList.Arguments;
            var argList = string.Join(", ", arguments.Select(arg => arg.ToString()));

            // Create a new yield return statement
            var startCoroutineCall = SyntaxFactory.ParseExpression($"yield return CodeInspector.RuntimeManager.Instance.StartCoroutine(COR_{methodName}({argList}))");

            return startCoroutineCall;
        }

        return base.VisitInvocationExpression(node);
    }
}