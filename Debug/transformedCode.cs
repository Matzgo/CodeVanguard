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
        for (int i = 0; i < 100; i++)
        {
            result = Double(result);
            int x = 1;
            int y = 1;
            int z = 1;
        }

        return a + b;
    }

    private int Double(int n)
    {
        return n * 2;
    }

    public System.Collections.IEnumerator COR_Add(int a, int b)
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(5);
        yield return new UnityEngine.WaitForSeconds(0.05f);
        int result = a + b;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(0.05f);
        Console.WriteLine(result);
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.05f);
        for (int i = 0; i < 100; i++)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(9);
            yield return new UnityEngine.WaitForSeconds(0.05f);
            yield return COR_Double(result);
            result = Double(result);
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(10);
            yield return new UnityEngine.WaitForSeconds(0.05f);
            int x = 1;
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(11);
            yield return new UnityEngine.WaitForSeconds(0.05f);
            int y = 1;
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(12);
            yield return new UnityEngine.WaitForSeconds(0.05f);
            int z = 1;
            ;
        }

        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(15);
        yield return new UnityEngine.WaitForSeconds(0.05f);
        ;
        CodeInspector.RuntimeManager.Instance.DisableHighlightLine();
    }

    private System.Collections.IEnumerator COR_Double(int n)
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(20);
        yield return new UnityEngine.WaitForSeconds(0.05f);
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