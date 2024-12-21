using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class SafeSecurity
{
    public void OpenSafeButton()
    {
        bool locked = Safe.Locked;
        if (!locked)
        {
            Safe.Alarm();
        }
        else
        {
            Safe.Open();
        }
    }

    public System.Collections.IEnumerator COR_OpenSafeButton()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(5);
        yield return new UnityEngine.WaitForSeconds(0.26f);
        bool locked = Safe.Locked;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(0.26f);
        if (!locked)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(8);
            yield return new UnityEngine.WaitForSeconds(0.26f);
            Safe.Alarm();
            ;
        }
        else
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(12);
            yield return new UnityEngine.WaitForSeconds(0.26f);
            Safe.Open();
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
            CodeInspector.RuntimeManager.Instance.CoroutineRunner.StartCoroutine((System.Collections.IEnumerator)method.Invoke(this, args));
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
}