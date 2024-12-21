using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform holdPoint; // Position in front of the camera where item is held

    private bool _isCarrying = false;
    public bool IsCarrying => _isCarrying;

    private PickupableCable _currentCarry;
    private Rigidbody _currentRigidbody;

    [SerializeField] private Vector3 holdOffset = new Vector3(0, -0.5f, 1.5f); // Offset from camera

    private void Update()
    {
        // Keep the carried item at the hold point while carrying
        if (_isCarrying && _currentCarry != null)
        {
            _currentCarry.transform.position = holdPoint.position + holdOffset;
            _currentCarry.transform.rotation = holdPoint.rotation;
        }
    }

    /// <summary>
    /// Picks up the item and disables its physics.
    /// </summary>
    public void PickUp(PickupableCable plug)
    {
        if (plug == null || _isCarrying) return;

        _isCarrying = true;
        _currentCarry = plug;
        _currentRigidbody = plug.GetComponent<Rigidbody>();

        // Disable Rigidbody to freeze the item
        if (_currentRigidbody)
        {
            _currentRigidbody.isKinematic = true;
            _currentRigidbody.detectCollisions = false;
        }

        plug.Unplug();
    }

    /// <summary>
    /// Drops the item, re-enabling its physics.
    /// </summary>
    public void Drop()
    {
        if (!_isCarrying) return;

        // Re-enable Rigidbody physics
        if (_currentRigidbody)
        {
            _currentRigidbody.isKinematic = false;
            _currentRigidbody.velocity = Vector3.zero;
            _currentRigidbody.detectCollisions = true;
        }

        _currentCarry = null;
        _currentRigidbody = null;
        _isCarrying = false;
    }

    /// <summary>
    /// Plugs the item into a socket, setting its position and state.
    /// </summary>
    public void PlugIn(CableSocket socket)
    {
        if (!_isCarrying || socket == null) return;

        if (socket.Occupied)
            Drop();

        _currentCarry.PlugIn(socket); // Call PlugIn logic of PickupableCable
        _currentCarry.transform.position = socket.transform.position;
        _currentCarry.transform.rotation = socket.transform.rotation;


        if (_currentRigidbody)
        {
            _currentRigidbody.isKinematic = true;
            _currentRigidbody.velocity = Vector3.zero;
            _currentRigidbody.detectCollisions = true;
        }

        _currentCarry = null;
        _currentRigidbody = null;
        _isCarrying = false;
    }
}
