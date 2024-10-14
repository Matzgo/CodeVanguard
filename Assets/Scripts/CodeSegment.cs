using UnityEngine;


public abstract class CodeSegment : MonoBehaviour
{
    public abstract bool IsEditable { get; }


    public abstract string Text { get; }
}
