using UnityEngine;

public class SlidingCameraBehaviour : MonoBehaviour {

    [Header("Translational Movement")]
    [SerializeField]
    private float panSpeed = 80f;

    [Header("Rotational Movement")]
    [SerializeField]
    private float horizontalRotationSpeed = 600f;
    [SerializeField]
    private float verticalRotationSpeed = 600f;
    [SerializeField]
    private float maxCameraAngle = 85;
    [SerializeField]
    private float minCameraAngle = 5;

    [Header("Zoom Level")]
    [SerializeField]
    private float zoomSpeed = 3000;
    [SerializeField]
    private float zoomTiltSpeed = 3;
    [SerializeField]
    private float minAltitude = 0.2f;
    [SerializeField]
    private float maxAltitude = 2000;


    private float rotateX;
    private float rotateY;
    private Camera cam;

    private void Awake() {
        cam = GetComponent<Camera>();
    }


    private void Start() {
        rotateX = transform.eulerAngles.x;
        rotateY = transform.eulerAngles.y;
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

    private void LateUpdate() {
        transform.rotation = Quaternion.identity;
        transform.Rotate(rotateX, rotateY, 0f);
    }

    /*
     * When zooming in we gradually approach whatever the cursor is pointing at.
     */
    private void AimedZoom() {
        var scroll = Input.GetAxis("Mouse ScrollWheel");

        // Zoom toward cursor:
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
            // Tilt camera only if it is close to the ground?
            //rotateX += zoomSpeed * Time.deltaTime * zoomTiltSpeed * scroll);
            //Mathf.Clamp(rotateX, minCameraAngle, maxCameraAngle);
            
            ClampCameraAltitude();
        }
    }

    private void RotateCamera() {
        rotateX += -Input.GetAxis("Mouse Y") * verticalRotationSpeed * Time.deltaTime;
        rotateY += Input.GetAxis("Mouse X") * horizontalRotationSpeed * Time.deltaTime;

        // So we don't look too far up or down:
        rotateX = Mathf.Clamp(rotateX, minCameraAngle, maxCameraAngle);
    }

    private void ClampCameraAltitude() {
        cam.transform.position = new Vector3(cam.transform.position.x,
                                             Mathf.Clamp(cam.transform.position.y, minAltitude, maxAltitude),
                                             cam.transform.position.z);
    }
}