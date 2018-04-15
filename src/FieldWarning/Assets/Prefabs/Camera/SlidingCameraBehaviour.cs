using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingCameraBehaviour : MonoBehaviour {

    [Header("Translational Movement")]
    [SerializeField] private float baseMovementspeed = 1.5f;

    [Header("Rotational Movement")]
    [SerializeField] private float horizontalRotationSpeed = 5;
    [SerializeField] private float verticalRotationSpeed = .1f;
    [SerializeField] private float upperAngleLimit = 20;
    [SerializeField] private float lowerAngleLimit = 80;

    [Header("Zoom Level")]
    [SerializeField] private float zoomSpeed = 1f;
    public int InitialZoomLevel = 0; //The Zoom Level at which the camera is set on level load.
    [SerializeField] private int currentZoomLevel; //The current Zoom Level of the camera.
    [SerializeField] private int minimumZoomLevel = 2;
    [SerializeField] private float minZoomLevelDistance = 0.5f;
    [SerializeField] private int maximumZoomLevel = 20;

    private Transform orbitPoint; //The transform around which the camera orbits.
    private Vector3 targetLocalPosition;
    private bool lerpingToTarget;
    private float distanceToStopLerpingToTarget = 0.01f;
    private Camera cam;

    private void Awake()
    {
        orbitPoint = transform.parent;
        cam = GetComponent<Camera>();
    }


    private void Start()
    {
        targetLocalPosition = new Vector3(0, 1, 0); //Initializes the target local position of the camera.

        ChangeZoomLevel(InitialZoomLevel); //Sets the camera's zoom level to its initial value.
	}
	
	private void Update()
    {
        //If movement key is received, move the orbit point.
        if (Input.GetKey(KeyCode.W))
        {
            MoveOrbitPoint(Vector3.forward);
        }
        if (Input.GetKey(KeyCode.S))
        {
            MoveOrbitPoint(Vector3.back);
        }
        if (Input.GetKey(KeyCode.A))
        {
            MoveOrbitPoint(Vector3.left);
        }
        if (Input.GetKey(KeyCode.D))
        {
            MoveOrbitPoint(Vector3.right);
        }

        //If mouse scroll wheel moved, change the zoom level.
        if (Input.mouseScrollDelta.y < -0.5f)
        {
            ChangeZoomLevel(currentZoomLevel + 1);
        }

        if (Input.mouseScrollDelta.y > 0.5f)
        {
            ChangeZoomLevel(currentZoomLevel - 1);
        }

        RotateCamera(); //Run logic for rotating the camera.
        
        //Lerps the position of the actual local position toward the target local position.
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targetLocalPosition, Time.deltaTime * zoomSpeed); 


        transform.LookAt(orbitPoint); //Makes sure the camera is always looking at the Orbit Point.
	}

    private void MoveOrbitPoint(Vector3 movementVector)
    {
        var off = targetLocalPosition;
        off.y = 0;
        movementVector = Quaternion.FromToRotation(Vector3.forward, off) * -movementVector;
        orbitPoint.position += Time.deltaTime * baseMovementspeed * movementVector * targetLocalPosition.magnitude;
    }
    
    private void ChangeZoomLevel(int newZoomLevel)
    {
        //Makes sure the new zoom level is within its limits.
        if (newZoomLevel < minimumZoomLevel)
        {
            newZoomLevel = minimumZoomLevel;
        }

        if (newZoomLevel > maximumZoomLevel)
        {
            newZoomLevel = maximumZoomLevel;
        }

        //The new target local position is the direction of the target from the orbit point multiplied by the zoom distance.
        targetLocalPosition = ((cam.transform.localPosition - orbitPoint.position).normalized * minZoomLevelDistance) * newZoomLevel * newZoomLevel;

        //Sets the current zoom level to the new zoom level.
        currentZoomLevel = newZoomLevel;
    }

    private void RotateCamera()
    {
        if (Input.GetMouseButton(2))
        {
            var dy = -Input.GetAxis("Mouse Y");
            if ((Vector3.Angle(targetLocalPosition, Vector3.up) > upperAngleLimit || dy < 0) && (Vector3.Angle(targetLocalPosition, Vector3.up) < lowerAngleLimit || dy > 0))
            {
                targetLocalPosition = Vector3.RotateTowards(targetLocalPosition, Vector3.up, dy * verticalRotationSpeed, 0f);
            }
            var dx = Input.GetAxis("Mouse X");
            targetLocalPosition = Quaternion.AngleAxis(dx * horizontalRotationSpeed, Vector3.up) * targetLocalPosition;
        }
    }
    
}
