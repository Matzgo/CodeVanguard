using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class Antenna
{
    public List<int> SortNumbers(List<int> numbers)
    {
        List<int> sortedNumbers = new List<int>(numbers); // Clone to avoid modifying the original list
        int n = sortedNumbers.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int minIndex = i; // Assume current index is the smallest
            // Find the minimum element in the remaining list
            for (int j = i + 1; j < n; j++)
            {
                if (sortedNumbers[j] < sortedNumbers[minIndex])
                {
                    minIndex = j; // Update index of the smallest value
                }
            }

            // Swap the found minimum element with the first element of the unsorted part
            int temp = sortedNumbers[i];
            sortedNumbers[i] = sortedNumbers[minIndex];
            sortedNumbers[minIndex] = temp;
        }

        return sortedNumbers;
    }

    public System.Collections.IEnumerator COR_SortNumbers(List<int> numbers)
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        List<int> sortedNumbers = new List<int>(numbers); // Clone to avoid modifying the original list
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(8);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        int n = sortedNumbers.Count;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(10);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        for (int i = 0; i < n - 1; i++)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(12);
            yield return new UnityEngine.WaitForSeconds(0.33f);
            int minIndex = i; // Assume current index is the smallest
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(15);
            yield return new UnityEngine.WaitForSeconds(0.33f);
            // Find the minimum element in the remaining list
            for (int j = i + 1; j < n; j++)
            {
                CodeInspector.RuntimeManager.Instance.HighlightLine(17);
                yield return new UnityEngine.WaitForSeconds(0.33f);
                if (sortedNumbers[j] < sortedNumbers[minIndex])
                {
                    CodeInspector.RuntimeManager.Instance.HighlightLine(19);
                    yield return new UnityEngine.WaitForSeconds(0.33f);
                    minIndex = j; // Update index of the smallest value
                    ;
                }

                ;
            }

            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(24);
            yield return new UnityEngine.WaitForSeconds(0.33f);
            // Swap the found minimum element with the first element of the unsorted part
            int temp = sortedNumbers[i];
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(25);
            yield return new UnityEngine.WaitForSeconds(0.33f);
            sortedNumbers[i] = sortedNumbers[minIndex];
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(26);
            yield return new UnityEngine.WaitForSeconds(0.33f);
            sortedNumbers[minIndex] = temp;
            ;
        }

        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(29);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        ;
        CodeInspector.RuntimeManager.Instance.DisableHighlightLine();
    }

    public void RunCoroutineMethod(string methodName, params object[] args)
    {
        var method = this.GetType().GetMethod(methodName);
        if (method != null)
        {
            var coroutine = (System.Collections.IEnumerator)method.Invoke(this, args);
            CodeInspector.RuntimeManager.Instance.CoroutineRunner.StartCoroutine(WrapCoroutine(coroutine));
        }
        else
        {
            throw new System.Exception($"Coroutine '{methodName}' not found.");
        }
    }

    public void StopCoroutinesMethod()
    {
        CodeInspector.RuntimeManager.Instance.CoroutineRunner.StopAllCoroutines();
    }

    private System.Collections.IEnumerator WrapCoroutine(System.Collections.IEnumerator coroutine)
    {
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
        }

        // Signal completion to RuntimeManager
        CodeInspector.RuntimeManager.Instance.CoroutineRunner.OnCoroutineComplete();
    }
}