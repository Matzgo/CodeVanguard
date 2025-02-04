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
    private Button _simulateButton;
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
    private string _entryPointMethodName;
    private ScriptProxy _solutionScript;
    private CSFileSet _lastTask;
    CSFileSet _currentTask;
    private Scenario _scenario;
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
    private bool _codeRunning;
    public bool CodeRunning => _codeRunning;
    private string _compiledUserCode;
    private string _compiledSolutionCode;

    private void Awake()
    {
        _simulateButton.onClick.AddListener(OnSimulateClicked);
        _resetButton.onClick.AddListener(OnResetClicked);
        //_runButton.onClick.AddListener(OnRunClicked);
    }

    private void OnResetClicked()
    {
        var before = (_currentTask.EntryPointFile as EditableCSFile).BeforeText;
        var after = (_currentTask.EntryPointFile as EditableCSFile).AfterText;
        var input = (_currentTask.EntryPointFile as EditableCSFile).Input;
        _codeEditorWindow.SetText(before, input, after);


        _fileNameText.text = _currentTask.EntryPointFile.FileName + ".cs";
        _solutionCode = _currentTask.CSFiles[0].SolutionFile.Text;
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


        _usingStatements = GenerateUsingStatements(_autoUsingDirectives);
        _coroutineUsingStatements = GenerateUsingStatements(_coroutineDirectives) + _usingStatements;
        _lineOffsetCount = GetLineCount(_usingStatements);
        _couroutineLineOffset = GetLineCount(_coroutineUsingStatements);
        _coroutineTransformer.Initialize(_couroutineLineOffset);
    }

    internal void LoadTask(CSFileSet task)
    {
        _lastTask = _currentTask;
        _currentTask = task;
        if (_scenario != null)
            _scenario.RunButton.ClearRegisteredAction();
        _scenario = ScenarioManager.Instance.GetScenario(task.ScenarioType);

        _scenario.RunButton.RegisterAction(OnRunClicked);

        var before = (_currentTask.EntryPointFile as EditableCSFile).BeforeText;
        var after = (_currentTask.EntryPointFile as EditableCSFile).AfterText;

        var input = (_currentTask.EntryPointFile as EditableCSFile).Input;
        // Only update input if the last task was different
        if (_lastTask == null || _lastTask != task)
            _codeEditorWindow.SetText(before, input, after);
        _fileNameText.text = task.EntryPointFile.FileName + ".cs";
        _solutionCode = task.CSFiles[0].SolutionFile.Text;
        CodeVanguardManager.Instance.LoadFeedback(new List<string> { task.OnPluggedInFeedbackKey });
        _entryPointMethodName = task.EntryPoint;

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


    private void OnSimulateClicked()
    {
        RuntimeManager.Instance.DisableWorldGame();
        CompileAndRun();

    }
    public void OnRunCompleted()
    {
        GradingResult res = null;
        if (_userScript != null)
            res = EvaluateSubmission();

        CodeVanguardManager.Instance.EndTask(res);

        _codeRunning = false;
        RuntimeManager.Instance.ResetStartButtons();
        RuntimeManager.Instance.CoroutineRunner.OnCoroutineFinshed -= OnRunCompleted;

    }

    private void OnRunClicked()
    {
        try
        {
            RuntimeManager.Instance.Console.SetActive(false);
            _coroutineDisabled = true;
            RuntimeManager.Instance.DisableWorldGame();
            CompileAndRun();
            RuntimeManager.Instance.EnableWorldGame();
            _coroutineDisabled = false;
            RuntimeManager.Instance.Console.SetActive(true);
            RuntimeManager.Instance.ResetWorld();

            RuntimeManager.Instance.CoroutineRunner.OnCoroutineFinshed += OnRunCompleted;
            TransformAndRunCoroutine(true);

            _codeRunning = true;

            //After code done:

            //GradingResult res = null;
            //if (_userScript != null)
            //    res = EvaluateSubmission();

            //CodeVanguardManager.Instance.EndTask(res);


        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private void CompileAndRun()
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
                //RuntimeManager.Instance.ResetMiniGame();
                RuntimeManager.Instance.ResetWorld();
                RunVoidMethodTest();
            }
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
        RuntimeManager.Instance.Console.SetActive(true);
        string userCode = _codeEditorWindow.Text;
        string userCodeWithDirectives = _usingStatements + "\n" + userCode;

        _compiledUserCode = userCodeWithDirectives;
        ScriptType userType = CompileCode(_compiledUserCode, _domain);
        if (userType == null) return;

        if (!ValidateCodeSecurity(_compiledUserCode)) return;

        _userScript = userType.CreateInstance();
        _activeCSharpSource = userCode;

        SaveSyntaxTree(_compiledUserCode);

        //RuntimeManager.Instance.ResetMiniGame();
        RuntimeManager.Instance.ResetWorld();
        RunVoidMethodTest();
        Debug.Log(RuntimeManager.Instance.IsWorldGameOn);
        Debug.Log(RuntimeManager.Instance.IsSimOn);
        TransformAndRunCoroutine(false);
    }

    private void CompileAndRunSolutionCode()
    {
        string solutionWithAddedDirectives = _usingStatements + "\n" + _solutionCode;
        _compiledSolutionCode = solutionWithAddedDirectives;
        ScriptType solutionType = CompileCode(_compiledSolutionCode, _domain);
        if (solutionType == null) return;

        if (!ValidateCodeSecurity(_compiledSolutionCode)) return;

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
            HandleCompilationErrors(domain, _lineOffsetCount);
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


    private void RunVoidMethodTest()
    {
        var p = RuntimeManager.Instance.IsWorldGameOn;

        //RuntimeManager.Instance.DisableMiniGame();
        RuntimeManager.Instance.DisableWorldGame();
        RuntimeManager.Instance.Console.SetActive(false);

        _userScript.Call(_entryPointMethodName);
        //RuntimeManager.Instance.EnableMiniGame();
        if (p)
            RuntimeManager.Instance.EnableWorldGame();


        RuntimeManager.Instance.Console.SetActive(true);

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

    private GradingResult EvaluateSubmission()
    {
        // Validate scripts are not null before grading
        if (_userScript == null)
        {
            RuntimeManager.Instance.Console.LogError("User script is not compiled. Run the code first.");
            return null;
        }
        if (_solutionScript == null)
        {
            RuntimeManager.Instance.Console.LogError("Solution script is not compiled. Run the solution first.");
            return null;
        }
        RuntimeManager.Instance.ResetWorld();
        RuntimeManager.Instance.EnableWorldGame();
        RuntimeManager.Instance.EnableWorldSimulator();
        _userScript.Call(_entryPointMethodName);


        (bool correct, var feedback, var feedbackKeys) = _scenario.CheckCorrectness();



        List<string> strucFeedback = new List<string>();
        if (!correct)
        {
            _codeGradingSystem.StructureFeedback(_compiledUserCode, _compiledSolutionCode, strucFeedback, _currentTask.StructureFeedbackTypes);
        }
        if (strucFeedback.Count > 0)
            feedback.AddRange(strucFeedback);


        var gradingResult = _codeGradingSystem.GradeSubmission(
            correct,
            _compiledUserCode,
            _compiledSolutionCode,
            _userScript,
            _solutionScript, _entryPointMethodName, _currentTask.ScenarioType
        );
        RuntimeManager.Instance.DisableWorldSimulator();

        gradingResult.Feedback.AddRange(feedback);

        feedbackKeys.AddRange(gradingResult.FeedbackKeys);
        gradingResult.FeedbackKeys = feedbackKeys;
        gradingResult.FeedbackKeys.Insert(0, "GEN_Analyzing");
        gradingResult.FeedbackKeys.Add("GEN_Details");
        gradingResult.Feedback.Insert(0, $"Maintainability Score: {gradingResult.NamingScore}");
        gradingResult.Feedback.Insert(0, $"Memory Score: {gradingResult.MemoryScore}");
        gradingResult.Feedback.Insert(0, $"Performance Score: {gradingResult.PerformanceScore}");

        LogGradingResults(gradingResult);
        return gradingResult;
    }

    private void LogGradingResults(GradingResult gradingResult)
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

    private void TransformAndRunCoroutine(bool useScenarioTickTime)
    {
        if (_coroutineDisabled)
            return;

        var userCode = _codeEditorWindow.Text;
        var userCodeWithCoroutineDirectives = _coroutineUsingStatements + "\n" + userCode;

        if (useScenarioTickTime)
        {
            _coroutineCode = _coroutineTransformer.TransformToCoroutine(userCodeWithCoroutineDirectives, _scenario.ScenarioTickTime);

        }
        else
        {
            _coroutineCode = _coroutineTransformer.TransformToCoroutine(userCodeWithCoroutineDirectives);

        }
        SaveCoroutineCode(_coroutineCode);

        ScriptType coroutineType = CompileCoroutine(_coroutineCode);
        if (coroutineType == null) return;

        var transformedActiveScript = coroutineType.CreateInstance();
        //StartTransformedCoroutine(transformedActiveScript, "COR_Add", 5, 3);
        StartTransformedCoroutine(transformedActiveScript, "COR_" + _entryPointMethodName);
    }

    private void SaveCoroutineCode(string coroutineCode)
    {
        var p = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath);
        DebugFileSaving.SaveCSharp(coroutineCode, $"{p}/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{outputCSFileName}");
    }

    private void SaveSyntaxTree(string codeWithDirectives)
    {
        var userSyntaxTree = CSharpSyntaxTree.ParseText(codeWithDirectives);
        var p = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath);
        DebugFileSaving.SaveSyntaxTree(userSyntaxTree, $"{p}/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{outputSyntaxTreeName}");
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
        if (e == null)
            return;
        Debug.LogError(e.ToString());
        var exc = e.InnerException ?? e;
        RuntimeManager.Instance.Console.LogError($"{exc.Message}");
        RuntimeManager.Instance.Console.LogError($"{exc.StackTrace}");
    }


    private void StopCoroutines(ScriptProxy proxy)
    {

        try
        {
            proxy.Call("StopCoroutinesMethod");
        }
        catch (Exception e)
        {
            RuntimeManager.Instance.Console.LogError("Failed to stop coroutines: " + e.Message);
        }
    }

    private void StartTransformedCoroutine(ScriptProxy proxy, string coroutineName, params object[] args)
    {
        StopCoroutines(proxy);

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
