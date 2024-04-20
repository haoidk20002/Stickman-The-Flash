using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackgroundAndPlatform : MonoBehaviour
{
    // Reference to the main camera
    public Camera mainCamera;

    // Prefab for generating background scenes
    public GameObject backgroundPrefab;

    // Distance from the camera at which to spawn new background scenes
    public float spawnDistance = 50f;

    // Distance from the camera at which to delete background scenes
    public float deletionDistance = 100f;

    // List to store spawned background scenes
    private List<GameObject> backgroundScenes = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //SpawnInitialBackgrounds();
    }

    // Update is called once per frame
    /*void Update()
    {
        // Check if camera is close to the boundary for spawning new background scenes
        if (Vector3.Distance(mainCamera.transform.position, backgroundScenes[backgroundScenes.Count - 1].transform.position) < spawnDistance)
        {
            SpawnBackground();
        }

        // Delete background scenes that are far from the camera
        for (int i = 0; i < backgroundScenes.Count; i++)
        {
            if (Vector3.Distance(mainCamera.transform.position, backgroundScenes[i].transform.position) > deletionDistance)
            {
                Destroy(backgroundScenes[i]);
                backgroundScenes.RemoveAt(i);
                i--; // Decrement i to account for removed item
            }
        }
    }*/

    // Spawn initial background scenes to fill the starting area
    void SpawnInitialBackgrounds()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnBackground();
        }
    }

    // Spawn a new background scene
    void SpawnBackground()
    {
        Vector3 spawnPosition = backgroundScenes.Count == 0 ? Vector3.zero : backgroundScenes[backgroundScenes.Count - 1].transform.position + Vector3.forward * spawnDistance;
        GameObject newBackground = Instantiate(backgroundPrefab, spawnPosition, Quaternion.identity);
        backgroundScenes.Add(newBackground);
    }
}
