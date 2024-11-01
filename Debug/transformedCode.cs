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
        Console.WriteLine(result);
        TestMethod();
        return result;
    }

    private void TestMethod()
    {
        Console.WriteLine("Test");
    }

    public System.Collections.IEnumerator COR_Add(int a, int b)
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(5);
        yield return new UnityEngine.WaitForSeconds(1f);
        int result = a + b;
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(1f);
        Console.WriteLine(result);
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(1f);
        yield return COR_TestMethod();
        CodeInspector.RuntimeManager.Instance.HighlightLine(8);
        yield return new UnityEngine.WaitForSeconds(1f);
        Debug.Log($"Line 8: RETURN " + result.ToString());
        CodeInspector.RuntimeManager.Instance.DisableHighlightLine();
    }

    private System.Collections.IEnumerator COR_TestMethod()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(13);
        yield return new UnityEngine.WaitForSeconds(1f);
        Console.WriteLine("Test");
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