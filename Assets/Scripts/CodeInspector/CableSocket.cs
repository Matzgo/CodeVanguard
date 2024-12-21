using UnityEngine;

public class CableSocket : MonoBehaviour
{
    bool _occupied;
    public bool Occupied => _occupied;

    internal void Free()
    {
        _occupied = false;
    }

    internal void Occupy(PickupableCable currentCarry)
    {
        _occupied = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
