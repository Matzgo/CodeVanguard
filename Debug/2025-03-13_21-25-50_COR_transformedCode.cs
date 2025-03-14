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
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(0.1f);
        List<int> sortedNumbers = new List<int>(numbers); // Clone to avoid modifying the original list
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.1f);
        int n = sortedNumbers.Count;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(9);
        yield return new UnityEngine.WaitForSeconds(0.1f);
        for (int i = 0; i < n - 1; i++)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(11);
            yield return new UnityEngine.WaitForSeconds(0.1f);
            int minIndex = i; // Assume current index is the smallest
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(14);
            yield return new UnityEngine.WaitForSeconds(0.1f);
            // Find the minimum element in the remaining list
            for (int j = i + 1; j < n; j++)
            {
                CodeInspector.RuntimeManager.Instance.HighlightLine(16);
                yield return new UnityEngine.WaitForSeconds(0.1f);
                if (sortedNumbers[j] < sortedNumbers[minIndex])
                {
                    CodeInspector.RuntimeManager.Instance.HighlightLine(18);
                    yield return new UnityEngine.WaitForSeconds(0.1f);
                    minIndex = j; // Update index of the smallest value
                    ;
                }

                ;
            }

            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(23);
            yield return new UnityEngine.WaitForSeconds(0.1f);
            // Swap the found minimum element with the first element of the unsorted part
            int temp = sortedNumbers[i];
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(24);
            yield return new UnityEngine.WaitForSeconds(0.1f);
            sortedNumbers[i] = sortedNumbers[minIndex];
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(25);
            yield return new UnityEngine.WaitForSeconds(0.1f);
            sortedNumbers[minIndex] = temp;
            ;
        }

        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(28);
        yield return new UnityEngine.WaitForSeconds(0.1f);
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