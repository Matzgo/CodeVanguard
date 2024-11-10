using CodeInspector;
using Microsoft.CodeAnalysis.CSharp;
using RoslynCSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuntimeCodeSystem : MonoBehaviour
{
    [SerializeField]
    CoroutineTransformer _coroutineTransformer;


    // Private
    private string _activeCSharpSource = null;
    private ScriptProxy _userScript = null;
    private ScriptDomain _domain = null;
    private ScriptDomain _coroutineDomain;


    /// <summary>
    /// The code editor window root game object.
    /// </summary>
    [SerializeField]
    private CustomCodeEditor _codeEditorWindow;
    [SerializeField]
    private CodeGradingSystem _codeGradingSystem;


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
    private AssemblyReferenceAsset[] coroutineReferences;

    [SerializeField]
    private List<string> _autoUsingDirectives = new List<string>();

    [SerializeField]
    private List<string> _coroutineDirectives = new List<string>();
    [SerializeField]
    BannedCallsDetector _bannedCallsDetector;

    [SerializeField]
    private string _solutionCode;

    private ScriptProxy _solutionScript;


    [SerializeField]
    CSFileSet _csFileSet;

    private string _usingStatements;
    private string _coroutineUsingStatements;
    private int _lineOffsetCount;
    private int _couroutineLineOffset;


    [SerializeField]
    [TextArea(minLines: 10, maxLines: 50)]
    string _coroutineCode;
    [SerializeField]
    string outputPath;
    string outputCSFileName = "transformedCode.cs";
    string outputSyntaxTreeName = "syntaxTree.txt";
    [SerializeField]
    private bool _coroutineDisabled;

    private void Awake()
    {
        _runButton.onClick.AddListener(OnRunClicked);
        _resetButton.onClick.AddListener(OnResetClicked);
    }

    private void OnResetClicked()
    {
        _codeEditorWindow.Text = _csFileSet.EntryPointFile.Text;
        _fileNameText.text = _csFileSet.EntryPointFile.FileName + ".cs";
        _solutionCode = _csFileSet.CSFiles[0].SolutionFile.Text;
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
        _domain = ScriptDomain.CreateDomain("Game", true);
        _coroutineDomain = ScriptDomain.CreateDomain("Coroutine", true);

        // Configure assembly references and security settings
        foreach (AssemblyReferenceAsset reference in assemblyReferences)
            _domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);

        // Configure assembly references and security settings
        foreach (AssemblyReferenceAsset reference in assemblyReferences)
            _coroutineDomain.RoslynCompilerService.ReferenceAssemblies.Add(reference);

        foreach (AssemblyReferenceAsset reference in coroutineReferences)
            _coroutineDomain.RoslynCompilerService.ReferenceAssemblies.Add(reference);

        _codeEditorWindow.Text = _csFileSet.EntryPointFile.Text;
        _fileNameText.text = _csFileSet.EntryPointFile.FileName + ".cs";
        _solutionCode = _csFileSet.CSFiles[0].SolutionFile.Text;

        _usingStatements = GenerateUsingStatements(_autoUsingDirectives);
        _coroutineUsingStatements = GenerateUsingStatements(_coroutineDirectives) + _usingStatements;
        _lineOffsetCount = GetLineCount(_usingStatements);
        _couroutineLineOffset = GetLineCount(_coroutineUsingStatements);
        _coroutineTransformer.Initialize(_couroutineLineOffset);
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
        string userCode = _codeEditorWindow.Text;

        if ((_activeCSharpSource != userCode || _userScript == null) || true)//false workaround to make this run every time
        {
            try
            {
                // Compile code
                var userCodeWithDirectives = _usingStatements + "\n" + userCode;

                ScriptType userType = _domain.CompileAndLoadMainSource(userCodeWithDirectives, ScriptSecurityMode.UseSettings, assemblyReferences);


                // Check for null
                if (userType == null)
                {
                    if (_domain.RoslynCompilerService.LastCompileResult.Success == false)
                    {
                        // Log compilation errors to the UIConsole
                        RuntimeManager.Instance.Console.LogErrors(_domain.RoslynCompilerService.LastCompileResult.Errors, _lineOffsetCount);
                        return; // Exit after logging the errors
                    }
                    else if (_domain.SecurityResult.IsSecurityVerified == false)
                    {
                        RuntimeManager.Instance.Console.LogError("SECURITY FAILED: " + _domain.SecurityResult.GetAllText(true));
                        return;
                    }
                }
                var bannedCalls = _bannedCallsDetector.GetBannedCalls(userCodeWithDirectives);
                if (bannedCalls.Count > 0)
                {
                    for (int i = 0; i < bannedCalls.Count; i++)
                    {
                        RuntimeManager.Instance.Console.LogError($"SECURITY FAILED: {bannedCalls[i]}");
                    }
                    return;
                }


                // Create an instance
                _userScript = userType.CreateInstance();
                _activeCSharpSource = userCode;


                var userSyntaxTree = CSharpSyntaxTree.ParseText(userCodeWithDirectives);
                var p = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath);
                DebugFileSaving.SaveSyntaxTree(userSyntaxTree, $"{p}/{outputSyntaxTreeName}");


                // Assuming the 'Add' method takes two integers, pass values and call it
                //int result = (int)_userScript.Call("Add", 5, 3);

                // Log the result to the console
                var a = 5;
                var b = 3;
                int result = (int)_userScript.Call("Add", a, b);
                if (result == 8)
                {
                    RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", Color.green);
                }
                else
                {
                    RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", new Color(1, 140f / 255f, 0));
                }



                //--- GOAL SOLUTION ----
                var solutionWithAddedDirectives = _usingStatements + "\n" + _solutionCode;


                ScriptType solutionType = _domain.CompileAndLoadMainSource(solutionWithAddedDirectives, ScriptSecurityMode.UseSettings, assemblyReferences);



                // Check for null
                if (solutionType == null)
                {
                    if (_domain.RoslynCompilerService.LastCompileResult.Success == false)
                    {
                        // Log compilation errors to the UIConsole
                        RuntimeManager.Instance.Console.LogErrors(_domain.RoslynCompilerService.LastCompileResult.Errors);
                        return; // Exit after logging the errors
                    }
                    else if (_domain.SecurityResult.IsSecurityVerified == false)
                    {
                        RuntimeManager.Instance.Console.LogError("SOLUTION SECURITY FAILED: " + _domain.SecurityResult.GetAllText(true));
                        return;
                    }
                }
                var solBannedCalls = _bannedCallsDetector.GetBannedCalls(solutionWithAddedDirectives);
                if (solBannedCalls.Count > 0)
                {
                    for (int i = 0; i < solBannedCalls.Count; i++)
                    {
                        RuntimeManager.Instance.Console.LogError($"SOLUTION SECURITY FAILED: {solBannedCalls[i]}");
                    }
                    return;
                }

                _solutionScript = solutionType.CreateInstance();

                //--- END GOAL SOLUTION -----------



                //GRADING
                var gradingResult = _codeGradingSystem.GradeSubmission(userCodeWithDirectives, _userScript, _solutionScript);
                // Log the results
                RuntimeManager.Instance.Console.WriteLine("=== Code Evaluation Results ===", Color.white);
                RuntimeManager.Instance.Console.WriteLine($"Performance Score: {gradingResult.PerformanceScore:F1}%", Color.cyan);
                RuntimeManager.Instance.Console.WriteLine($"Memory Score: {gradingResult.MemoryScore:F1}%", Color.cyan);
                RuntimeManager.Instance.Console.WriteLine($"Naming Score: {gradingResult.NamingScore:F1}%", Color.cyan);
                RuntimeManager.Instance.Console.WriteLine($"Total Score: {gradingResult.TotalScore:F1}%", Color.green);

                if (gradingResult.Feedback.Count > 0)
                {
                    RuntimeManager.Instance.Console.WriteLine("\nFeedback:", Color.yellow);
                    foreach (var feedback in gradingResult.Feedback)
                    {
                        RuntimeManager.Instance.Console.WriteLine($"- {feedback}", Color.yellow);
                    }
                }





                if (_coroutineDisabled)
                    return;


                var userCodeWithCoroutineDirectives = _coroutineUsingStatements + "\n" + userCode;
                _coroutineCode = _coroutineTransformer.TransformToCoroutine(userCodeWithCoroutineDirectives);
                DebugFileSaving.SaveCSharp(_coroutineCode, $"{p}/{outputCSFileName}");




                var refs = coroutineReferences.ToList();
                refs.AddRange(assemblyReferences.ToList());
                ScriptType coroutineType = _coroutineDomain.CompileAndLoadMainSource(_coroutineCode, ScriptSecurityMode.EnsureLoad, refs.ToArray());

                // Check for null
                if (coroutineType == null)
                {
                    if (_coroutineDomain.RoslynCompilerService.LastCompileResult.Success == false)
                    {
                        // Log compilation errors to the UIConsole
                        RuntimeManager.Instance.Console.LogErrors(_coroutineDomain.RoslynCompilerService.LastCompileResult.Errors, _couroutineLineOffset);
                        return; // Exit after logging the errors
                    }
                    else if (_coroutineDomain.SecurityResult.IsSecurityVerified == false)
                    {
                        RuntimeManager.Instance.Console.LogError("SECURITY FAILED: " + _coroutineDomain.SecurityResult.GetAllText(true));
                        return;
                    }
                }


                var tranformedActiveScript = coroutineType.CreateInstance();
                StartTransformedCoroutine(tranformedActiveScript, "COR_Add", 5, 3);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                var exc = e;
                if (e.InnerException != null)
                    exc = e.InnerException;
                RuntimeManager.Instance.Console.LogError($"{exc.Message}");
                RuntimeManager.Instance.Console.LogError($"{exc.StackTrace}");
            }
        }
        else
        {
            var a = 5;
            var b = 3;
            int result = (int)_userScript.Call("Add", a, b);
            if (result == 8)
            {
                RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", Color.green);
            }
            else
            {
                RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", new Color(1, 140f / 255f, 0));
            }
        }
    }


    private void StartTransformedCoroutine(ScriptProxy proxy, string coroutineName, params object[] args)
    {
        try
        {
            proxy.Call("RunCoroutineMethod", coroutineName, args);
        }
        catch (Exception e)
        {
            RuntimeManager.Instance.Console.LogError("Failed to start coroutine: " + e.Message);
        }
    }



}
