using CodeInspector;
using System;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public Action OnCoroutineFinshed;
    public void OnCoroutineComplete()
    {
        RuntimeManager.Instance.Console.WriteLine("--- FINISHED ---", Color.magenta);
        OnCoroutineFinshed?.Invoke();
    }
}
