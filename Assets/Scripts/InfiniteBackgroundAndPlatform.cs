using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackgroundAndPlatform : MonoBehaviour
{
    // Reference to the main camera
    public Camera mainCamera;

    // Prefab for generating background scenes
    public GameObject[] backgroundPrefab;

    // Distance from the camera at which to spawn new background scenes
    [SerializeField] private float spawnDistance = 25f;

    // Distance from the camera at which to delete background scenes
    [SerializeField] private float deletionDistance = 25f;

    private Vector3 cameraBounds;
    private float minX, maxX;

    private void GetCameraBounds() //camera's bounds depending on aspect ratio
    {
        var leftBounds = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0));
        var rightBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0));
        cameraBounds = rightBounds - leftBounds;

        //Debug.Log("Camera Bounds: " + cameraBounds);
    }

    private void CalculateBoundsLocation()
    {
        minX = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0)).x;
        maxX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0)).x;
    }

    void Start(){
        // Camera boundaries
        GetCameraBounds();
    }
    void LateUpdate(){
        CalculateBoundsLocation();
        // if camera's bound is near the background boundaries, the part of the background being far from camera is move the location the connects
        // to the one being near the camera
        // if (transform.position.x < 25){

        // }
        // if(){

        // }
    }
}
