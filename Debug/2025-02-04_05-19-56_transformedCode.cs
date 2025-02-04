using UnityEngine;
using Game;
using System;
using System.Collections.Generic;
using Console = Game.Console;
using Crane = Game.Crane;

public class GeneratorController
{
    int beamCount;
    public void HandleBeam()
    {
        beamCount++;
        if (beamCount % 2 == 0)
        {
            Generator.RedirectBeam();
        }
    }

    public void TestBeams()
    {
        Generator.FireBeam();
        HandleBeam();
        Generator.FireBeam();
        HandleBeam();
        Generator.FireBeam();
        HandleBeam();
        Generator.FireBeam();
        HandleBeam();
        Generator.FireBeam();
        HandleBeam();
        Generator.FireBeam();
        HandleBeam();
        Generator.Start();
    }

    public System.Collections.IEnumerator COR_HandleBeam()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(8);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        beamCount++;
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(9);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        if (beamCount % 2 == 0)
        {
            CodeInspector.RuntimeManager.Instance.HighlightLine(11);
            yield return new UnityEngine.WaitForSeconds(0.0628183f);
            Generator.RedirectBeam();
            ;
        }

        ;
        CodeInspector.RuntimeManager.Instance.DisableHighlightLine();
    }

    public System.Collections.IEnumerator COR_TestBeams()
    {
        CodeInspector.RuntimeManager.Instance.HighlightLine(17);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.FireBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(18);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        yield return COR_HandleBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(19);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.FireBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(20);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        yield return COR_HandleBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(21);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.FireBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(22);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        yield return COR_HandleBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(23);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.FireBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(24);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        yield return COR_HandleBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(25);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.FireBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(26);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        yield return COR_HandleBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(27);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.FireBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(28);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        yield return COR_HandleBeam();
        ;
        CodeInspector.RuntimeManager.Instance.HighlightLine(29);
        yield return new UnityEngine.WaitForSeconds(0.0628183f);
        Generator.Start();
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