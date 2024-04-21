using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform
    [SerializeField] private float cameraSpeed; // Speed at which the camera follows the player

    [SerializeField] private float positionDifference;

    [SerializeField] private float spawnDistance;
    [SerializeField] private float deletionDistance;

    public GameObject backgroundPrefabs;

    private Camera mainCamera;
    private Vector3 desiredLocation;



    void Awake()
    {
        desiredLocation = transform.position;
        mainCamera = Camera.main;
    }
    void Update()
    {
        desiredLocation.x = player.position.x;
    }
    void MoveCamera()
    {
        if (player.position.x < transform.position.x - positionDifference)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
        }
        else if (player.position.x > transform.position.x + positionDifference)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
        }
    }


    void LateUpdate()
    {
        MoveCamera();
    }
}

