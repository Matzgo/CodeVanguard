using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [Header("Instanced Mesh Details")]
    [SerializeField, Tooltip("The Mesh of chain link to render")] Mesh link;
    [SerializeField, Tooltip("The chain link material, must have gpu instancing enabled!")] Material linkMaterial;

    [Space]

    [Header("Demo Parameters")]
    [SerializeField, Min(0), Tooltip("The distance to project the mouse into world space")] float mouseOffset = 10f;

    [Space]

    [Header("Verlet Parameters")]

    [SerializeField, Tooltip("The distance between each link in the chain")] float nodeDistance = 0.35f;
    [SerializeField, Tooltip("The radius of the sphere collider used for each chain link")] float nodeColliderRadius = 0.2f;

    [SerializeField, Tooltip("Works best with a lower value")] float gravityStrength = 2;

    [SerializeField, Tooltip("The number of chain links. Decreases performance with high values and high iteration")] int totalNodes = 100;

    [SerializeField, Range(0, 1), Tooltip("Modifier to dampen velocity so the simulation can stabilize")] float velocityDampen = 0.95f;

    [SerializeField, Range(0, 0.99f), Tooltip("The stiffness of the simulation. Set to lower values for more elasticity")] float stiffness = 0.8f;

    [SerializeField, Tooltip("Setting this will test collisions for every n iterations. Possibly more performance but less stable collisions")] int iterateCollisionsEvery = 1;

    [SerializeField, Tooltip("Iterations for the simulation. More iterations is more expensive but more stable")] int iterations = 100;

    [SerializeField, Tooltip("How many colliders to test against for every node.")] int colliderBufferSize = 1;
    [SerializeField, Tooltip("Maximum velocity for rope nodes to stabilize physics")]
    private float maxVelocity = 5f;
    RaycastHit[] raycastHitBuffer;
    Collider[] colliderHitBuffer;
    Camera cam;

    // Need a better way of stepping through collisions for high Gravity
    // And high Velocity
    Vector3 gravity;

    Vector3 startLock;
    Vector3 endLock;

    bool isStartLocked = false;
    bool isEndLocked = false;

    [Space]

    // For Debug Drawing the chain/rope
    [Header("Line Renderer")]
    [SerializeField, Tooltip("Width for the line renderer")] float ropeWidth = 0.1f;

    LineRenderer lineRenderer;
    Vector3[] linePositions;

    Vector3[] previousNodePositions;

    Vector3[] currentNodePositions;
    Quaternion[] currentNodeRotations;

    SphereCollider nodeCollider;
    GameObject nodeTester;
    Matrix4x4[] matrices;

    [SerializeField]
    GameObject start;
    [SerializeField]
    GameObject end;
    [SerializeField]
    private bool drawMesh;
    int totalIntermediateNodes;
    [SerializeField, Tooltip("Number of intermediate meshes between main nodes")] int intermediateNodes = 2;

    [Header("Debug Info")]
    [SerializeField] private float currentRopeLength;
    public float CurrentRopeLength => currentRopeLength;
    [SerializeField] private float initialRopeLength;
    [SerializeField] private float floorHeight = .15f;
    private int _frameNr;

    public float InitialRopeLength => initialRopeLength;
    public Vector3 EndDirection => (currentNodePositions[totalNodes - 4] - currentNodePositions[totalNodes - 1]).normalized;
    void Awake()
    {
        currentNodePositions = new Vector3[totalNodes];
        previousNodePositions = new Vector3[totalNodes];
        currentNodeRotations = new Quaternion[totalNodes];

        raycastHitBuffer = new RaycastHit[colliderBufferSize];
        colliderHitBuffer = new Collider[colliderBufferSize];
        gravity = new Vector3(0, -gravityStrength, 0);
        cam = Camera.main;
        lineRenderer = this.GetComponent<LineRenderer>();

        nodeTester = new GameObject();
        nodeTester.name = "Node Tester";
        nodeTester.layer = 8;
        nodeCollider = nodeTester.AddComponent<SphereCollider>();
        nodeCollider.radius = nodeColliderRadius;

        // Calculate total number of nodes including intermediates
        totalIntermediateNodes = (totalNodes - 1) * (intermediateNodes + 1) + 1;
        matrices = new Matrix4x4[totalIntermediateNodes];

        // Adjust the spacing to maintain the same total rope length
        float adjustedNodeDistance = nodeDistance * (totalNodes - 1) / (totalIntermediateNodes - 1);

        Vector3 startPosition = start.transform.position;
        for (int i = 0; i < totalNodes; i++)
        {
            // Use original nodeDistance for physics simulation nodes
            currentNodePositions[i] = startPosition;
            currentNodeRotations[i] = Quaternion.identity;
            previousNodePositions[i] = startPosition;
            startPosition.y += nodeDistance;
        }

        // Initialize matrices
        for (int i = 0; i < totalIntermediateNodes; i++)
        {
            matrices[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        }

        linePositions = new Vector3[totalNodes];

        CalculateRopeLength();
        initialRopeLength = currentRopeLength;
    }

    private void CalculateRopeLength()
    {
        float length = 0f;
        for (int i = 0; i < totalNodes - 1; i++)
        {
            length += Vector3.Distance(currentNodePositions[i], currentNodePositions[i + 1]);
        }
        currentRopeLength = length;
    }

    void Update()
    {
        //// Attach rope end to mouse click position
        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (!isStartLocked)
        //    {
        //        isStartLocked = true;
        //    }
        //    else if (!isEndLocked)
        //    {
        //        isEndLocked = true;
        //    }
        //}
        //else if (!isStartLocked && !isEndLocked)
        //{
        //    startLock = cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, mouseOffset));
        //}
        //else if (isStartLocked && !isEndLocked)
        //{
        //    endLock = cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, mouseOffset));
        //}

        startLock = start.transform.position;
        isStartLocked = true;
        endLock = end.transform.position;
        isEndLocked = true;
        //DrawRope();
        CalculateRopeLength();
        //ValidateMatrices();
        // Instanced drawing here is really performant over using GameObjects
        if (drawMesh)
            Graphics.DrawMeshInstanced(link, 0, linkMaterial, matrices, totalIntermediateNodes);
    }

    void ValidateMatrices()
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            Matrix4x4 matrix = matrices[i];
            if (!IsValidMatrix(matrix))
            {
                Debug.LogError($"Invalid matrix at index {i}: {matrix}");
                matrices[i] = Matrix4x4.identity; // Replace with a safe default
            }
        }
    }

    bool IsValidMatrix(Matrix4x4 matrix)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (float.IsNaN(matrix[i, j]) || float.IsInfinity(matrix[i, j]))
                    return false;
            }
        }
        return true;
    }

    private void FixedUpdate()
    {
        //_frameNr++;
        //if (_frameNr < 100)
        //    return;

        Simulate();

        for (int i = 0; i < iterations; i++)
        {
            ApplyConstraint();

            if (i % iterateCollisionsEvery == 0)
            {
                AdjustCollisions();
            }
        }

        SetAngles();
        TranslateMatrices();
    }

    private void Simulate()
    {
        var fixedDt = Time.fixedDeltaTime;
        for (int i = 0; i < totalNodes; i++)
        {
            // Calculate velocity
            Vector3 velocity = currentNodePositions[i] - previousNodePositions[i];



            velocity *= velocityDampen;


            // Clamp the velocity to the maximum allowed value
            if (velocity.magnitude > maxVelocity)
            {
                velocity = velocity.normalized * maxVelocity;
            }

            // Store the previous position
            previousNodePositions[i] = currentNodePositions[i];

            // Calculate the new position
            Vector3 newPos = currentNodePositions[i] + velocity;
            newPos += gravity * fixedDt;

            if (float.IsNaN(newPos.x) || float.IsNaN(newPos.y) || float.IsNaN(newPos.z))
            {
                newPos = previousNodePositions[i];  // Recover using previous position
            }

            if (newPos.y < floorHeight)
            {
                newPos = new Vector3(newPos.x, floorHeight, newPos.z);
            }

            currentNodePositions[i] = newPos;
        }
    }


    private void AdjustCollisions()
    {
        int layerToExclude1 = 8;
        int layerToExclude2 = 11;

        // Create a layer mask that excludes both layers 8 and 11
        int layerMask = ~((1 << layerToExclude1) | (1 << layerToExclude2));

        for (int i = 0; i < totalNodes; i++)
        {
            int result = Physics.OverlapSphereNonAlloc(currentNodePositions[i], nodeColliderRadius + 0.01f, colliderHitBuffer, layerMask);

            if (result > 0)
            {
                Vector3 totalAdjustment = Vector3.zero;
                float highestUpwardAdjustment = 0f;

                // First pass: collect all collision adjustments
                for (int n = 0; n < result; n++)
                {
                    Vector3 colliderPosition = colliderHitBuffer[n].transform.position;
                    Quaternion colliderRotation = colliderHitBuffer[n].gameObject.transform.rotation;

                    Vector3 dir;
                    float distance;

                    Physics.ComputePenetration(nodeCollider, currentNodePositions[i], Quaternion.identity,
                        colliderHitBuffer[n], colliderPosition, colliderRotation, out dir, out distance);

                    Vector3 adjustment = dir * distance;

                    // Track the highest upward adjustment
                    if (adjustment.y > highestUpwardAdjustment)
                    {
                        highestUpwardAdjustment = adjustment.y;
                    }

                    totalAdjustment += adjustment;
                }

                // If we're being pushed down overall but have some upward collisions,
                // prioritize the upward movement
                if (totalAdjustment.y < 0 && highestUpwardAdjustment > 0)
                {
                    // Keep the horizontal adjustment but use the highest upward adjustment
                    totalAdjustment.y = highestUpwardAdjustment;
                }

                // Apply the final adjustment
                currentNodePositions[i] += totalAdjustment;

                // Additional safety check: ensure minimum height if we detect we might be going through the floor
                if (result > 1 && totalAdjustment.y < 0)
                {
                    if (currentNodePositions[i].y < floorHeight)
                    {
                        currentNodePositions[i].y = floorHeight + nodeColliderRadius;
                    }
                }
            }
        }
    }

    private void ApplyConstraint()
    {
        currentNodePositions[0] = startLock;
        if (isStartLocked)
        {
            currentNodePositions[totalNodes - 1] = endLock;
        }

        for (int i = 0; i < totalNodes - 1; i++)
        {
            var node1 = currentNodePositions[i];
            var node2 = currentNodePositions[i + 1];

            // Get the current distance between rope nodes
            float currentDistance = (node1 - node2).magnitude;
            float difference = Mathf.Abs(currentDistance - nodeDistance);
            Vector3 direction = Vector3.zero;

            // determine what direction we need to adjust our nodes
            if (currentDistance > nodeDistance)
            {
                direction = (node1 - node2).normalized;
            }
            else if (currentDistance < nodeDistance)
            {
                direction = (node2 - node1).normalized;
            }

            // calculate the movement vector
            Vector3 movement = direction * difference;
            // apply correction
            currentNodePositions[i] -= (movement * stiffness);
            currentNodePositions[i + 1] += (movement * stiffness);
        }
    }

    void SetAngles()
    {
        for (int i = 0; i < totalNodes - 1; i++)
        {
            var node1 = currentNodePositions[i];
            var node2 = currentNodePositions[i + 1];

            var dir = (node2 - node1).normalized;
            if (dir != Vector3.zero)
            {
                if (i > 0)
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(dir, Vector3.right);
                    currentNodeRotations[i + 1] = desiredRotation;
                }
                else if (i < totalNodes - 1)
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(dir, Vector3.right);
                    currentNodeRotations[i + 1] = desiredRotation;
                }
                else
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(dir, Vector3.right);
                    currentNodeRotations[i] = desiredRotation;
                }
            }

            if (i % 2 == 0 && i != 0)
            {
                currentNodeRotations[i + 1] *= Quaternion.Euler(0, 0, 90);
            }
        }
    }

    void TranslateMatrices()
    {
        int currentIndex = 0;

        for (int i = 0; i < totalNodes - 1; i++)
        {
            Vector3 startPos = currentNodePositions[i];
            Vector3 endPos = currentNodePositions[i + 1];
            Quaternion startRot = currentNodeRotations[i];
            Quaternion endRot = currentNodeRotations[i + 1];

            // Set the main node
            matrices[currentIndex].SetTRS(startPos, startRot, Vector3.one);
            currentIndex++;

            // Create intermediate nodes
            for (int j = 1; j <= intermediateNodes; j++)
            {
                float t = j / (float)(intermediateNodes + 1);

                // Use Vector3.LerpUnclamped to maintain proper segment length
                Vector3 intermediatePos = Vector3.Lerp(startPos, endPos, t);

                // For rotation, we still use regular Lerp as we want smooth rotation
                Quaternion intermediateRot = Quaternion.Lerp(startRot, endRot, t);

                // Add variation to intermediate rotations if desired
                if (i % 2 == 0)
                {
                    intermediateRot *= Quaternion.Euler(0, 0, 90 * t);
                }

                matrices[currentIndex].SetTRS(intermediatePos, intermediateRot, Vector3.one);
                currentIndex++;
            }
        }

        //// Set the last main node
        //matrices[currentIndex].SetTRS(
        //    currentNodePositions[totalNodes - 1],
        //    currentNodeRotations[totalNodes - 1],
        //    Vector3.one
        //);
    }

    private void DrawRope()
    {
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        for (int n = 0; n < totalNodes; n++)
        {
            linePositions[n] = currentNodePositions[n];
        }

        lineRenderer.positionCount = linePositions.Length;
        lineRenderer.SetPositions(linePositions);
    }

}
