using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;

public class Test
{
    public int Add(int a, int b)
    {
        int result = a + b;
        for (int i = 0; i < 10; i++)
        {
            result = Double(result);
        }

        return result;
    }

    private int Double(int n)
    {
        return n * 2;
    }

    public System.Collections.IEnumerator COR_Add(int a, int b)
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(5);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        int result = a + b;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Console.WriteLine(result);
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        for (int i = 0; i < 10; i++)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(9);
            yield return new UnityEngine.WaitForSeconds(0.33f);
            yield return COR_Double(result);
            result = Double(result);
            ;
        }

        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(12);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        ;
        CodeInspector.RuntimeManager.Instance.DisableHighlightLine();
    }

    private System.Collections.IEnumerator COR_Double(int n)
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(17);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        ;
        CodeInspector.RuntimeManager.Instance.DisableHighlightLine();
    }

    public void RunCoroutineMethod(string methodName, params object[] args)
    {
        var method = this.GetType().GetMethod(methodName);
        if (method != null)
        {
            CodeInspector.RuntimeManager.Instance.StartCoroutine((System.Collections.IEnumerator)method.Invoke(this, args));
        }
        else
        {
            throw new System.Exception($"Coroutine '{methodName}' not found.");
        }
    }
}