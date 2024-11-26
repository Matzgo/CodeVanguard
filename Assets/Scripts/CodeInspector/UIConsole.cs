using RoslynCSharp.Compiler;
using System.Text;
using TMPro;
using UnityEngine;

public class UIConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    private StringBuilder outputBuilder = new StringBuilder();
    private bool _active;

    private void Awake()
    {
        outputBuilder = new StringBuilder();
    }

    public void Write(string message)
    {
        if (!_active) return;  // Early return if not active
        outputBuilder.Append(message);
        UpdateConsoleText();
    }

    public void WriteLine(string message)
    {
        if (!_active) return;  // Early return if not active
        outputBuilder.AppendLine(message);
        UpdateConsoleText();
    }

    public void WriteLine(string message, Color color)
    {
        if (!_active) return;  // Early return if not active
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
        //if (!_active) return;  // Early return if not active
        outputBuilder.Clear();
        UpdateConsoleText();
    }

    public void LogErrors(CompilationError[] errors, int lineOffset)
    {
        if (!_active) return;  // Early return if not active
        foreach (var error in errors)
        {
            LogError(error, lineOffset);
        }
    }

    public void LogErrors(CompilationError[] errors)
    {
        if (!_active) return;  // Early return if not active
        foreach (var error in errors)
        {
            LogError(error);
        }
    }

    public void LogError(CompilationError error)
    {
        if (!_active) return;  // Early return if not active
        // Assuming CompilationError has properties for line, column, and message
        int line = error.SourceLine;      // Replace with actual property
        int column = error.SourceColumn;   // Replace with actual property
        string message = error.Message; // Replace with actual property

        WriteLine($"<color=red>Line: {line} Col: {column}: {message}</color>");
    }

    public void LogError(CompilationError error, int lineOffset)
    {
        if (!_active) return;  // Early return if not active
        // Assuming CompilationError has properties for line, column, and message
        int line = error.SourceLine;      // Replace with actual property
        int column = error.SourceColumn;   // Replace with actual property
        string message = error.Message; // Replace with actual property
        line = line - lineOffset;
        WriteLine($"<color=red>Line: {line} Col: {column}: {message}</color>");
    }

    public void LogError(string error)
    {
        if (!_active) return;  // Early return if not active
        WriteLine($"<color=red>{error}</color>");
    }

    public void SetActive(bool b)
    {
        _active = b;
    }
}
