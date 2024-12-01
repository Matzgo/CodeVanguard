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
        Crane.MoveRight();
        Crane.MoveRight();
        Crane.Drop();
        Crane.MoveLeft();
        Crane.MoveLeft();
        Crane.MoveLeft();
        Crane.PickUp();
        Crane.MoveRight();
        Crane.MoveRight();
        Crane.MoveRight();
        Crane.Drop();
    }

    public System.Collections.IEnumerator COR_MoveItems()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(5);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.PickUp();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(6);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(7);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(8);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(9);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.Drop();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(10);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveLeft();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(11);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveLeft();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(12);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveLeft();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(13);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.PickUp();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(14);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(15);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(16);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.MoveRight();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(17);
        yield return new UnityEngine.WaitForSeconds(0.33f);
        Crane.Drop();
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