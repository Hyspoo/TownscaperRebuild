using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private Transform target;

    private Vector3 previousPosition;
    private float visualRange = 10f;

    public float minZoomRate = 3f;
    public float maxZoomRate = 100f;
    public float zoomSpeed = 1.001f;

    void Start()
    {
        target = GameObject.Find("CenterLocator").transform;
        cam.transform.position = target.position;
        cam.transform.Translate(new Vector3(0, 0, -visualRange));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            cam.transform.position = target.position;
            cam.transform.Rotate(Vector3.right, direction.y * 180);
            cam.transform.Rotate(Vector3.up, -direction.x * 180, Space.World);

            cam.transform.Translate(new Vector3(0, 0, -visualRange));

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetKey(KeyCode.UpArrow) && visualRange > minZoomRate) 
        {
            visualRange /= zoomSpeed;
            cam.transform.position = target.position;
            cam.transform.Translate(new Vector3(0, 0, -visualRange));
        }
        
        if (Input.GetKey(KeyCode.DownArrow) && visualRange < maxZoomRate) 
        {
            visualRange *= zoomSpeed;
            cam.transform.position = target.position;
            cam.transform.Translate(new Vector3(0, 0, -visualRange));
        }
    }
}
