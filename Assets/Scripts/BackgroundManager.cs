using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public GameObject background;
    private float minX, maxX;
    [SerializeField] private float movePoint;
    private Vector3 desiredPos;
    void Start()
    {
        desiredPos = transform.position;
    }

    void GetCameraBoundsLocation()
    {
        minX = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0)).x;
        maxX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0)).x;
        Debug.Log("MinX: " + minX + " MaxX: " + maxX);
    }

    // Update is called once per frame
    void Update()
    {
        GetCameraBoundsLocation();
        if(maxX > transform.position.x + movePoint){
            desiredPos.x += movePoint;
            transform.position = desiredPos;
        } else if(minX < transform.position.x - movePoint)
        {
            desiredPos.x -= movePoint;
            transform.position = desiredPos;
        }
    }
}
