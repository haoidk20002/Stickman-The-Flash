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
    // Update is called once per frame
    void Update()
    {
        // more convenient way (Use a static method from static class)
        CameraBounds.GetCameraBoundsLocation(Camera.main, out minX, out maxX);
        // GetCameraBoundsLocation();
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
