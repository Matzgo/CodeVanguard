using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BannedCallsDetector : MonoBehaviour
{
    [Serializable]
    public class BannedCall
    {
        public string Namespace;
        public string Type;
        public string Method;

        public BannedCall(string ns, string type, string method)
        {
            Namespace = ns;
            Type = type;
            Method = method;
        }
    }

    [SerializeField]
    private List<BannedCall> bannedCalls = new List<BannedCall>();

    private void Awake()
    {
        // Example initialization of banned calls
        bannedCalls.Add(new BannedCall("System", "Console", "Write"));
        bannedCalls.Add(new BannedCall("System", "Console", "WriteLine"));
        // Add more banned calls as needed
    }

    public bool ContainsBannedCalls(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
            // Add other necessary references
        };

        var compilation = CSharpCompilation.Create("DynamicCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var model = compilation.GetSemanticModel(syntaxTree);

        var invocations = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var symbolInfo = model.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                var containingType = methodSymbol.ContainingType;
                var containingNamespace = containingType.ContainingNamespace;

                foreach (var bannedCall in bannedCalls)
                {
                    if (IsMatchingBannedCall(containingNamespace, containingType, methodSymbol, bannedCall))
                    {
                        Debug.Log($"Banned call detected: {containingNamespace}.{containingType}.{methodSymbol.Name}");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsMatchingBannedCall(INamespaceSymbol namespaceSymbol, INamedTypeSymbol typeSymbol, IMethodSymbol methodSymbol, BannedCall bannedCall)
    {
        bool namespaceMatch = string.IsNullOrEmpty(bannedCall.Namespace) ||
                              namespaceSymbol.ToDisplayString() == bannedCall.Namespace;
        bool typeMatch = string.IsNullOrEmpty(bannedCall.Type) ||
                         typeSymbol.Name == bannedCall.Type;
        bool methodMatch = string.IsNullOrEmpty(bannedCall.Method) ||
                           methodSymbol.Name == bannedCall.Method;

        return namespaceMatch && typeMatch && methodMatch;
    }
}