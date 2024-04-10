using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform
    [SerializeField] private float cameraSpeed; // Speed at which the camera follows the player

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
    {
        if(player.position.x < transform.position.x - 10)
        {
            //transform.Translate(new Vector3(-1f,0,0));
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed*Time.deltaTime);
        }else if(player.position.x > transform.position.x + 10)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed*Time.deltaTime);
            //transform.Translate(new Vector3(1f,0,0));
        }
    }
}

