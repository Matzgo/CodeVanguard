using CodeInspector;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class GradingResult
{
    public bool Correct { get; set; }
    public float PerformanceScore { get; set; }
    public float MemoryScore { get; set; }
    public float NamingScore { get; set; }
    public List<string> Feedback { get; set; }
    public float TotalScore => (PerformanceScore + MemoryScore + NamingScore) / 3;

}
public class CodeGradingSystem : MonoBehaviour
{


    [SerializeField] private int maxExecutionTime = 1000; // milliseconds
    [SerializeField] private long maxMemoryUsage = 5242880; // bytes

    private string goalSolution;

    [SerializeField]
    private int _testCallCount;
    private string _entryPointMethodName;

    public GradingResult GradeSubmission(
        string userCode,
        ScriptProxy userScriptProxy,
        ScriptProxy goalScriptProxy,
        string entryPointMethodName)
    {

        _entryPointMethodName = entryPointMethodName;
        var result = new GradingResult
        {
            Feedback = new List<string>()
        };
        // Correctness grading
        //var (correctnessScore, correctnessFeedback) = GradeCorrectness(userScriptProxy, goalScriptProxy);
        //result.Feedback.AddRange(correctnessFeedback);
        //if (correctnessScore == false) // If correctness fails, skip performance and memory grading
        //{
        //    result.Correct = false;
        //    result.PerformanceScore = 0;
        //    result.MemoryScore = 0;
        //    result.NamingScore = 0;
        //    return result;
        //}
        //else
        //{
        //    result.Correct = true;
        //}

        // Performance grading
        var (performanceScore, performanceFeedback, avg) = GradePerformance(userScriptProxy, goalScriptProxy);
        result.PerformanceScore = performanceScore;
        result.Feedback.Add($"Average runtime: {avg}");
        result.Feedback.AddRange(performanceFeedback);

        // Memory grading
        var (memoryScore, memoryFeedback) = GradeMemory(userCode);
        result.MemoryScore = memoryScore;
        result.Feedback.AddRange(memoryFeedback);

        // Naming convention grading
        var (namingScore, namingFeedback) = GradeNaming(userCode);
        result.NamingScore = namingScore;
        result.Feedback.AddRange(namingFeedback);

        return result;
    }
    //private (bool correct, List<string> feedback) GradeCorrectness(ScriptProxy userProxy, ScriptProxy goalProxy)
    //{
    //    var feedback = new List<string>();
    //    bool isCorrect = true;

    //    try
    //    {
    //        // Test cases for the Add method
    //        object[][] testCases = new object[][]
    //        {
    //            new object[] { 5, 3 },
    //            new object[] { 10, 20 },
    //            new object[] { 0, 0 },
    //            new object[] { -5, 5 },
    //            new object[] { 1000, 1000 },
    //            new object[] { -1000, -1000 }
    //        };

    //        foreach (var testCase in testCases)
    //        {
    //            var userResult = userProxy.Call("Add", testCase[0], testCase[1]);
    //            var goalResult = goalProxy.Call("Add", testCase[0], testCase[1]);

    //            // Compare results
    //            if (!userResult.Equals(goalResult))
    //            {
    //                isCorrect = false;
    //                feedback.Add($"Test case Add({testCase[0]}, {testCase[1]}) produced incorrect result: {userResult} (expected {goalResult})");
    //                return (isCorrect, feedback);
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        isCorrect = false;
    //        feedback.Add($"Runtime error during correctness testing: {e.Message}");
    //    }

    //    return (isCorrect, feedback);
    //}
    private (float score, List<string> feedback, double avgTime) GradePerformance(ScriptProxy userProxy, ScriptProxy goalProxy)
    {
        RuntimeManager.Instance.Console.SetActive(false);

        var feedback = new List<string>();
        float score = 100;
        double totalUserTime = 0;
        double totalGoalTime = 0;
        int testCaseCount = 0;

        //warmup calls
        //userProxy.Call("Add", 1, 1);
        //goalProxy.Call("Add", 1, 1);

        userProxy.Call(_entryPointMethodName);
        goalProxy.Call(_entryPointMethodName);

        try
        {
            // Test cases for the Add method
            //object[][] testCases = new object[][]
            //{
            //new object[] { 5, 3 },
            //new object[] { 10, 20 },
            //new object[] { 0, 0 },
            //new object[] { -5, 5 },
            //new object[] { 1000, 1000 },
            //new object[] { -1000, -1000 }
            //};

            for (int i = 0; i < _testCallCount; i++)
            {

                //foreach (var testCase in testCases)
                //{
                testCaseCount++;
                var sw = Stopwatch.StartNew();
                //var userResult = userProxy.Call("Add", testCase[0], testCase[1]);
                var userResult = userProxy.Call(_entryPointMethodName);
                var userTime = sw.ElapsedTicks;
                var userTimeMS = sw.ElapsedMilliseconds;
                totalUserTime += userTime;
                sw.Restart();
                var goalResult = goalProxy.Call(_entryPointMethodName);
                var goalTime = sw.ElapsedTicks;
                totalGoalTime += goalTime;


                // Check execution time
                if (userTimeMS > maxExecutionTime)
                {
                    feedback.Add($"Exceeded max Execution time: {userTimeMS}/{maxExecutionTime}");
                    RuntimeManager.Instance.Console.SetActive(true);
                    return (0, feedback, maxExecutionTime);
                }


                //}
            }
        }
        catch (System.Exception e)
        {
            score = 0;
            feedback.Add($"Runtime error during performance testing: {e.Message}");
        }

        // Calculate the average time for the user's method
        double averageUserTime = totalUserTime / testCaseCount; /// testCaseCount;
        double averageSolTime = totalGoalTime / testCaseCount; /// testCaseCount;
        feedback.Add($"USER: {averageUserTime} | SOL: {averageSolTime} | USER/SOL: {averageUserTime / averageSolTime}");

        // Compare with goal solution time
        if (averageUserTime > averageSolTime * 1.4)
        {
            // Calculate the ratio of userTime to solTime
            double ratio = averageUserTime / averageSolTime;

            // Apply an increasing penalty based on how much larger the ratio is
            double penalty = Math.Pow(ratio - 1.4, 2) + .25;  // Squared to increase the penalty exponentially

            // Reduce the score based on the penalty, with a minimum score of 50%
            score *= (float)Math.Max(0, 1 - penalty);

            // Optionally, add feedback about the performance
            // feedback.Add($"Test case Add({testCase[0]}, {testCase[1]}) was significantly slower than optimal solution");
        }
        RuntimeManager.Instance.Console.SetActive(true);

        return (score, feedback, averageUserTime);
    }

    private (float score, List<string> feedback) GradeMemory(string userCode)
    {
        var feedback = new List<string>();
        float score = 100;

        try
        {
            var tree = CSharpSyntaxTree.ParseText(userCode);
            var root = tree.GetRoot();

            // Check for unnecessary variables
            var variables = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>();

            int unusedVarCount = 0;
            foreach (var variable in variables)
            {
                foreach (var declarator in variable.Variables)
                {
                    // Get the variable name
                    var variableName = declarator.Identifier.Text;

                    // Check if the variable is used
                    var references = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id => id.Identifier.Text == variableName)
                        .ToList();

                    // If only declared and referenced once in a return statement, consider it "used"
                    if (references.Count == 1)
                    {
                        var parentStatement = references.First().Parent;
                        if (parentStatement is ReturnStatementSyntax)
                        {
                            continue; // Variable is used directly in a return statement, so we skip it
                        }
                    }
                    else if (references.Count <= 1)
                    {
                        // If not found or only declared, it's considered unused
                        unusedVarCount++;
                    }
                }
            }

            if (unusedVarCount > 0)
            {
                score *= 0.95f;
                feedback.Add($"Found {unusedVarCount} potentially unused variables");
            }

            // Check for unnecessary object creation
            var objectCreations = root.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .Count();

            if (objectCreations > 0)
            {
                //score *= 0.9f;
                //feedback.Add("The Add method shouldn't require object creation");
            }

            // Check for array allocations
            var arrayCreations = root.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>()
                .Count();

            if (arrayCreations > 0)
            {
                //score *= 0.9f;
                //feedback.Add("The Add method shouldn't require array allocation");
            }
        }
        catch (System.Exception e)
        {
            feedback.Add($"Error during memory analysis: {e.Message}");
        }

        return (score, feedback);
    }

    private (float score, List<string> feedback) GradeNaming(string userCode)
    {
        var feedback = new List<string>();
        float score = 100;

        try
        {
            var tree = CSharpSyntaxTree.ParseText(userCode);
            var root = tree.GetRoot();

            // Check variable naming
            var variables = root.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>();

            foreach (var variable in variables)
            {
                string name = variable.Identifier.Text;

                // Check for single-letter variables (except loop counters)
                if (name.Length == 1 && name != "i" && name != "j" && name != "k" && name != "x" && name != "y" && name != "z" && name != "a" && name != "b" && name != "c" && name != "d")
                {
                    score *= 0.95f;
                    feedback.Add($"Variable '{name}' should have a more descriptive name");
                }

                // Check for proper casing (camelCase for variables)
                if (char.IsUpper(name[0]))
                {
                    score *= 0.95f;
                    feedback.Add($"Variable '{name}' should start with lowercase letter");
                }

                // Check for numeric suffixes
                if (System.Text.RegularExpressions.Regex.IsMatch(name, @"\d+$"))
                {
                    score *= 0.95f;
                    feedback.Add($"Variable '{name}' uses a numeric suffix - consider a more descriptive name");
                }
            }

            // Check method naming (although Add is predefined in this case)
            var methods = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                string name = method.Identifier.Text;
                if (!char.IsUpper(name[0]))
                {
                    score *= 0.95f;
                    feedback.Add($"Method '{name}' should start with uppercase letter");
                }
            }
        }
        catch (System.Exception e)
        {
            feedback.Add($"Error during naming analysis: {e.Message}");
        }

        return (score, feedback);
    }
}