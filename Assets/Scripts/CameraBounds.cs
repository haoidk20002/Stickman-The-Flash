
using UnityEngine;

public static class CameraBounds
{
    public static void GetCameraBoundsLocation(Camera cam, out float min, out float max)
    {
        min = cam.ScreenToWorldPoint(new Vector3(0,0,0)).x;
        max = cam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0)).x;
        //Debug.Log("MinX: " + minX + " MaxX: " + maxX);
    }
}
