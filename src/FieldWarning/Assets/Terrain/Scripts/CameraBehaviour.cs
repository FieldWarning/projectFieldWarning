using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour
{
    Camera cam;
    Vector3 camOffset;
    Transform orbitPoint;
    
    float baseMovementspeed = 1.5f;
    float scaleSpeed = 1;
    float zoomFactor = 1.6f;
    float horizontalROtationSpeed = 5;
    float verticalROtationSpeed = .1f;
    float upperAngleLimit = 20;
    float lowerAngleLimit = 80;
    float maxZoom = 250;
    float minZoom = 10;

    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();
        orbitPoint = transform.parent;
        camOffset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero;



        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        camOffset *= Mathf.Pow(zoomFactor, -scroll);
        if (camOffset.magnitude > maxZoom) camOffset *= (maxZoom / camOffset.magnitude);
        if (camOffset.magnitude < minZoom) camOffset *= (minZoom / camOffset.magnitude);
        if (Input.GetMouseButton(2))
        {
            var dy = -Input.GetAxis("Mouse Y");
            if ((Vector3.Angle(camOffset, Vector3.up) > upperAngleLimit || dy < 0) && (Vector3.Angle(camOffset, Vector3.up) < lowerAngleLimit || dy > 0))
            {
                camOffset = Vector3.RotateTowards(camOffset, Vector3.up, dy * verticalROtationSpeed, 0f);
            }
            var dx = Input.GetAxis("Mouse X");
            camOffset = Quaternion.AngleAxis(dx * horizontalROtationSpeed, Vector3.up) * camOffset;
        }
        var off = camOffset;
        off.y = 0;
        movement = Quaternion.FromToRotation(Vector3.forward, off) * -movement;
        orbitPoint.position += Time.deltaTime * baseMovementspeed * movement * camOffset.magnitude;
        transform.localPosition = camOffset;
        transform.LookAt(orbitPoint, Vector3.up);
    }
}
