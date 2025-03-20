using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class CraneController
{
    public Crane crane;
    public void MoveItems()
    {
        for (int i = 0; i < 6; i++)
        {
            crane.PickUp();
            crane.MoveRight();
            crane.MoveRight();
            crane.MoveRight();
            crane.Drop();
            crane.MoveLeft();
            crane.MoveLeft();
            crane.MoveLeft();
        }

        // Move 4 items from Column 2 to Column 4
        crane.MoveRight(); // Move to Column 2
        for (int i = 0; i < 4; i++)
        {
            crane.PickUp();
            crane.MoveRight();
            crane.MoveRight();
            crane.Drop();
            crane.MoveLeft();
            crane.MoveLeft();
        }
    }

    public System.Collections.IEnumerator COR_MoveItems()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.5f);
        for (int i = 0; i < 6; i++)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(9);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.PickUp();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(10);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveRight();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(11);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveRight();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(12);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveRight();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(13);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.Drop();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(14);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveLeft();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(15);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveLeft();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(16);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveLeft();
            ;
        }

        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(20);
        yield return new UnityEngine.WaitForSeconds(0.5f);
        // Move 4 items from Column 2 to Column 4
        crane.MoveRight(); // Move to Column 2
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(21);
        yield return new UnityEngine.WaitForSeconds(0.5f);
        for (int i = 0; i < 4; i++)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(23);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.PickUp();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(24);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveRight();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(25);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveRight();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(26);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.Drop();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(27);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveLeft();
            ;
            CodeInspector.RuntimeManager.Instance.HighlightLine(28);
            yield return new UnityEngine.WaitForSeconds(0.5f);
            crane.MoveLeft();
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