using UnityEngine;

public class MoveLeftRight : MonoBehaviour
{
    public float speed = 2f; // Speed of movement
    public float distance = 5f; // Distance to move left and right

    private Vector3 startPosition; // Starting position of the GameObject

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position; // Record the initial position
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new x position using Mathf.PingPong
        float newX = startPosition.x + Mathf.PingPong(Time.time * speed, distance * 2) - distance;

        // Update the GameObject's position
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
