using CodeInspector;
using Game;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCSharp;
using System;
using System.Collections.Generic;
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
    private ScriptProxy _activeScript = null;
    private ScriptDomain _domain = null;
    private ScriptDomain _coroutineDomain;


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
    private AssemblyReferenceAsset[] coroutineReferences;

    [SerializeField]
    private List<string> _autoUsingDirectives = new List<string>();

    [SerializeField]
    private List<string> _coroutineDirectives = new List<string>();

    [SerializeField]
    GameCSFile _gameCSFile;
    [SerializeField]
    BannedCallsDetector _bannedCallsDetector;


    private string _usingStatements;
    private string _coroutineUsingStatements;
    private int _lineOffsetCount;
    private int _couroutineLineOffset;
    [SerializeField]
    [TextArea(minLines: 10, maxLines: 50)]
    string _transformedCode;

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

        _codeEditorWindow.Text = _gameCSFile.Text;
        _fileNameText.text = _gameCSFile.FileName + ".cs";

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
        string cSharpSource = _codeEditorWindow.Text;

        if ((_activeCSharpSource != cSharpSource || _activeScript == null) || true)//false workaround to make this run every time
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
                        RuntimeManager.Instance.Console.LogError("SECURITY FAILED: " + _domain.SecurityResult.GetAllText(true));
                        return;
                    }
                }
                var bannedCalls = _bannedCallsDetector.GetBannedCalls(codeWithAddedDirectives);
                if (bannedCalls.Count > 0)
                {
                    for (int i = 0; i < bannedCalls.Count; i++)
                    {
                        RuntimeManager.Instance.Console.LogError($"SECURITY FAILED: {bannedCalls[i]}");
                    }
                    return;
                }


                // Create an instance
                _activeScript = type.CreateInstance();
                _activeCSharpSource = cSharpSource;




                // Assuming the 'Add' method takes two integers, pass values and call it
                //int result = (int)_activeScript.Call("Add", 5, 3);

                // Log the result to the console
                var a = 5;
                var b = 3;
                int result = (int)_activeScript.Call("Add", a, b);
                if (result == 8)
                {
                    RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", Color.green);
                }
                else
                {
                    RuntimeManager.Instance.Console.WriteLine($"Add({a}, {b}) = {result}", new Color(1, 140f / 255f, 0));
                }



                var codeWithCoroutineDirectives = _coroutineUsingStatements + "\n" + cSharpSource;


                _transformedCode = TransformToCoroutine(codeWithCoroutineDirectives);

                var refs = coroutineReferences.ToList();
                refs.AddRange(assemblyReferences.ToList());
                ScriptType tType = _coroutineDomain.CompileAndLoadMainSource(_transformedCode, ScriptSecurityMode.EnsureLoad, refs.ToArray());
                // Check for null
                if (tType == null)
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



                var tranformedActiveScript = tType.CreateInstance();
                StartTransformedCoroutine(tranformedActiveScript, "Add", 5, 3);
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
            int result = (int)_activeScript.Call("Add", a, b);
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

    private string TransformToCoroutine(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(m => m.Identifier.Text == "Add");

        if (method == null) return sourceCode;

        // Transform the method into a coroutine
        var newMethod = _coroutineTransformer.TransformToCoroutine(method);

        // Get the parent class of the method
        var parentClass = method.Parent as ClassDeclarationSyntax;
        if (parentClass == null) return sourceCode;

        // Add the transformed method back to the parent class
        var modifiedClass = parentClass.ReplaceNode(method, newMethod);

        // Add the RunCoroutineMethod to the class
        modifiedClass = _coroutineTransformer.AddRunCoroutineMethod(modifiedClass);

        // Replace the modified class in the root syntax tree
        var newRoot = root.ReplaceNode(parentClass, modifiedClass);

        return newRoot.NormalizeWhitespace().ToFullString();
    }

}
