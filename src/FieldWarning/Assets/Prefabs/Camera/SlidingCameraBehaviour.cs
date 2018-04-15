using UnityEngine;

public class SlidingCameraBehaviour : MonoBehaviour {

    [Header("Translational Movement")]
    [SerializeField]
    private float panSpeed = 20f;

    [Header("Rotational Movement")]
    [SerializeField]
    private float horizontalRotationSpeed = 400f;
    [SerializeField]
    private float verticalRotationSpeed = 400f;
    [SerializeField]
    private float maxCameraAngle = 85;
    [SerializeField]
    private float minCameraAngle = 5;

    [Header("Zoom Level")]
    [SerializeField]
    private float zoomSpeed = 1000;
    [SerializeField]
    private float zoomTiltSpeed = 3;
    [SerializeField]
    private int minAltitude = 2;
    [SerializeField]
    private int maxAltitude = 2000;

    private Camera cam;

    private void Awake() {
        cam = GetComponent<Camera>();
    }


    private void Start() {

    }

    private void Update() {
        // Camera panning:
        // TODO normalize before translation so panning does not slow down if we look down
        var y = cam.transform.position.y;
        cam.transform.Translate(Input.GetAxis("Horizontal") * Vector3.right * panSpeed * Time.deltaTime);
        cam.transform.Translate(Input.GetAxis("Vertical") * Vector3.forward * panSpeed * Time.deltaTime);

        // Panning shouldn't change cam altitude:
        cam.transform.position = new Vector3(cam.transform.position.x, y, cam.transform.position.z);

        // Zoom:
        AimedZoom();

        if (Input.GetMouseButton(2)) {
            RotateCamera();
        }
    }

    /*
     * When zooming in we gradually approach whatever the cursor is pointing at
     */
    private void AimedZoom() {
        var scroll = Input.GetAxis("Mouse ScrollWheel");

        // Zoom toward cursor and gradually tilt camera up:
        if (scroll > 0) {
            // Use a ray from the cursor to find what we'll be zooming into:
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the cursor is not pointing at anything, zooming in is forbidden:
            if (!Physics.Raycast(ray, out hit)) {
                return;
            }

            cam.transform.position = Vector3.MoveTowards(cam.transform.position,
                                                        hit.point,
                                                        scroll * zoomSpeed * Time.deltaTime);
        }

        // Zoom out:
        if (scroll < 0) {
            cam.transform.Translate(Vector3.forward * scroll * zoomSpeed * Time.deltaTime);
        }

        // Tilt camera up/down slightly and limit its angle and altitude:
        if (scroll != 0) {
            cam.transform.Rotate(Vector3.left * zoomSpeed * Time.deltaTime * zoomTiltSpeed * scroll);

            ClampCameraAngle(scroll > 0);
            ClampCameraAltitude();
        }
    }

    private void RotateCamera() {
        var dy = Input.GetAxis("Mouse Y") * verticalRotationSpeed * Time.deltaTime;
        var dx = -Input.GetAxis("Mouse X") * horizontalRotationSpeed * Time.deltaTime;
        cam.transform.eulerAngles -= new Vector3(dy, dx, 0);

        ClampCameraAngle(dy > 0);
    }

    /*
     * So we don't look too far up or down:
     */
    private void ClampCameraAngle(bool wasRotatedDownwards) {
        //  TODO: TO fix gimbal lock, we need to clamp with quaternions; see https://forum.unity.com/threads/how-do-i-clamp-a-quaternion.370041/

        //Debug.LogFormat("{0}; {1}; {2} \n", Quaternion.Angle(cam.transform.rotation, Quaternion.Euler(0, 0, 0)), 

        //    Quaternion.Angle(cam.transform.rotation, Quaternion.Euler((maxCameraAngle + minCameraAngle) / 2, 180, 0)), 

        //    Quaternion.Angle(cam.transform.rotation, Quaternion.Euler((maxCameraAngle + minCameraAngle) / 2, 0, 0))
        //    - Quaternion.Angle(cam.transform.rotation, Quaternion.Euler((maxCameraAngle + minCameraAngle) / 2, 180, 0)));

        //if (Quaternion.Angle(cam.transform.rotation, Quaternion.Euler((maxCameraAngle + minCameraAngle) / 2, 0,0)) 
        //    - Quaternion.Angle(cam.transform.rotation, Quaternion.Euler((maxCameraAngle + minCameraAngle) / 2, 180, 180)) 
        //    > (maxCameraAngle + minCameraAngle) / 2) {

        if (cam.transform.rotation.x > maxCameraAngle || cam.transform.eulerAngles.x < minCameraAngle) {
            cam.transform.eulerAngles = new Vector3(wasRotatedDownwards ? minCameraAngle : maxCameraAngle,
                                                    cam.transform.eulerAngles.y,
                                                    cam.transform.eulerAngles.z);
        }
    }

    private void ClampCameraAltitude() {
        cam.transform.position = new Vector3(cam.transform.position.x,
                                             Mathf.Clamp(cam.transform.position.y, minAltitude, maxAltitude),
                                             cam.transform.position.z);
    }
}