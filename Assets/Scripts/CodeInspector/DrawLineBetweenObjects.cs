using UnityEngine;

public class DrawLineBetweenObjects : MonoBehaviour
{
    public Transform startObject; // The starting point
    public Transform endObject;   // The ending point

    private LineRenderer lineRenderer;

    void Start()
    {
        // Get or add the LineRenderer component
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        // LineRenderer settings
        lineRenderer.positionCount = 2; // Two points: start and end
        lineRenderer.startWidth = 0.1f; // Line thickness
        lineRenderer.endWidth = 0.1f;
        //lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
    }

    void Update()
    {
        if (startObject != null && endObject != null)
        {
            // Update the LineRenderer positions
            lineRenderer.SetPosition(0, startObject.position); // Start point
            lineRenderer.SetPosition(1, endObject.position);   // End point
        }
    }
}
