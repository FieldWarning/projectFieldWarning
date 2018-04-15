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
    private float zoomTiltSpeed = 0.3f;
    [SerializeField]
    private float minAltitude = 0.2f;
    [SerializeField]
    private float tiltThreshold = 2f;
    [SerializeField]
    private float maxAltitude = 2000;
    
    private float rotateX;
    private float rotateY;
    private float translateX;
    private float translateZ;

    private Camera cam;

    private void Awake() {
        cam = GetComponent<Camera>();
    }


    private void Start() {
        rotateX = transform.eulerAngles.x;
        rotateY = transform.eulerAngles.y;
    }

    // Update() only plans movement; position/rotation are directly changed in LateUpdate().
    private void Update() {
        // Camera panning:
        translateX += Input.GetAxis("Horizontal") * panSpeed * Time.deltaTime;
        translateZ += Input.GetAxis("Vertical") * panSpeed * Time.deltaTime;  
        

        AimedZoom();

        if (Input.GetMouseButton(2)) {
            RotateCamera();
        }
    }
    
    private void LateUpdate() {
        var dx = translateX < panSpeed * Time.deltaTime ? translateX : panSpeed * Time.deltaTime;
        var dz = translateZ < panSpeed * Time.deltaTime ? translateZ : panSpeed * Time.deltaTime;

        transform.Translate(dx * Vector3.right);

        // If we move forward in local space, camera will also change altitude. To properly move forward, we have to rotate the forward vector to be horizontal in world space while keeping the magnitude:
        var worldForward = transform.TransformDirection(Vector3.forward);
        var angle = Quaternion.FromToRotation(worldForward, new Vector3(worldForward.x, 0, worldForward.z));
        transform.position += angle * worldForward * dz;

        translateX -= dx;
        translateZ -= dz;


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

            transform.position = Vector3.MoveTowards(transform.position,
                                                        hit.point,
                                                        scroll * zoomSpeed * Time.deltaTime);
        }

        // Zoom out:
        if (scroll < 0) {
            transform.Translate(Vector3.forward * scroll * zoomSpeed * Time.deltaTime);
        }
        
        if (scroll != 0) {
            TiltLowFlyingCamera(scroll);
            ClampCameraAltitude();
        }
    }

    private void RotateCamera() {
        rotateX += -Input.GetAxis("Mouse Y") * verticalRotationSpeed * Time.deltaTime;
        rotateY += Input.GetAxis("Mouse X") * horizontalRotationSpeed * Time.deltaTime;

        // So we don't look too far up or down:
        rotateX = Mathf.Clamp(rotateX, minCameraAngle, maxCameraAngle);
    }

    // Camera looks down when high and up when low:
    private void TiltLowFlyingCamera(float scroll) {
        if (transform.position.y < tiltThreshold) {
            rotateX -= zoomSpeed * Time.deltaTime * zoomTiltSpeed * scroll;
            rotateX = Mathf.Clamp(rotateX, minCameraAngle, maxCameraAngle);
        }
    }

    private void ClampCameraAltitude() {
        transform.position = new Vector3(transform.position.x,
                                             Mathf.Clamp(transform.position.y, minAltitude, maxAltitude),
                                             transform.position.z);
    }
}