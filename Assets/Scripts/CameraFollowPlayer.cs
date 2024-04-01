using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform
    private float cameraSpeed = 5f; // Speed at which the camera follows the player
    public float edgeBuffer = 1f; // Buffer distance from the edges to start following
    private Camera cam;
    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // Calculate the bounds based on the camera's viewport
        minBounds = cam.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z));
        maxBounds = cam.ViewportToWorldPoint(new Vector3(1, 1, transform.position.z));

        // Calculate the buffer zone
        minBounds.x += edgeBuffer;
        maxBounds.x -= edgeBuffer;

        // Check if the player's x-position deviates from the center
        if (Mathf.Abs(player.position.x - transform.position.x) > 0.1f)
        {
            // Calculate target position
            float targetX = Mathf.Clamp(player.position.x, minBounds.x, maxBounds.x);
            float targetY = transform.position.y; // Keep the y position of the camera unchanged
            Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);

            // Move the camera towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
        }
    }
}

