//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;


//public class CoroutineResult<T>
//{
//    public T Value { get; set; }
//}
//public class CoroutineTransformer : MonoBehaviour
//{
//    private int _lineOffset;
//    private HashSet<string> _methodsToTransform = new HashSet<string>();
//    private Dictionary<string, string> _methodReturnTypes = new Dictionary<string, string>();
//    public void Initialize(int lineOffset)
//    {
//        _lineOffset = lineOffset;
//        _methodsToTransform = new HashSet<string>();
//        _methodReturnTypes = new Dictionary<string, string>();
//    }

//    private StatementSyntax TransformMethodInvocationsToYieldReturn(StatementSyntax statement)
//    {
//        var invocationExpressions = statement.DescendantNodes()
//            .OfType<InvocationExpressionSyntax>()
//            .ToList();

//        foreach (var invocation in invocationExpressions)
//        {
//            string methodName = null;
//            if (invocation.Expression is IdentifierNameSyntax identifier)
//            {
//                methodName = identifier.Identifier.Text;
//            }
//            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
//            {
//                methodName = memberAccess.Name.Identifier.Text;
//            }

//            if (methodName != null && _methodsToTransform.Contains(methodName))
//            {
//                var arguments = invocation.ArgumentList.Arguments;
//                var argList = arguments.Any()
//                    ? string.Join(", ", arguments.Select(arg => arg.ToString()))
//                    : "";

//                // Check if method has a non-void return type
//                if (_methodReturnTypes.TryGetValue(methodName, out string returnType) && returnType != "void")
//                {
//                    var resultVar = $"result_{methodName}";
//                    // Create result wrapper
//                    var varDecl = SyntaxFactory.ParseStatement($"var {resultVar} = new CoroutineResult<{returnType}>();");
//                    // Create yield return statement with result wrapper
//                    var yieldReturn = argList.Length > 0
//                        ? SyntaxFactory.ParseStatement($"yield return COR_{methodName}({argList}, {resultVar});")
//                        : SyntaxFactory.ParseStatement($"yield return COR_{methodName}({resultVar});");

//                    // If this is part of a return statement
//                    if (statement is ReturnStatementSyntax)
//                    {
//                        return SyntaxFactory.Block(
//                            varDecl,
//                            yieldReturn,
//                            SyntaxFactory.ParseStatement($"return {resultVar}.Value;")
//                        );
//                    }

//                    // If this is part of an assignment
//                    if (statement is ExpressionStatementSyntax expressionStatement &&
//                        expressionStatement.Expression is AssignmentExpressionSyntax assignment)
//                    {
//                        return SyntaxFactory.Block(
//                            varDecl,
//                            yieldReturn,
//                            SyntaxFactory.ParseStatement($"{assignment.Left} = {resultVar}.Value;")
//                        );
//                    }

//                    return SyntaxFactory.Block(varDecl, yieldReturn);
//                }
//                else
//                {
//                    // Handle void methods
//                    return argList.Length > 0
//                        ? SyntaxFactory.ParseStatement($"yield return COR_{methodName}({argList});")
//                        : SyntaxFactory.ParseStatement($"yield return COR_{methodName}();");
//                }
//            }
//        }
//        return statement;
//    }
//    public string TransformToCoroutine(string sourceCode)
//    {
//        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
//        var root = syntaxTree.GetRoot();

//        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
//        if (methods.Count == 0) return sourceCode;

//        var parentClass = methods.First().Parent as ClassDeclarationSyntax;
//        if (parentClass == null) return sourceCode;

//        var transformedMethods = new List<MethodDeclarationSyntax>();

//        // First pass: collect return types
//        foreach (var method in methods)
//        {
//            _methodsToTransform.Add(method.Identifier.Text);
//            if (method.ReturnType is PredefinedTypeSyntax || method.ReturnType is IdentifierNameSyntax)
//            {
//                _methodReturnTypes[method.Identifier.Text] = method.ReturnType.ToString();
//            }
//        }

//        // Second pass: transform methods
//        foreach (var method in methods)
//        {
//            var coroutineMethod = TransformToCoroutine(method)
//                .WithIdentifier(SyntaxFactory.Identifier("COR_" + method.Identifier.Text));
//            transformedMethods.Add(coroutineMethod);
//        }

//        var modifiedClass = parentClass.AddMembers(transformedMethods.ToArray());
//        modifiedClass = AddRunCoroutineMethod(modifiedClass);

//        var newRoot = root.ReplaceNode(parentClass, modifiedClass);
//        return newRoot.NormalizeWhitespace().ToFullString();
//    }


//    public MethodDeclarationSyntax TransformToCoroutine(MethodDeclarationSyntax method)
//    {
//        var originalReturnType = method.ReturnType.ToString();
//        var parameters = method.ParameterList.Parameters;
//        var parameterList = parameters.ToList();

//        // Add result wrapper parameter if method has return type that's not void
//        if (_methodReturnTypes.ContainsKey(method.Identifier.Text))
//        {
//            var returnType = _methodReturnTypes[method.Identifier.Text];
//            if (returnType != "void")
//            {
//                var resultParameter = SyntaxFactory.Parameter(
//                    SyntaxFactory.Identifier("result"))
//                    .WithType(SyntaxFactory.ParseTypeName($"CoroutineResult<{returnType}>"));
//                parameterList.Add(resultParameter);
//            }
//        }

//        var originalStatements = method.Body?.Statements ?? new SyntaxList<StatementSyntax>();
//        var transformedStatements = new List<StatementSyntax>();

//        foreach (var statement in originalStatements)
//        {
//            if (statement is ReturnStatementSyntax returnStatement && returnStatement.Expression != null)
//            {
//                // Transform return statements to set the result wrapper
//                transformedStatements.Add(SyntaxFactory.ParseStatement(
//                    $"result.Value = {returnStatement.Expression.ToString()};"));
//                transformedStatements.Add(SyntaxFactory.ParseStatement("yield break;"));
//            }
//            else
//            {
//                transformedStatements.Add(statement);
//            }
//        }

//        var disableRunningLine = SyntaxFactory.ParseStatement(
//            $"CodeInspector.RuntimeManager.Instance.DisableHighlightLine();");
//        transformedStatements.Add(disableRunningLine);

//        return method
//            .WithReturnType(SyntaxFactory.ParseTypeName("System.Collections.IEnumerator"))
//            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterList)))
//            .WithBody(SyntaxFactory.Block(transformedStatements))
//            .WithModifiers(method.Modifiers)
//            .WithAttributeLists(method.AttributeLists);
//    }

//    // Recursive method to transform statements, handling nested blocks
//    private List<StatementSyntax> TransformStatements(SyntaxList<StatementSyntax> statements)
//    {
//        var newStatements = new List<StatementSyntax>();
//        float lineTime = 1f;
//        foreach (var statement in statements)
//        {
//            var location = statement.GetLocation();
//            var lineSpan = location.GetLineSpan();
//            var lineNumber = lineSpan.StartLinePosition.Line;

//            // Add coroutine markers and line highlighting for each statement
//            newStatements.Add(SyntaxFactory.ParseStatement($"//---- COR START ----"));
//            newStatements.Add(SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.HighlightLine({lineNumber - _lineOffset});"));
//            newStatements.Add(SyntaxFactory.ParseStatement($"yield return new UnityEngine.WaitForSeconds({lineTime}f);"));

//            // Recursively handle nested blocks
//            if (statement is BlockSyntax block)
//            {
//                var transformedNestedStatements = TransformStatements(block.Statements);
//                var newBlock = SyntaxFactory.Block(transformedNestedStatements);
//                newStatements.Add(newBlock);
//            }
//            else if (statement is IfStatementSyntax ifStatement)
//            {
//                // Recursively transform the 'then' and 'else' blocks of the if statement
//                var transformedIfStatement = TransformIfStatement(ifStatement);
//                newStatements.Add(transformedIfStatement);
//            }
//            else if (statement is ForStatementSyntax forStatement)
//            {
//                // Recursively transform the body of the for statement
//                var transformedForStatement = TransformForStatement(forStatement);
//                newStatements.Add(transformedForStatement);
//            }
//            else if (statement is WhileStatementSyntax whileStatement)
//            {
//                // Recursively transform the body of the while statement
//                var transformedWhileStatement = TransformWhileStatement(whileStatement);
//                newStatements.Add(transformedWhileStatement);
//            }
//            else if (ContainsMethodInvocation(statement))
//            {
//                var transformedStatement = TransformMethodInvocationsToYieldReturn(statement);
//                newStatements.Add(transformedStatement);
//            }
//            else if (statement is ReturnStatementSyntax returnStatement)
//            {
//                if (returnStatement.Expression != null)
//                {
//                    var logReturnValue = SyntaxFactory.ParseStatement(
//                        $"Debug.Log($\"Line {lineNumber - _lineOffset}: RETURN \" + {returnStatement.Expression}.ToString());");
//                    newStatements.Add(logReturnValue);
//                }
//                //newStatements.Add(statement); // add the actual return statement
//            }
//            else
//            {
//                newStatements.Add(statement); // Add the statement as is
//            }

//            newStatements.Add(SyntaxFactory.ParseStatement($"//---- COR END ----"));
//        }

//        return newStatements;
//    }
//    // Helper method for transforming If statements
//    private IfStatementSyntax TransformIfStatement(IfStatementSyntax ifStatement)
//    {
//        var transformedCondition = ifStatement.Condition;

//        // Wrap single statements in a block if braces are missing
//        var transformedThenStatement = ifStatement.Statement is BlockSyntax block
//            ? SyntaxFactory.Block(TransformStatements(block.Statements))
//            : SyntaxFactory.Block(TransformStatements(new SyntaxList<StatementSyntax> { ifStatement.Statement }));

//        ElseClauseSyntax? transformedElse = null;
//        if (ifStatement.Else != null)
//        {
//            // Wrap single statements in a block if braces are missing in the else clause
//            var elseStatements = ifStatement.Else.Statement is BlockSyntax elseBlock
//                ? elseBlock.Statements
//                : new SyntaxList<StatementSyntax> { ifStatement.Else.Statement };

//            var transformedElseStatements = SyntaxFactory.Block(TransformStatements(elseStatements));
//            transformedElse = SyntaxFactory.ElseClause(transformedElseStatements);
//        }

//        return ifStatement
//            .WithCondition(transformedCondition)
//            .WithStatement(transformedThenStatement)
//            .WithElse(transformedElse);
//    }



//    // Helper method for transforming For statements
//    private ForStatementSyntax TransformForStatement(ForStatementSyntax forStatement)
//    {
//        var transformedBody = SyntaxFactory.Block(TransformStatements(forStatement.Statement is BlockSyntax block ? block.Statements : new SyntaxList<StatementSyntax> { forStatement.Statement }));
//        return forStatement.WithStatement(transformedBody);
//    }

//    // Helper method for transforming While statements
//    private WhileStatementSyntax TransformWhileStatement(WhileStatementSyntax whileStatement)
//    {
//        var transformedBody = SyntaxFactory.Block(TransformStatements(whileStatement.Statement is BlockSyntax block ? block.Statements : new SyntaxList<StatementSyntax> { whileStatement.Statement }));
//        return whileStatement.WithStatement(transformedBody);
//    }
//    private bool ContainsMethodInvocation(StatementSyntax statement)
//    {
//        // Check if the statement contains any method invocations that need to be transformed
//        return statement.DescendantNodes()
//            .OfType<InvocationExpressionSyntax>()
//            .Any(invocation =>
//            {
//                if (invocation.Expression is IdentifierNameSyntax identifier)
//                {
//                    return _methodsToTransform.Contains(identifier.Identifier.ValueText);
//                }
//                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
//                {
//                    return _methodsToTransform.Contains(memberAccess.Name.Identifier.ValueText);
//                }
//                return false;
//            });
//    }


//    public ClassDeclarationSyntax AddRunCoroutineMethod(ClassDeclarationSyntax classDeclaration)
//    {
//        var runCoroutineMethod = GenerateRunCoroutineMethod();
//        return classDeclaration.AddMembers(runCoroutineMethod);
//    }

//    // Update RunCoroutine method to use the wrapper
//    private MethodDeclarationSyntax GenerateRunCoroutineMethod()
//    {
//        var runCoroutineMethodCode = @"
//        public T RunCoroutineMethod<T>(string methodName, params object[] args)
//        {
//            var method = this.GetType().GetMethod($""COR_{methodName}"");
//            if (method != null)
//            {
//                var result = new CoroutineResult<T>();
//                var allArgs = args.ToList();
//                allArgs.Add(result);

//                CodeInspector.RuntimeManager.Instance.StartCoroutine(
//                    (System.Collections.IEnumerator)method.Invoke(this, allArgs.ToArray()));

//                return result.Value;
//            }
//            throw new System.Exception($""Coroutine '{methodName}' not found."");
//        }

//        public void RunCoroutineMethod(string methodName, params object[] args)
//        {
//            var method = this.GetType().GetMethod($""COR_{methodName}"");
//            if (method != null)
//            {
//                CodeInspector.RuntimeManager.Instance.StartCoroutine(
//                    (System.Collections.IEnumerator)method.Invoke(this, args));
//            }
//            else
//            {
//                throw new System.Exception($""Coroutine '{methodName}' not found."");
//            }
//        }";

//        return SyntaxFactory.ParseMemberDeclaration(runCoroutineMethodCode) as MethodDeclarationSyntax;
//    }
//}
