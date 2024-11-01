using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Text;
using Debug = UnityEngine.Debug;
public static class DebugFileSaving
{

    public static void SaveCSharp(string code, string outputPath)
    {
        // Write the transformed code to a .cs file
        File.WriteAllText(outputPath, code);
        Debug.Log($"Transformed code written to {outputPath}");
    }

    public static void SaveSyntaxTree(SyntaxTree syntaxTree, string syntaxTreePath)
    {
        // Get the root of the syntax tree
        var root = syntaxTree.GetRoot();

        // Build the tree representation
        var syntaxTreeRepresentation = BuildSyntaxTreeRepresentation(root, 0);

        // Write the syntax tree to a text file
        File.WriteAllText(syntaxTreePath, syntaxTreeRepresentation);
        Debug.Log($"Transformed code written to {syntaxTreePath}");
    }

    private static string BuildSyntaxTreeRepresentation(SyntaxNode node, int indentLevel)
    {
        var sb = new StringBuilder();
        string indent = new string(' ', indentLevel * 4); // 2 spaces for each indent level

        // Append the node type, its details, and corresponding code
        sb.AppendLine($"{indent}{node.GetType().Name} ({node.Kind()})");
        sb.AppendLine($"{indent}Code: {node.ToFullString().Trim()}");

        // Recursively process child nodes
        foreach (var child in node.ChildNodes())
        {
            sb.Append(BuildSyntaxTreeRepresentation(child, indentLevel + 1));
        }

        return sb.ToString();
    }
}
