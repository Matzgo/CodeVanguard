using UnityEngine;

public class PaperInteractable : MonoBehaviour, IInteractable
{
    [SerializeField]
    ObjectInspector _objectInspector;
    [SerializeField]
    Vector3 _inspectRotate;
    [SerializeField]
    float _distance = 1;
    private void Awake()
    {
        if (_objectInspector == null)
            _objectInspector = FindAnyObjectByType<ObjectInspector>();
    }

    public void OnInteract()
    {
        _objectInspector.Show(this, _inspectRotate, _distance);
    }

    public bool CanInteract()
    {
        return true;
    }
}
