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

/**
 * Sliding camera is our main RTS cam. It is wargame-like and provides almost entirely free movement. Zooming in goes toward the cursor ("sliding"), zooming out moves back and up at a fixed angle. The camera faces up slightly when zoomed all the way into the ground, and tries to restore its facing when zoomed out again.
 * 
 * Restrictions:
 * - Players can't look too far up or down.
 * - There are minimal and maximal altitudes to prevent going below the level.
 * 
 * TODO:
 * - Make the camera stay above terrain deformations.
 * - Maybe prevent the camera from clipping into units.
 * - A/B test values for a better feel.
 */
public class SlidingCameraBehaviour : MonoBehaviour
{
    [Header("Translational Movement")]
    [SerializeField] private float _panSpeed = 50f * TerrainConstants.MAP_SCALE;
    [SerializeField] private float _panLerpSpeed = 100f * TerrainConstants.MAP_SCALE;

    [Header("Rotational Movement")]
    [SerializeField] private float _horizontalRotationSpeed = 600f;
    [SerializeField] private float _verticalRotationSpeed = 600f;
    [SerializeField] private float _rotLerpSpeed = 10f;
    [SerializeField] private float _maxCameraAngle = 85f;
    [SerializeField] private float _minCameraAngle = 5f;

    [Header("Zoom Level")]
    [SerializeField] private float _zoomSpeed = 5000f * TerrainConstants.MAP_SCALE;
    [SerializeField] private float _zoomTiltSpeed = 4f;
    [SerializeField] private float _minAltitude = 1.0f * TerrainConstants.MAP_SCALE;
    [SerializeField] private float _tiltThreshold = 2f;
    [SerializeField] private float _maxAltitude = 20000f * TerrainConstants.MAP_SCALE;
    [SerializeField] private float _heightSpeedScaling = 0.75f;
    [SerializeField] private float _zoomOutAngle = 45f;

    private Vector3 _zoomOutDirection;

    // We store the camera facing and reapply it every LateUpdate() for simplicity:
    private float _rotateX;
    private float _rotateY;

    // Leftover translations from zoom and pan:
    private float _translateX;
    private float _translateZ;
    private float _leftoverZoom = 0f;
    private Vector3 _zoomDestination;

    // All planned transforms are actually applied to a target object, which the camera then lerps to. Maybe it is pointlessly indirect and can be refactored.
    private Vector3 _targetPosition;

    private Camera _cam;

    // If we allow the camera to get to height = 0 we would need special cases for the height scaling.
    private float GetScaledPanSpeed()
    {
        return _panSpeed * Time.deltaTime * Mathf.Pow(transform.position.y, _heightSpeedScaling);
    }

    private float GetScaledZoomSpeed()
    {
        return _zoomSpeed * Time.deltaTime * Mathf.Pow(transform.position.y, _heightSpeedScaling);
    }

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }


    private void Start()
    {
        _rotateX = transform.eulerAngles.x;
        _rotateY = transform.eulerAngles.y;
        _targetPosition = transform.position;

        _zoomOutDirection = Quaternion.AngleAxis(_zoomOutAngle, Vector3.right) * Vector3.back;
    }

    // Update() only plans movement; position/rotation are directly changed in LateUpdate().
    private void Update()
    {
        // Camera panning:
        _translateX += Input.GetAxis("Horizontal") * GetScaledPanSpeed();
        _translateZ += Input.GetAxis("Vertical") * GetScaledPanSpeed();

        AimedZoom();

        if (Input.GetMouseButton(2)) {
            RotateCamera();
        }
    }

    private void LateUpdate()
    {
        var dx = _translateX < GetScaledPanSpeed() ? _translateX : GetScaledPanSpeed();
        var dz = _translateZ < GetScaledPanSpeed() ? _translateZ : GetScaledPanSpeed();

        _targetPosition += transform.TransformDirection(dx * Vector3.right);

        // If we move forward in local space, camera will also change altitude. To properly move forward, we have to rotate the forward vector to be horizontal in world space while keeping the magnitude:
        var worldForward = transform.TransformDirection(Vector3.forward);
        var angle = Quaternion.FromToRotation(worldForward, new Vector3(worldForward.x, 0, worldForward.z));
        _targetPosition += angle * worldForward * dz;

        _translateX -= dx;
        _translateZ -= dz;

        // Apply zoom movement:
        var dzoom = Mathf.Abs(_leftoverZoom) < GetScaledZoomSpeed() ? _leftoverZoom : GetScaledZoomSpeed();
        var oldAltitude = _targetPosition.y;

        // Zoom in:
        if (dzoom > 0) {
            ApplyZoomIn(dzoom);
        } else if (dzoom < 0) {
            ApplyZoomOut(dzoom);
        }

        _leftoverZoom -= dzoom;
        ClampCameraAltitude();
        TiltCameraIfNearGround(oldAltitude);

        // Prevent clipping through hills
        if (_targetPosition.y < Terrain.activeTerrain.SampleHeight(_targetPosition)) {
            _targetPosition.y = Terrain.activeTerrain.SampleHeight(_targetPosition);
        }
        
        // It is mathematically incorrect to directly lerp on deltaTime like this, since we never get to the target (except by rounding I guess):
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _panLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(_rotateX, _rotateY, 0f), Time.deltaTime * _rotLerpSpeed);
    }

    /// <summary>
    /// When zooming in we gradually approach whatever the cursor is pointing at. 
    /// </summary>
    private void AimedZoom()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0)
            return;

        // Zoom toward cursor:
        if (scroll > 0) {
            // Use a ray from the cursor to find what we'll be zooming into:
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the cursor is not pointing at anything, zooming in is forbidden:
            if (!Physics.Raycast(ray, out hit)) {
                return;
            }
            _zoomDestination = hit.point;
        }

        _leftoverZoom += scroll * GetScaledZoomSpeed();
    }

    private void RotateCamera()
    {
        _rotateX += -Input.GetAxis("Mouse Y") * _verticalRotationSpeed * Time.deltaTime;
        _rotateY += Input.GetAxis("Mouse X") * _horizontalRotationSpeed * Time.deltaTime;

        // So we don't look too far up or down:
        _rotateX = Mathf.Clamp(_rotateX, _minCameraAngle, _maxCameraAngle);
    }

    private void ApplyZoomIn(float dzoom)
    {
        _targetPosition = Vector3.MoveTowards(_targetPosition, _zoomDestination, dzoom);
    }

    private void ApplyZoomOut(float dzoom)
    {
        Quaternion rotateToCamFacing = Quaternion.AngleAxis(_rotateY, Vector3.up);
        _targetPosition -= rotateToCamFacing * _zoomOutDirection * dzoom;
    }

    /// <summary>
    /// Camera looks down when high and up when low:
    /// </summary>
    /// <param name="oldAltitude"></param>
    private void TiltCameraIfNearGround(float oldAltitude)
    {
        if (transform.position.y < _tiltThreshold || _targetPosition.y < _tiltThreshold) {

            _rotateX += (_targetPosition.y - oldAltitude) * _zoomTiltSpeed;
            _rotateX = Mathf.Clamp(_rotateX, _minCameraAngle, _maxCameraAngle);
        }
    }

    private void ClampCameraAltitude()
    {
        _targetPosition = new Vector3(
            _targetPosition.x, 
            Mathf.Clamp(_targetPosition.y, _minAltitude, _maxAltitude),
            _targetPosition.z);
    }
}
