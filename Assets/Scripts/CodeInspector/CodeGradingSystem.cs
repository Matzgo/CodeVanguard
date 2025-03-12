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

    public List<string> FeedbackKeys { get; set; }
    public float TotalScore => (PerformanceScore + MemoryScore + NamingScore) / 3;

}
public class CodeGradingSystem : MonoBehaviour
{


    [SerializeField] private int maxExecutionTime = 1000; // milliseconds
    [SerializeField] private long maxMemoryUsage = 5242880; // bytes

    private string goalSolution;

    [SerializeField]
    private int _testCallCount;
    private ScenarioType _scenarioType;
    private string _entryPointMethodName;

    public GradingResult GradeSubmission(
        bool correct,
        string userCode,
        string solutionCode,
        ScriptProxy userScriptProxy,
        ScriptProxy goalScriptProxy,
        string entryPointMethodName, ScenarioType scenario, CSFileSet task)
    {

        _scenarioType = scenario;
        _entryPointMethodName = entryPointMethodName;
        var result = new GradingResult
        {
            Feedback = new List<string>()

        };
        result.FeedbackKeys = new List<string>();


        //Correctness grading
        if (correct == false) // If correctness fails, skip performance and memory grading
        {
            result.Correct = false;
            result.PerformanceScore = 0;
            result.MemoryScore = 0;
            result.NamingScore = 0;
            return result;
        }
        else
        {
            result.Correct = true;
        }

        // Performance grading
        var (performanceScore, performanceFeedback, performanceFeedbackKeys, avg) = GradePerformance(userScriptProxy, goalScriptProxy);
        result.PerformanceScore = performanceScore;
        //result.Feedback.Add($"Average runtime: {avg}");
        result.Feedback.AddRange(performanceFeedback);
        result.FeedbackKeys.AddRange(performanceFeedbackKeys);
        // Memory grading
        var (memoryScore, memoryFeedback, memoryFeedbackKeys) = GradeMemory(userCode, solutionCode);
        result.MemoryScore = memoryScore;
        result.Feedback.AddRange(memoryFeedback);
        result.FeedbackKeys.AddRange(memoryFeedbackKeys);

        // Naming convention grading
        var (namingScore, namingFeedback, namingFeedbackKeys) = GradeNaming(userCode, solutionCode, task);
        result.NamingScore = namingScore;
        result.Feedback.AddRange(namingFeedback);
        result.FeedbackKeys.AddRange(namingFeedbackKeys);

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
    private (float score, List<string> feedback, List<string> feedbackKeys, double avgTime) GradePerformance(ScriptProxy userProxy, ScriptProxy goalProxy)
    {
        RuntimeManager.Instance.Console.SetActive(false);

        var feedback = new List<string>();
        var feedbackKeys = new List<string>();
        float score = 100;
        double totalUserTime = 0;
        double totalGoalTime = 0;
        int testCaseCount = 0;

        //warmup calls
        //userProxy.Call("Add", 1, 1);
        //goalProxy.Call("Add", 1, 1);
        ScenarioCodeCalls.DoTestCall(_scenarioType, userProxy);
        ScenarioCodeCalls.DoTestCall(_scenarioType, goalProxy);

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
                ScenarioCodeCalls.DoTestCall(_scenarioType, userProxy);
                var userTime = sw.ElapsedTicks;
                var userTimeMS = sw.ElapsedMilliseconds;
                totalUserTime += userTime;
                sw.Restart();
                ScenarioCodeCalls.DoTestCall(_scenarioType, goalProxy);
                var goalTime = sw.ElapsedTicks;
                totalGoalTime += goalTime;


                // Check execution time
                if (userTimeMS > maxExecutionTime)
                {
                    feedback.Add($"Exceeded max Execution time: {userTimeMS}/{maxExecutionTime}");
                    RuntimeManager.Instance.Console.SetActive(true);
                    return (0, feedback, feedbackKeys, maxExecutionTime);
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
        //feedback.Add($"USER: {averageUserTime} | SOL: {averageSolTime} | USER/SOL: {averageUserTime / averageSolTime}");
        feedback.Add($"RUNTIME: {averageUserTime}ms | Expected: {averageSolTime}ms");
        //feedback.Add($"Current Code is {averageSolTime / averageUserTime} faster than expected");

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

        if (score < 100f)
        {
            feedbackKeys.Add("PERF_Subopt");
            feedback.Add("Performance Suboptimal!");

        }
        else
        {
            //feedbackKeys.Add("PERF_Opt");
            //feedback.Add("Optimal Performance!");
        }


        RuntimeManager.Instance.Console.SetActive(true);




        return (score, feedback, feedbackKeys, averageUserTime);
    }

    private (float score, List<string> feedback, List<string> feedbackKeys) GradeMemory(string userCode, string solutionCode)
    {
        var feedback = new List<string>();
        var feedbackKeys = new List<string>();
        float score = 100f;

        try
        {
            // Parse both user and solution code
            var userTree = CSharpSyntaxTree.ParseText(userCode);
            var solutionTree = CSharpSyntaxTree.ParseText(solutionCode);

            // Count various memory allocation types
            // Count allocation types separately
            int CountObjectCreations(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Count();

            int CountArrayCreations(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Count();

            int CountVariableAllocations(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes()
                    .OfType<VariableDeclarationSyntax>()
                    .SelectMany(v => v.Variables)
                    .Count(v => v.Initializer != null);

            // Get allocation counts
            int userObjects = CountObjectCreations(userTree);
            int solutionObjects = CountObjectCreations(solutionTree);
            int userArrays = CountArrayCreations(userTree);
            int solutionArrays = CountArrayCreations(solutionTree);
            int userVars = CountVariableAllocations(userTree);
            int solutionVars = CountVariableAllocations(solutionTree);

            int userAllocations = userObjects + userArrays + userVars;
            int solutionAllocations = solutionObjects + solutionArrays + solutionVars;




            if (solutionAllocations == 0) // Check if the solution code has zero allocations
            {
                if (userAllocations == 0) // If the user's code also has zero allocations
                {
                    score = 100f; // Perfect score for matching the solution
                    //feedbackKeys.Add("MEM_EQUAL");
                    //feedback.Add("Memory usage matches expected pattern.");
                    //feedback.Add("No allocations detected. Matches expected behavior.");
                }
                else // User's code has allocations while the solution does not
                {
                    feedbackKeys.Add("MEM_Subopt");

                    score = Math.Max(0f, 100f / (1 + userAllocations)); // Penalize the score based on user allocations
                    feedback.Add("More memory allocations than expected! Improvements possible");
                }
            }
            else // The solution code has allocations
            {
                if (userAllocations == 0) // If the user's code has zero allocations
                {
                    score = 100f; // Score based on the number of solution allocations
                    //feedback.Add("Remarkable! Less memory usage than before!");
                    feedbackKeys.Add("MEM_Super");
                }
                else // User's code has allocations
                {
                    score = 100f * solutionAllocations / userAllocations; // Calculate score based on user vs. solution allocations

                    if (solutionAllocations == userAllocations)
                    {
                        //feedbackKeys.Add("MEM_EQUAL");
                        //feedback.Add("Memory usage matches expected pattern.");
                    }
                    else
                    {
                        feedbackKeys.Add("MEM_Subopt");

                        // Check if the user created more object allocations than the solution
                        if (userObjects > solutionObjects)
                        {
                            feedback.Add("More object allocations than required.");
                        }
                        else if (userObjects < solutionObjects) // User created fewer object allocations than the solution
                        {
                            //feedback.Add("Reduced object allocations. Potential optimization.");
                        }

                        // Check if the user created more array allocations than the solution
                        if (userArrays > solutionArrays)
                        {
                            feedback.Add("More array allocations than expected");
                        }
                        else if (userArrays < solutionArrays) // User created fewer array allocations than the solution
                        {
                            //feedback.Add("Memory storage refined: Fewer arrays in use. This may improve efficiency—ensure all necessary data is still accessible. Wohoo!");
                        }

                        // Check if the user created more variable allocations than the solution
                        if (userVars > solutionVars)
                        {
                            feedback.Add("More variables than expected.");
                        }
                        else if (userVars < solutionVars) // User created fewer variable allocations than the solution
                        {
                            //feedback.Add("Variable usage optimized: Fewer direct memory allocations. A more compact footprint—good for performance. Nice!");
                        }

                        // Compare total user allocations to solution allocations
                        if (userAllocations > solutionAllocations)
                        {
                            feedback.Add("Higher total memory allocations. Optimization possible.");
                        }
                        else if (userAllocations < solutionAllocations) // User has fewer total allocations than the solution
                        {
                            //feedback.Add("Memory footprint reduced. Fewer allocations without loss of stability! An efficient optimization. ");
                        }
                    }


                }

                score = Math.Max(0f, score); // Ensure the score is not negative
            }






            // Add detailed feedback
            //feedback.Add($"Solution Total Allocations: {solutionAllocations}");
            //feedback.Add($"User Total Allocations: {userAllocations}");
        }
        catch (Exception ex)
        {
            // If there's an error parsing, assume 0 score
            score = 0f;
            feedback.Add($"Error analyzing code: {ex.Message}");
        }

        return (score, feedback, feedbackKeys);
    }

    private (float score, List<string> feedback, List<string> feedbackKeys) GradeNaming(string userCode, string solutionCode, CSFileSet task)
    {
        var feedback = new List<string>();
        var feedbackKeys = new List<string>();
        float score = 100;

        try
        {
            var tree = CSharpSyntaxTree.ParseText(userCode);
            var root = tree.GetRoot();

            // Ensure OptimalNames list is not null
            HashSet<string> optimalNames = task.OptimalVariableNames != null ? new HashSet<string>(task.OptimalVariableNames) : new HashSet<string>();

            // Check variable naming
            var variables = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            foreach (var variable in variables)
            {
                string name = variable.Identifier.Text;

                // Check for single-letter variables (except common ones)
                if (name.Length == 1 && !new HashSet<string> { "i", "j", "k", "x", "y", "z", "a", "b", "c", "d" }.Contains(name))
                {
                    score *= 0.5f;
                    feedback.Add($"Variable '{name}' should have a more descriptive name.");
                }

                // Check for proper casing (camelCase for variables)
                if (char.IsUpper(name[0]))
                {
                    score *= 0.5f;
                    feedback.Add($"Variable '{name}' should start with a lowercase letter.");
                }

                // Check for numeric suffixes
                if (System.Text.RegularExpressions.Regex.IsMatch(name, @"\d+$"))
                {
                    score *= 0.5f;
                    feedback.Add($"Variable '{name}' uses a numeric suffix - consider a more descriptive name.");
                }

                // Check if variable name is not in the OptimalNames list
                if (optimalNames.Count > 0 && !optimalNames.Contains(name))
                {
                    score *= 0.5f; // Less harsh penalty compared to other naming issues
                    feedback.Add($"Variable '{name}' could have a name that makes its' purpose easier to understand");
                }
            }

            // Check method naming
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                string name = method.Identifier.Text;
                if (!char.IsUpper(name[0]))
                {
                    score *= 0.5f;
                    feedback.Add($"Method '{name}' should start with an uppercase letter.");
                }
            }

            // Check if user code is too long compared to the solution
            if (userCode.Length > 2.5 * solutionCode.Length)
            {
                score *= 0.5f;
                feedback.Add("Your code is significantly longer than necessary. Consider simplifying your implementation.");
            }
        }
        catch (System.Exception e)
        {
            feedback.Add($"Error during naming analysis: {e.Message}");
        }

        if (score < 100f)
        {
            feedbackKeys.Add("MAINTAIN_Subopt");
        }

        return (score, feedback, feedbackKeys);
    }




    public void StructureFeedback(string userCode, string solutionCode, List<string> feedback, StructureFeedbackTypes feedbackTypes)
    {
        try
        {
            // Parse both user and solution code
            var userTree = CSharpSyntaxTree.ParseText(userCode);
            var solutionTree = CSharpSyntaxTree.ParseText(solutionCode);

            // Count the number of for loops, while loops, if statements, switch statements, methods, and variables in both codes
            int CountForLoops(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<ForStatementSyntax>().Count();

            int CountWhileLoops(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<WhileStatementSyntax>().Count();

            int CountIfStatements(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<IfStatementSyntax>().Count();

            int CountSwitchStatements(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<SwitchStatementSyntax>().Count();

            int CountMethods(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count();

            int CountVariables(SyntaxTree tree) =>
                tree.GetRoot().DescendantNodes().OfType<VariableDeclarationSyntax>().Count();

            // Get counts for user and solution code
            int userForLoops = CountForLoops(userTree);
            int solutionForLoops = CountForLoops(solutionTree);
            int userWhileLoops = CountWhileLoops(userTree);
            int solutionWhileLoops = CountWhileLoops(solutionTree);
            int userIfStatements = CountIfStatements(userTree);
            int solutionIfStatements = CountIfStatements(solutionTree);
            int userSwitchStatements = CountSwitchStatements(userTree);
            int solutionSwitchStatements = CountSwitchStatements(solutionTree);
            int userMethods = CountMethods(userTree);
            int solutionMethods = CountMethods(solutionTree);
            int userVariables = CountVariables(userTree);
            int solutionVariables = CountVariables(solutionTree);

            // Provide subtle feedback based on comparisons
            if (feedbackTypes.HasFlag(StructureFeedbackTypes.ForLoops))
            {
                if (userForLoops < solutionForLoops)
                {
                    feedback.Add("Consider using loops for repeating segments.");
                    return;
                }
                else if (userForLoops > solutionForLoops)
                {
                    feedback.Add("More loops than expected.");
                    return;
                }
            }

            if (feedbackTypes.HasFlag(StructureFeedbackTypes.WhileLoops))
            {
                if (userWhileLoops < solutionWhileLoops)
                {
                    feedback.Add("Consider using while loops for better control flow.");
                    return;

                }
            }

            if (feedbackTypes.HasFlag(StructureFeedbackTypes.IfStatements))
            {
                if (userIfStatements < solutionIfStatements)
                {
                    feedback.Add("Additional conditional checks may prove useful.");
                    return;

                }
                else if (userIfStatements > solutionIfStatements)
                {
                    //feedback.Add("Too many conditions may impact readability.");
                    return;

                }
            }

            if (feedbackTypes.HasFlag(StructureFeedbackTypes.SwitchStatements))
            {
                if (userSwitchStatements < solutionSwitchStatements)
                {
                    feedback.Add("A switch statement might help");
                    return;

                }
                else if (userSwitchStatements > solutionSwitchStatements)
                {
                    //feedback.Add("A switch statement might help");
                    return;

                }
            }

            if (feedbackTypes.HasFlag(StructureFeedbackTypes.Methods))
            {
                if (userMethods < solutionMethods)
                {
                    feedback.Add("Consider modularizing code by adding more methods.");
                    return;

                }
                else if (userMethods > solutionMethods)
                {
                    //feedback.Add("Review method usage—simpler structure may be possible.");
                    return;

                }
            }

            if (feedbackTypes.HasFlag(StructureFeedbackTypes.Variables))
            {
                if (userVariables < solutionVariables)
                {
                    //feedback.Add("Ensure all required variables are declared.");
                    return;

                }
                else if (userVariables > solutionVariables)
                {
                    feedback.Add("Reduce number of variables for cleaner code.");
                    return;

                }
            }
        }
        catch (Exception e)
        {
            feedback.Add($"Error analyzing structure: {e.Message}");
            return;

        }
    }


}

[Flags]
public enum StructureFeedbackTypes
{
    None = 0,
    ForLoops = 1 << 0,          // 1
    WhileLoops = 1 << 1,        // 2
    IfStatements = 1 << 2,      // 4
    SwitchStatements = 1 << 3,   // 8
    Methods = 1 << 4,           // 16
    Variables = 1 << 5,         // 32
    All = ForLoops | WhileLoops | IfStatements | SwitchStatements | Methods | Variables // 63
}