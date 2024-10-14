using CodeInspector;
using Game;
using InGameCodeEditor;
using RoslynCSharp;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuntimeCodeSystem : MonoBehaviour
{
    // Private
    private string _activeCSharpSource = null;
    private ScriptProxy _activeScript = null;
    private ScriptDomain _domain = null;


    /// <summary>
    /// The code editor window root game object.
    /// </summary>
    [SerializeField]
    private CustomCodeEditor _codeEditorWindow;


    /// <summary>
    /// The run code button.
    /// </summary>
    [SerializeField]
    private Button _resetButton;
    [SerializeField]
    private Button _runButton;
    [SerializeField]
    TextMeshProUGUI _fileNameText;
    [SerializeField]
    private AssemblyReferenceAsset[] assemblyReferences;

    [SerializeField]
    private List<string> _autoUsingDirectives = new List<string>();
    [SerializeField]
    GameCSFile _gameCSFile;
    [SerializeField]
    BannedCallsDetector _bannedCallsDetector;


    private string _usingStatements;
    private int _lineOffsetCount;

    private void Awake()
    {
        _runButton.onClick.AddListener(OnRunClicked);
        _resetButton.onClick.AddListener(OnResetClicked);
    }

    private void OnResetClicked()
    {
        _codeEditorWindow.Text = _gameCSFile.Text;
        _fileNameText.text = _gameCSFile.FileName;
    }
    private int GetLineCount(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return 0; // Return 0 if the code is null or empty
        }

        // Split the code into lines and count them
        return code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
    void Start()
    {
        // Create the _domain with restricted access to certain namespaces
        _domain = ScriptDomain.CreateDomain("MazeCrawlerCode", true);

        // Configure assembly references and security settings
        foreach (AssemblyReferenceAsset reference in assemblyReferences)
            _domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);


        _codeEditorWindow.Text = _gameCSFile.Text;
        _fileNameText.text = _gameCSFile.FileName + ".cs";

        _usingStatements = GenerateUsingStatements(_autoUsingDirectives);
        _lineOffsetCount = GetLineCount(_usingStatements);
    }

    private string GenerateUsingStatements(List<string> directives)
    {
        if (directives == null || directives.Count == 0)
        {
            return string.Empty; // No using directives to add
        }

        // Construct the using statements
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var directive in directives)
        {
            sb.AppendLine($"using {directive};");
        }

        return sb.ToString();
    }
    private void OnRunClicked()
    {
        string cSharpSource = _codeEditorWindow.Text;


        // Don't recompile the same code
        if (_activeCSharpSource != cSharpSource || _activeScript == null)
        {
            try
            {
                // Compile code
                var codeWithAddedDirectives = _usingStatements + "\n" + cSharpSource;

                ScriptType type = _domain.CompileAndLoadMainSource(codeWithAddedDirectives, ScriptSecurityMode.UseSettings, assemblyReferences);




                // Check for null
                if (type == null)
                {
                    if (_domain.RoslynCompilerService.LastCompileResult.Success == false)
                    {
                        // Log compilation errors to the UIConsole
                        RuntimeManager.Instance.Console.LogErrors(_domain.RoslynCompilerService.LastCompileResult.Errors, _lineOffsetCount);
                        return; // Exit after logging the errors
                    }
                    else if (_domain.SecurityResult.IsSecurityVerified == false)
                    {
                        RuntimeManager.Instance.Console.LogError("NOT ALLOWED");
                        return;
                    }
                }
                if (_bannedCallsDetector.ContainsBannedCalls(codeWithAddedDirectives))
                {
                    RuntimeManager.Instance.Console.LogError("BANNED CALL");
                    return;
                }


                // Create an instance
                _activeScript = type.CreateInstance();
                _activeCSharpSource = cSharpSource;

                _activeScript.Call("Main");
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
            }
        }
        else
        {
            _activeScript.Call("Main");
        }
    }



}
