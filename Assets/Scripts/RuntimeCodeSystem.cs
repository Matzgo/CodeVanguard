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
    /// The run code primerButton.
    /// </summary>
    [SerializeField]
    private Button _resetButton;
    [SerializeField]
    private Button _runButton;
    [SerializeField]
    private Button _approveButton;
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
        _approveButton.onClick.AddListener(OnGradeClicked);
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
        try
        {
            if (ShouldRecompileCode())
            {
                CompileAndRunUserCode();
                CompileAndRunSolutionCode();
            }
            else
            {
                RunSimpleAddTest();
            }
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private void OnGradeClicked()
    {
        try
        {
            // Ensure code is compiled before grading
            if (_userScript == null || _solutionScript == null)
            {
                RuntimeManager.Instance.Console.LogError("Please run the code first before grading.");
                return;
            }

            // Perform grading
            EvaluateSubmission();
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private bool ShouldRecompileCode()
    {
        // The || true is a workaround to make this run every time
        return (_activeCSharpSource != _codeEditorWindow.Text || _userScript == null) || true;
    }

    private void CompileAndRunUserCode()
    {
        string userCode = _codeEditorWindow.Text;
        string userCodeWithDirectives = _usingStatements + "\n" + userCode;

        ScriptType userType = CompileCode(userCodeWithDirectives, _domain);
        if (userType == null) return;

        if (!ValidateCodeSecurity(userCodeWithDirectives)) return;

        _userScript = userType.CreateInstance();
        _activeCSharpSource = userCode;

        SaveSyntaxTree(userCodeWithDirectives);
        RunSimpleAddTest();
    }

    private void CompileAndRunSolutionCode()
    {
        string solutionWithAddedDirectives = _usingStatements + "\n" + _solutionCode;

        ScriptType solutionType = CompileCode(solutionWithAddedDirectives, _domain);
        if (solutionType == null) return;

        if (!ValidateCodeSecurity(solutionWithAddedDirectives)) return;

        _solutionScript = solutionType.CreateInstance();
    }


    private ScriptType CompileCode(string codeWithDirectives, ScriptDomain domain)
    {
        ScriptType compiledType = domain.CompileAndLoadMainSource(
            codeWithDirectives,
            ScriptSecurityMode.UseSettings,
            assemblyReferences
        );

        if (compiledType == null)
        {
            HandleCompilationErrors(domain);
            return null;
        }

        return compiledType;
    }

    private bool ValidateCodeSecurity(string codeWithDirectives)
    {
        // Check for banned calls
        var bannedCalls = _bannedCallsDetector.GetBannedCalls(codeWithDirectives);
        if (bannedCalls.Count > 0)
        {
            foreach (var call in bannedCalls)
            {
                RuntimeManager.Instance.Console.LogError($"SECURITY FAILED: {call}");
            }
            return false;
        }

        return true;
    }

    private void SaveSyntaxTree(string codeWithDirectives)
    {
        var userSyntaxTree = CSharpSyntaxTree.ParseText(codeWithDirectives);
        var p = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath);
        DebugFileSaving.SaveSyntaxTree(userSyntaxTree, $"{p}/{outputSyntaxTreeName}");
    }

    private void RunSimpleAddTest()
    {
        RuntimeManager.Instance.Console.SetActive(true);

        var a = 5;
        var b = 3;
        int result = (int)_userScript.Call("Add", a, b);

        Color resultColor = result == 8 ? Color.green : new Color(1, 140f / 255f, 0);
        RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", resultColor);
    }

    private void EvaluateSubmission()
    {
        // Validate scripts are not null before grading
        if (_userScript == null)
        {
            RuntimeManager.Instance.Console.LogError("User script is not compiled. Run the code first.");
            return;
        }

        if (_solutionScript == null)
        {
            RuntimeManager.Instance.Console.LogError("Solution script is not compiled. Run the solution first.");
            return;
        }

        var gradingResult = _codeGradingSystem.GradeSubmission(
            _usingStatements + "\n" + _codeEditorWindow.Text,
            _userScript,
            _solutionScript
        );

        LogGradingResults(gradingResult);
    }

    private void LogGradingResults(CodeGradingSystem.GradingResult gradingResult)
    {
        RuntimeManager.Instance.Console.WriteLine("=== Code Evaluation Results ===", Color.green);
        RuntimeManager.Instance.Console.WriteLine($"Performance Score: {gradingResult.PerformanceScore:F1}%", Color.green);
        RuntimeManager.Instance.Console.WriteLine($"Memory Score: {gradingResult.MemoryScore:F1}%", Color.green);
        RuntimeManager.Instance.Console.WriteLine($"Naming Score: {gradingResult.NamingScore:F1}%", Color.green);
        RuntimeManager.Instance.Console.WriteLine($"Total Score: {gradingResult.TotalScore:F1}%", Color.green);

        if (gradingResult.Feedback.Count > 0)
        {
            RuntimeManager.Instance.Console.WriteLine("\nFeedback:", Color.yellow);
            foreach (var feedback in gradingResult.Feedback)
            {
                RuntimeManager.Instance.Console.WriteLine($"- {feedback}", Color.yellow);
            }
        }
    }

    private void TransformAndRunCoroutine()
    {
        if (_coroutineDisabled)
            return;

        var userCode = _codeEditorWindow.Text;
        var userCodeWithCoroutineDirectives = _coroutineUsingStatements + "\n" + userCode;

        _coroutineCode = _coroutineTransformer.TransformToCoroutine(userCodeWithCoroutineDirectives);
        SaveCoroutineCode(_coroutineCode);

        ScriptType coroutineType = CompileCoroutine(_coroutineCode);
        if (coroutineType == null) return;

        var transformedActiveScript = coroutineType.CreateInstance();
        StartTransformedCoroutine(transformedActiveScript, "COR_Add", 5, 3);
    }

    private void SaveCoroutineCode(string coroutineCode)
    {
        var p = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath);
        DebugFileSaving.SaveCSharp(coroutineCode, $"{p}/{outputCSFileName}");
    }

    private ScriptType CompileCoroutine(string coroutineCode)
    {
        var refs = coroutineReferences.ToList();
        refs.AddRange(assemblyReferences.ToList());

        ScriptType coroutineType = _coroutineDomain.CompileAndLoadMainSource(
            coroutineCode,
            ScriptSecurityMode.EnsureLoad,
            refs.ToArray()
        );

        if (coroutineType == null)
        {
            HandleCompilationErrors(_coroutineDomain, _couroutineLineOffset);
            return null;
        }

        return coroutineType;
    }

    private void HandleCompilationErrors(ScriptDomain domain, int lineOffset = 0)
    {
        if (domain.RoslynCompilerService.LastCompileResult.Success == false)
        {
            RuntimeManager.Instance.Console.LogErrors(
                domain.RoslynCompilerService.LastCompileResult.Errors,
                lineOffset
            );
        }
        else if (domain.SecurityResult.IsSecurityVerified == false)
        {
            RuntimeManager.Instance.Console.LogError(
                "SECURITY FAILED: " + domain.SecurityResult.GetAllText(true)
            );
        }
    }

    private void HandleException(Exception e)
    {
        Debug.LogError(e.ToString());
        var exc = e.InnerException ?? e;
        RuntimeManager.Instance.Console.LogError($"{exc.Message}");
        RuntimeManager.Instance.Console.LogError($"{exc.StackTrace}");
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

    // Optional: If you want to add a method to reset the scripts
    private void ResetScripts()
    {
        _userScript = null;
        _solutionScript = null;
        _activeCSharpSource = null;
        RuntimeManager.Instance.Console.WriteLine("Scripts reset. Please run the code again.", Color.yellow);
    }

}
