using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTransformer : MonoBehaviour
{
    private int _lineOffset;

    public void Initialize(int lineOffset)
    {
        _lineOffset = lineOffset;
    }

    public MethodDeclarationSyntax TransformToCoroutine(MethodDeclarationSyntax method)
    {
        float lineTime = .5f;
        var newReturnType = SyntaxFactory.ParseTypeName("System.Collections.IEnumerator");
        var parameters = method.ParameterList.Parameters;
        var originalStatements = method.Body?.Statements ?? new SyntaxList<StatementSyntax>();
        var newStatements = new List<StatementSyntax>();

        foreach (var statement in originalStatements)
        {
            if (statement is ReturnStatementSyntax returnStatement)
            {
                if (returnStatement.Expression != null)
                {
                    // Handle return with value
                    var returnValueExpression = returnStatement.Expression;
                    var location = statement.GetLocation();
                    var lineSpan = location.GetLineSpan();
                    var lineNumber = lineSpan.StartLinePosition.Line;

                    var runningLine = SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.HighlightLine({lineNumber - _lineOffset});");
                    newStatements.Add(runningLine);


                    var logReturnValue = SyntaxFactory.ParseStatement(
                        $"Console.WriteLine(\"Line {lineNumber - _lineOffset}: RETURN \" + {"(" + returnValueExpression + ")"}.ToString());");
                    newStatements.Add(logReturnValue);
                }
                else
                {
                    // Handle void return
                    var location = statement.GetLocation();
                    var lineSpan = location.GetLineSpan();
                    var lineNumber = lineSpan.StartLinePosition.Line;
                    //var logReturnValue = SyntaxFactory.ParseStatement($"Console.WriteLine(\"Line {lineNumber - _lineOffset}: RETURN\");");
                    //newStatements.Add(logReturnValue);
                    var runningLine = SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.HighlightLine({lineNumber - _lineOffset});");
                    newStatements.Add(runningLine);
                }
            }
            else
            {
                // Add the original statement
                newStatements.Add(statement);
                var location = statement.GetLocation();
                var lineSpan = location.GetLineSpan();
                var lineNumber = lineSpan.StartLinePosition.Line;
                //var logStatement = SyntaxFactory.ParseStatement($"Console.WriteLine(\"Line {lineNumber - _lineOffset}: {statement.ToString()}\");");
                //newStatements.Add(logStatement);
                var runningLine = SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.HighlightLine({lineNumber - _lineOffset});");
                newStatements.Add(runningLine);
            }

            // Add yield return after each statement
            var yieldStatement = SyntaxFactory.ParseStatement($"yield return new UnityEngine.WaitForSeconds({lineTime}f);");
            newStatements.Add(yieldStatement);

            var disableRunningLine = SyntaxFactory.ParseStatement($"CodeInspector.RuntimeManager.Instance.DisableHighlightLine();");
            newStatements.Add(disableRunningLine);
        }

        var newBody = SyntaxFactory.Block(newStatements);

        // Preserve the original method modifiers and attributes
        return method
            .WithReturnType(newReturnType)
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
            .WithBody(newBody)
            .WithModifiers(method.Modifiers) // Preserve original modifiers
            .WithAttributeLists(method.AttributeLists); // Preserve original attributes
    }


    public ClassDeclarationSyntax AddRunCoroutineMethod(ClassDeclarationSyntax classDeclaration)
    {
        // Generate the RunCoroutineMethod
        var runCoroutineMethod = GenerateRunCoroutineMethod();

        // Add the generated method to the class
        return classDeclaration.AddMembers(runCoroutineMethod);
    }


    private MethodDeclarationSyntax GenerateRunCoroutineMethod()
    {
        var runCoroutineMethodCode = $@"
        public void RunCoroutineMethod(string methodName, params object[] args)
        {{
            var method = this.GetType().GetMethod(methodName);
            if (method != null)
            {{
                CodeInspector.RuntimeManager.Instance.StartCoroutine((System.Collections.IEnumerator)method.Invoke(this, args));
            }}
            else
            {{
                throw new System.Exception($""Coroutine '{{methodName}}' not found."");
            }}
        }}";

        return SyntaxFactory.ParseMemberDeclaration(runCoroutineMethodCode) as MethodDeclarationSyntax;
    }
}