
using UnityEngine;

public static class CameraBounds
{
    public static void GetCameraBoundsLocation(Camera cam, out float minX, out float maxX, out float maxY, out float minY)
    {
        minX = cam.ScreenToWorldPoint(new Vector3(0,0,0)).x;
        maxX = cam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0)).x;
        minY = cam.ScreenToWorldPoint(new Vector3(0,0,0)).y;
        maxY = cam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0)).y;
        //Debug.Log("MinX: " + minX + " MaxX: " + maxX);
    }
}
