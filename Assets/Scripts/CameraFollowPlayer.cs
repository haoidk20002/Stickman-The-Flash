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

    private bool playerDead = false, zoomed = false;

    private Character mainPlayer;

    private void HandlePlayerDeath(bool value)
    {
        playerDead = value;
    }



    void Awake()
    {
        desiredLocation = transform.position;
        mainCamera = Camera.main;
        //mainPlayer.Evt_PlayerDead += HandlePlayerDeath;
    }
    private void Update()
    {
        if (player == null) { return;}
        // if (playerDead && !zoomed){
        //     //StartCoroutine(ZoomInThenOut());
        //     zoomed = true;
        //     return;
        // }
        desiredLocation.x = player.position.x;
    }
    private void MoveCamera()
    {
        if (player == null) { return;}
        if (player.position.x < transform.position.x - positionDifference)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
        }
        else if (player.position.x > transform.position.x + positionDifference)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, cameraSpeed * Time.deltaTime);
        }
    }
    // private IEnumerator ZoomInThenOut(){
    //     mainCamera.orthographicSize = 5f;
    //     yield return new WaitForSeconds(1f);
    //     mainCamera.orthographicSize = 20f;
    // }


    void LateUpdate()
    {
        MoveCamera();
        // if player is about to die, zoom camera in (achievable using delegates)
    }
}

