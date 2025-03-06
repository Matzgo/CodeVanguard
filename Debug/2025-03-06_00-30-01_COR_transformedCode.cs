using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class SafeSecurity
{
    public Safe safe;
    public void OpenSafeButton()
    {
        bool LoCkeD = safe.Locked;
        if (false)
        {
            safe.Alarm();
        }
        else
        {
            safe.Open();
        }
    }

    public System.Collections.IEnumerator COR_OpenSafeButton()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(1.05f);
        bool LoCkeD = safe.Locked;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(8);
        yield return new UnityEngine.WaitForSeconds(1.05f);
        if (false)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(10);
            yield return new UnityEngine.WaitForSeconds(1.05f);
            safe.Alarm();
            ;
        }
        else
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(14);
            yield return new UnityEngine.WaitForSeconds(1.05f);
            safe.Open();
            ;
        }

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