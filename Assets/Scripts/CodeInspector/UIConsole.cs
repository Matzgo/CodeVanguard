using RoslynCSharp.Compiler;
using System.Text;
using TMPro;
using UnityEngine;

public class UIConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    private StringBuilder outputBuilder = new StringBuilder();


    private void Awake()
    {
        outputBuilder = new StringBuilder();
    }

    public void Write(string message)
    {
        outputBuilder.Append(message);
        UpdateConsoleText();
    }

    public void WriteLine(string message)
    {
        outputBuilder.AppendLine(message);
        UpdateConsoleText();
    }
    public void WriteLine(string message, Color color)
    {
        // Convert the Color to a hex string
        string hexColor = ColorUtility.ToHtmlStringRGBA(color);

        // Add the message with color formatting using rich text
        outputBuilder.AppendLine($"<color=#{hexColor}>{message}</color>");

        // Update the console text
        UpdateConsoleText();
    }

    private void UpdateConsoleText()
    {
        consoleText.text = outputBuilder.ToString();
    }

    public void ClearConsole()
    {
        outputBuilder.Clear();
        UpdateConsoleText();
    }


    public void LogErrors(CompilationError[] errors, int lineOffset)
    {
        foreach (var error in errors)
        {
            LogError(error, lineOffset);
        }
    }

    public void LogErrors(CompilationError[] errors)
    {
        foreach (var error in errors)
        {
            LogError(error);
        }
    }

    public void LogError(CompilationError error)
    {
        // Assuming CompilationError has properties for line, column, and message
        int line = error.SourceLine;      // Replace with actual property
        int column = error.SourceColumn;   // Replace with actual property
        string message = error.Message; // Replace with actual property

        WriteLine($"<color=red>Line: {line} Col: {column}: {message}</color>");
    }
    public void LogError(CompilationError error, int lineOffset)
    {
        // Assuming CompilationError has properties for line, column, and message
        int line = error.SourceLine;      // Replace with actual property
        int column = error.SourceColumn;   // Replace with actual property
        string message = error.Message; // Replace with actual property
        line = line - lineOffset;
        WriteLine($"<color=red>Line: {line} Col: {column}: {message}</color>");
    }

    public void LogError(string error)
    {


        WriteLine($"<color=red>{error}</color>");
    }
}

