using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform
    [SerializeField] private float cameraSpeed; // Speed at which the camera follows the player
    [SerializeField] private int cameraRangeLimit = 250;

    [SerializeField] private float positionDifference = 10;


    private Vector3 desiredLocation;


    void Awake()
    {
        desiredLocation = transform.position;
    }
    void Update()
    {
        desiredLocation.x = player.position.x;
    }

    void LateUpdate()
    {   // Lock camera movement with range of 500, origin at (0,6.5,-10)
        //if (Mathf.Abs(transform.position.x) <= cameraRangeLimit)
        //{
            if (player.position.x < transform.position.x - positionDifference)
            {
                //transform.Translate(new Vector3(-1f,0,0));
                transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
            }
            else if (player.position.x > transform.position.x + positionDifference)
            {
                transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
                //transform.Translate(new Vector3(1f,0,0));
            }
        //}
    }
}

