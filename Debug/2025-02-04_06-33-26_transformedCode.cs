using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class CraneController
{
    public void MoveItems()
    {
        Crane.PickUp();
        Crane.MoveRight();
        Crane.Drop();
        Crane.MoveLeft();
    }

    public System.Collections.IEnumerator COR_MoveItems()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(5);
        yield return new UnityEngine.WaitForSeconds(0.3093595f);
        Crane.PickUp();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(0.3093595f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.3093595f);
        Crane.Drop();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(8);
        yield return new UnityEngine.WaitForSeconds(0.3093595f);
        Crane.MoveLeft();
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