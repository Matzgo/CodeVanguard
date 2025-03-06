using CodeInspector;
using UnityEngine;

public class PickupableCable : MonoBehaviour
{
    [SerializeField]
    CSFileSet _task;
    [SerializeField]
    CodeVanguardManager _codeVanguardManager;
    [SerializeField]
    Rope _rope;

    CableSocket _socket;
    private float _forceCooldown = .2f;
    private float _lastForceTime;

    int waitFrames = 300;
    private int _frameCount = 0;

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

    internal bool IsBeyondMaxLength()
    {
        if (_rope.CurrentRopeLength > _rope.InitialRopeLength + .5f + _rope.InitialRopeLength * .01f)
            return true;

        return false;
    }

    private void FixedUpdate()
    {
        _frameCount++;
        if (_frameCount < waitFrames)
            return;

        if (IsBeyondMaxLength())
        {
            //Check if enough time has passed since the last force application
            if (Time.time - _lastForceTime >= _forceCooldown)
            {
                GetComponent<Rigidbody>().AddForce(_rope.EndDirection * 70f);
                _lastForceTime = Time.time; // Update the last force time
            }
        }
    }
}
