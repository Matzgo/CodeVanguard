using CodeInspector;
using UnityEngine;

public class PickupableCable : MonoBehaviour
{
    [SerializeField]
    CSFileSet _task;
    [SerializeField]
    CodeVanguardManager _codeVanguardManager;

    CableSocket _socket;
    public void Unplug()
    {
        if (_socket == null)
            return;


        _codeVanguardManager.Unplug();
        _socket.Free();
        _socket = null;
    }

    public void PlugIn(CableSocket socket)
    {
        socket.Occupy(this);
        _socket = socket;
        _codeVanguardManager.PlugIn(_task);
    }
}
