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
    
    // We store the camera facing and reapply it every LateUpdate() for simplicity:
    private float rotateX;
    private float rotateY;

    // Leftover translations from zoom and pan:
    private float translateX;
    private float translateZ;
    private float leftoverZoom = 0f;
    private Vector3 zoomDestination;

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

        // Apply zoom movement:
        var dzoom = Mathf.Abs(leftoverZoom) < zoomSpeed * Time.deltaTime ? leftoverZoom : zoomSpeed * Time.deltaTime;
        if (dzoom > 0) {
            transform.position = Vector3.MoveTowards(transform.position, zoomDestination, dzoom);
        } else if (dzoom < 0) {
            transform.Translate(Vector3.forward * dzoom);
        }
        leftoverZoom -= dzoom;

        TiltCameraIfNearGround(dzoom);

        transform.rotation = Quaternion.identity;
        transform.Rotate(rotateX, rotateY, 0f);


        ClampCameraAltitude();
    }

    /*
     * When zooming in we gradually approach whatever the cursor is pointing at.
     */
    private void AimedZoom() {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0)
            return;

        // Zoom toward cursor:
        if (scroll > 0) {
            // Use a ray from the cursor to find what we'll be zooming into:
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the cursor is not pointing at anything, zooming in is forbidden:
            if (!Physics.Raycast(ray, out hit)) {
                return;
            }
            zoomDestination = hit.point;
        }
        
        leftoverZoom += scroll * zoomSpeed * Time.deltaTime;
    }

    private void RotateCamera() {
        rotateX += -Input.GetAxis("Mouse Y") * verticalRotationSpeed * Time.deltaTime;
        rotateY += Input.GetAxis("Mouse X") * horizontalRotationSpeed * Time.deltaTime;

        // So we don't look too far up or down:
        rotateX = Mathf.Clamp(rotateX, minCameraAngle, maxCameraAngle);
    }

    // Camera looks down when high and up when low:
    private void TiltCameraIfNearGround(float dzoom) {
        if (transform.position.y < tiltThreshold) {
            rotateX -= zoomTiltSpeed * dzoom;
            rotateX = Mathf.Clamp(rotateX, minCameraAngle, maxCameraAngle);
        }
    }

    private void ClampCameraAltitude() {
        transform.position = new Vector3(transform.position.x,
                                             Mathf.Clamp(transform.position.y, minAltitude, maxAltitude),
                                             transform.position.z);
    }
}