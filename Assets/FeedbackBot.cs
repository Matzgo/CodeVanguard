using UnityEngine;
using UnityEngine.AI; // Import for NavMeshAgent

public class FeedbackBot : MonoBehaviour
{
    [SerializeField] private GameObject _player; // The player GameObject to follow
    [SerializeField] private float stopDistance = 1f; // Distance to stop from the player
    private NavMeshAgent _agent; // Reference to the NavMeshAgent

    void Start()
    {
        // Get the NavMeshAgent component on this GameObject
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("No NavMeshAgent component found on the FeedbackBot!");
        }

        if (_player == null)
        {
            Debug.LogError("Player GameObject not assigned in the inspector!");
        }
    }

    void Update()
    {
        if (_player == null || _agent == null) return;

        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (distanceToPlayer > stopDistance)
        {
            // Move towards the player
            _agent.SetDestination(_player.transform.position);
        }
        else
        {
            // Stop moving
            _agent.ResetPath();
        }
    }
}
