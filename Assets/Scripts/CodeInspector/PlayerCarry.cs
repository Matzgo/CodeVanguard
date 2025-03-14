using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField]
    private float throwStrength = 5f;
    [SerializeField] private Transform holdPoint; // Position in front of the camera where item is held

    private bool _isCarrying = false;
    public bool IsCarrying => _isCarrying;

    private PickupableCable _currentCarry;
    private Rigidbody _currentRigidbody;

    [SerializeField] private Vector3 holdOffset = new Vector3(0, -0.5f, 1.5f); // Offset from camera
    [SerializeField] private float _lerpSpeed = 10f;
    [SerializeField] private Camera _camera;

    private void Update()
    {
        // Keep the carried item at the hold point while carrying
        if (_isCarrying && _currentCarry != null)
        {
            // Calculate target position and rotation
            Vector3 targetPosition = holdPoint.position + holdOffset;
            Quaternion targetRotation = holdPoint.rotation;

            // Smoothly interpolate the position and rotation
            _currentCarry.transform.position = Vector3.Lerp(_currentCarry.transform.position, targetPosition, Time.deltaTime * _lerpSpeed); // Adjust the speed multiplier as needed
            _currentCarry.transform.rotation = Quaternion.Slerp(_currentCarry.transform.rotation, targetRotation, Time.deltaTime * _lerpSpeed); // Adjust the speed multiplier as needed

            if (_currentCarry.IsBeyondMaxLength())
            {
                Drop();
                //_currentCarry.GetComponent<Rigidbody>().velocity = _currentCarry.
            }
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
            // Get the camera's forward direction
            Transform cameraTransform = _camera.transform;

            // Adjust the throwing direction by modifying the x-rotation
            Quaternion adjustedRotation = Quaternion.Euler(cameraTransform.eulerAngles.x - 20, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);

            // Calculate the adjusted forward direction
            Vector3 throwDirection = adjustedRotation * Vector3.forward;

            _currentRigidbody.isKinematic = false;
            _currentRigidbody.velocity = throwDirection * throwStrength;
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

        if (_currentRigidbody)
        {
            _currentRigidbody.isKinematic = true;
            _currentRigidbody.velocity = Vector3.zero;
            _currentRigidbody.detectCollisions = true;
        }

        _currentCarry.transform.position = socket.transform.position;
        _currentCarry.transform.rotation = socket.transform.rotation;




        _currentCarry = null;
        _currentRigidbody = null;
        _isCarrying = false;
    }
}
