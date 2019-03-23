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

using System;
using UnityEngine;
using UnityEngine.UI;

/**
 * Sliding camera is our main RTS cam. It is wargame-like and provides almost entirely free movement. Zooming in goes toward the cursor ("sliding"), zooming out moves back and up at a fixed angle. The camera faces up slightly when zoomed all the way into the ground, and tries to restore its facing when zoomed out again.
 * 
 * Restrictions:
 * - Players can't look too far up or down.
 * - There are minimal and maximal altitudes to prevent going below the level.
 * 
 * TODO:
 * - Maybe prevent the camera from clipping into units.
 * - A/B test values for a better feel.
 */
public class SlidingCameraBehaviour : MonoBehaviour
{
    [Header("Translational Movement")]
    [SerializeField] private float _panSpeed = 50f * TerrainConstants.MAP_SCALE;
    [SerializeField] private float _panLerpSpeed = 100f * TerrainConstants.MAP_SCALE;
    [SerializeField] private float _borderPanningOffset = 2;    //Pixels
    [SerializeField] private float _borderPanningCornerSize = 200;    //Pixels
    [SerializeField] private float _maxCameraHorizontalDistanceFromTerrain = 5000f * TerrainConstants.MAP_SCALE;
    private Image _cornerArrowBottomLeft;
    private Image _cornerArrowBottomRight;
    private Image _cornerArrowTopLeft;
    private Image _cornerArrowTopRight;
    private Image _sideArrowLeft;
    private Image _sideArrowRight;
    private Image _sideArrowTop;
    private Image _sideArrowBottom;

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

        _cornerArrowBottomLeft = GameObject.Find("PanningArrowBottomLeft").GetComponent<Image>();
        if (_cornerArrowBottomLeft == null)
            throw new Exception("No cornerArrowBottomLeft specified!");

        _cornerArrowBottomRight = GameObject.Find("PanningArrowBottomRight").GetComponent<Image>();
        if (_cornerArrowBottomRight == null)
            throw new Exception("No cornerArrowBottomRight specified!");

        _cornerArrowTopLeft = GameObject.Find("PanningArrowTopLeft").GetComponent<Image>();
        if (_cornerArrowTopLeft == null)
            throw new Exception("No cornerArrowTopLeft specified!");

        _cornerArrowTopRight = GameObject.Find("PanningArrowTopRight").GetComponent<Image>();
        if (_cornerArrowTopRight == null)
            throw new Exception("No cornerArrowTopRight specified!");

        _sideArrowLeft = GameObject.Find("PanningArrowLeft").GetComponent<Image>();
        if (_sideArrowLeft == null)
            throw new Exception("No sideArrowLeft specified!");

        _sideArrowRight = GameObject.Find("PanningArrowRight").GetComponent<Image>();
        if (_sideArrowRight == null)
            throw new Exception("No sideArrowRight specified!");

        _sideArrowTop = GameObject.Find("PanningArrowTop").GetComponent<Image>();
        if (_sideArrowTop == null)
            throw new Exception("No sideArrowTop specified!");

        _sideArrowBottom = GameObject.Find("PanningArrowBottom").GetComponent<Image>();
        if (_sideArrowBottom == null)
            throw new Exception("No sideArrowBottom specified!");

    }

    // Update() only plans movement; position/rotation are directly changed in LateUpdate().
    private void Update()
    {
        // Camera panning:
        _translateX += Input.GetAxis("Horizontal") * GetScaledPanSpeed();
        _translateZ += Input.GetAxis("Vertical") * GetScaledPanSpeed();

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) {
            //Try border panning with mouse
            PanFromScreenBorder();
        } else {
            SetPanningCursor(ScreenCorner.None);
        }

        AimedZoom();

        if (Input.GetMouseButton(2)) {
            RotateCamera();
        }

    }

    private void LateUpdate()
    {
        var dx = _translateX < GetScaledPanSpeed() ? _translateX : GetScaledPanSpeed();
        var dz = _translateZ < GetScaledPanSpeed() ? _translateZ : GetScaledPanSpeed();
        _targetPosition = transform.position;
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
        TiltCameraIfNearGround(oldAltitude);
        ClampCameraAltitude();
        ClampCameraXZPosition();

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
        _targetPosition.y = Mathf.Clamp(
                _targetPosition.y,
                Terrain.activeTerrain.SampleHeight(_targetPosition) + _minAltitude,
                _maxAltitude);
    }

    private void PanFromScreenBorder()
    {
        if ((Input.mousePosition.x <= _borderPanningOffset && Input.mousePosition.x >= 0
            && Input.mousePosition.y <= _borderPanningCornerSize && Input.mousePosition.y >= 0)
            || (Input.mousePosition.x <= _borderPanningCornerSize && Input.mousePosition.x >= 0
            && Input.mousePosition.y <= _borderPanningOffset && Input.mousePosition.y >= 0)) { //Lower-left screen corner
            SetPanningCursor(ScreenCorner.BottomLeft);
            _translateX += -1 * GetScaledPanSpeed();
            _translateZ += -1 * GetScaledPanSpeed();

        } else if ((Input.mousePosition.x >= Screen.width - _borderPanningOffset && Input.mousePosition.x <= Screen.width
            && Input.mousePosition.y <= _borderPanningCornerSize && Input.mousePosition.y >= 0)
            || (Input.mousePosition.x >= Screen.width - _borderPanningCornerSize && Input.mousePosition.x <= Screen.width
            && Input.mousePosition.y <= _borderPanningOffset && Input.mousePosition.y >= 0)) {  //Lower-right screen corner
            SetPanningCursor(ScreenCorner.BottomRight);
            _translateX += 1 * GetScaledPanSpeed();
            _translateZ += -1 * GetScaledPanSpeed();

        } else if ((Input.mousePosition.x <= _borderPanningOffset && Input.mousePosition.x >= 0
            && Input.mousePosition.y >= Screen.height - _borderPanningCornerSize && Input.mousePosition.y <= Screen.height)
            || (Input.mousePosition.x <= _borderPanningCornerSize && Input.mousePosition.x >= 0
            && Input.mousePosition.y >= Screen.height - _borderPanningOffset && Input.mousePosition.y <= Screen.height)) {  //Upper-left screen corner
            SetPanningCursor(ScreenCorner.TopLeft);
            _translateX += -1 * GetScaledPanSpeed();
            _translateZ += 1 * GetScaledPanSpeed();

        } else if ((Input.mousePosition.x >= Screen.width - _borderPanningOffset && Input.mousePosition.x <= Screen.width
            && Input.mousePosition.y >= Screen.height - _borderPanningCornerSize && Input.mousePosition.y <= Screen.height)
            || (Input.mousePosition.x >= Screen.width - _borderPanningCornerSize && Input.mousePosition.x <= Screen.width
            && Input.mousePosition.y >= Screen.height - _borderPanningOffset && Input.mousePosition.y <= Screen.height)) {  //Upper-right screen corner
            SetPanningCursor(ScreenCorner.TopRight);
            _translateX += 1 * GetScaledPanSpeed();
            _translateZ += 1 * GetScaledPanSpeed();

        } else {    //Border of screen but not corners
            if (Input.mousePosition.x <= _borderPanningOffset && Input.mousePosition.x >= 0
                && Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height) {  //Left screen side
                SetPanningCursor(ScreenCorner.Left);
                _translateX += -1 * GetScaledPanSpeed();

            } else if (Input.mousePosition.x >= Screen.width - _borderPanningOffset && Input.mousePosition.x <= Screen.width
                && Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height) {  //Right screen side
                SetPanningCursor(ScreenCorner.Right);
                _translateX += 1 * GetScaledPanSpeed();

            } else if (Input.mousePosition.y <= _borderPanningOffset && Input.mousePosition.y >= 0
                && Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width) {   //Bottom screen side
                SetPanningCursor(ScreenCorner.Bottom);
                _translateZ += -1 * GetScaledPanSpeed();

            } else if (Input.mousePosition.y >= Screen.height - _borderPanningOffset && Input.mousePosition.y <= Screen.height
                && Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width) {   //Top screen side
                SetPanningCursor(ScreenCorner.Top);
                _translateZ += 1 * GetScaledPanSpeed();

            } else {
                SetPanningCursor(ScreenCorner.None);
            }
        }

    }

    private void ClampCameraXZPosition()
    {
        _targetPosition.x = Mathf.Clamp(
                _targetPosition.x,
                Terrain.activeTerrain.GetPosition().x - _maxCameraHorizontalDistanceFromTerrain,
                Terrain.activeTerrain.GetPosition().x + Terrain.activeTerrain.terrainData.size.x + _maxCameraHorizontalDistanceFromTerrain);
        _targetPosition.z = Mathf.Clamp(
                _targetPosition.z,
                Terrain.activeTerrain.GetPosition().z - _maxCameraHorizontalDistanceFromTerrain,
                Terrain.activeTerrain.GetPosition().z + Terrain.activeTerrain.terrainData.size.z + _maxCameraHorizontalDistanceFromTerrain);
    }

    private void SetPanningCursor(ScreenCorner corner)
    {
        Cursor.visible = false;
        DisableAllPanningArrows();
        switch (corner) {
        case ScreenCorner.TopLeft:
            _cornerArrowTopLeft.transform.position = Input.mousePosition;
            _cornerArrowTopLeft.enabled = true;
            break;
        case ScreenCorner.TopRight:
            _cornerArrowTopRight.transform.position = Input.mousePosition;
            _cornerArrowTopRight.enabled = true;
            break;
        case ScreenCorner.BottomLeft:
            _cornerArrowBottomLeft.transform.position = Input.mousePosition;
            _cornerArrowBottomLeft.enabled = true;
            break;
        case ScreenCorner.BottomRight:
            _cornerArrowBottomRight.transform.position = Input.mousePosition;
            _cornerArrowBottomRight.enabled = true;
            break;
        case ScreenCorner.Top:
            _sideArrowTop.transform.position = Input.mousePosition;
            _sideArrowTop.enabled = true;
            break;
        case ScreenCorner.Bottom:
            _sideArrowBottom.transform.position = Input.mousePosition;
            _sideArrowBottom.enabled = true;
            break;
        case ScreenCorner.Left:
            _sideArrowLeft.transform.position = Input.mousePosition;
            _sideArrowLeft.enabled = true;
            break;
        case ScreenCorner.Right:
            _sideArrowRight.transform.position = Input.mousePosition;
            _sideArrowRight.enabled = true;
            break;
        case ScreenCorner.None:
            Cursor.visible = true;
            break;
        default:
            break;
        }
    }

    private void DisableAllPanningArrows()
    {
        _cornerArrowBottomLeft.enabled = false;
        _cornerArrowBottomRight.enabled = false;
        _cornerArrowTopLeft.enabled = false;
        _cornerArrowTopRight.enabled = false;
        _sideArrowLeft.enabled = false;
        _sideArrowRight.enabled = false;
        _sideArrowTop.enabled = false;
        _sideArrowBottom.enabled = false;
    }

    enum ScreenCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top,
        Bottom,
        Left,
        Right,
        None
    }
}
