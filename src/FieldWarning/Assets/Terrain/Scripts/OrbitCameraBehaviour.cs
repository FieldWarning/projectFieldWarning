/**
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
 */
 
using UnityEngine;
using UnityEngine.EventSystems;
using static SlidingCameraBehaviour;

/// <summary>
/// A camera moved around a pivot. 
/// 
/// Currently not used, but it may find use in the armory in the future.
/// </summary>
public class OrbitCameraBehaviour : MonoBehaviour
{
    Camera cam;
    Vector3 camOffset;
    Transform orbitPoint;

    [SerializeField]
    private float _borderPanningOffset = 2; // Pixels
    [SerializeField]
    private float _borderPanningCornerSize = 200; // Pixels

    const float BASE_MOVEMENT_SPEED = 1.5f;
    float zoomFactor = 1.6f;
    float horizontalROtationSpeed = 5f;
    float verticalROtationSpeed = .1f;
    float upperAngleLimit = 20f;
    float lowerAngleLimit = 80f;
    float maxZoom = 250f;
    float minZoom = 10f;

    static public GameObject FollowObject = null;
    private Vector3 FollowDistance;


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

        var xin = Input.GetAxis("Horizontal");
        var zin = Input.GetAxis("Vertical");

        //TODO: Will need to clean this up.. maybe make some basic camera input util class or something
        /**
        if (Input.GetKey(KeyCode.W)) {
            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S)) {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A)) {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D)) {
            movement += Vector3.right;
        }
        **/
        

        var corner = SlidingCameraBehaviour.GetScreenCornerForMousePosition(_borderPanningOffset, _borderPanningCornerSize);

        if (corner != ScreenCorner.None || xin != 0 || zin != 0)
        {
            gameObject.GetComponentInParent<SlidingCameraBehaviour>().enabled = true;
            enabled = false;
        }

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        camOffset *= Mathf.Pow(zoomFactor, -scroll);

        if (camOffset.magnitude > maxZoom) camOffset *= (maxZoom / camOffset.magnitude);
        if (camOffset.magnitude < minZoom) camOffset *= (minZoom / camOffset.magnitude);

        if (Input.GetMouseButton(2)) {
            var dy = -Input.GetAxis("Mouse Y");
            if ((Vector3.Angle(camOffset, Vector3.up) > upperAngleLimit || dy < 0) && (Vector3.Angle(camOffset, Vector3.up) < lowerAngleLimit || dy > 0)) {
                camOffset = Vector3.RotateTowards(camOffset, Vector3.up, dy * verticalROtationSpeed, 0f);
            }
            var dx = Input.GetAxis("Mouse X");
            camOffset = Quaternion.AngleAxis(dx * horizontalROtationSpeed, Vector3.up) * camOffset;
        }

        var off = camOffset;
        off.y = 0;
        movement = Quaternion.FromToRotation(Vector3.forward, off) * -movement;
        orbitPoint.position += Time.deltaTime * BASE_MOVEMENT_SPEED * movement * camOffset.magnitude;
        if (FollowObject)
        {
            orbitPoint.position = FollowObject.transform.position;

        }

        transform.localPosition = camOffset;
        transform.LookAt(orbitPoint, Vector3.up);
    }

    // this is needed to reset our zoom level after we are enabled again. We dont want to suddenly jump
    // back to the zoom before we were disabled.
    void OnEnable()
    {
        camOffset = transform.localPosition;
    }
}
