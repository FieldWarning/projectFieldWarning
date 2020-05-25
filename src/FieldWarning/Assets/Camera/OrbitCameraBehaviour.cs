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
using static PFW.SlidingCameraBehaviour;
using PFW.Model;

namespace PFW
{
    /// <summary>
    /// A camera moved around a pivot. 
    /// 
    /// Currently used for the follow cam, 
    /// and it may find use in the armory in the future.
    /// </summary>
    public class OrbitCameraBehaviour : MonoBehaviour
    {
        private Vector3 _camOffset;
        private Transform _orbitPoint;

        [SerializeField]
        private float _borderPanningOffset = 2; // Pixels
        [SerializeField]
        private float _borderPanningCornerSize = 200; // Pixels

        private const float BASE_MOVEMENT_SPEED = 1.5f;
        private float _zoomFactor = 1.6f;
        private float _horizontalROtationSpeed = 5f;
        private float _verticalROtationSpeed = .1f;
        private float _upperAngleLimit = 20f;
        private float _lowerAngleLimit = 80f;
        private float _maxZoom = 250f;
        private float _minZoom = 10f;


        //[SerializeField]
        //private float _panSpeed = 50f * Constants.MAP_SCALE;
        //[SerializeField]
        //private float _panLerpSpeed = 100f * Constants.MAP_SCALE;
        [SerializeField]
        private float _rotLerpSpeed = 10f;

        private float _zoomSpeed => GameSession.Singleton.Settings.CameraSettings.ZoomSpeed;

        static public GameObject FollowObject = null;

        private void Start()
        {
            _orbitPoint = transform.parent;
            _camOffset = transform.localPosition;
        }

        private void Update()
        {
            Vector3 movement = Vector3.zero;

            float xin = Input.GetAxis("Horizontal");
            float zin = Input.GetAxis("Vertical");

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


            ScreenCorner corner = SlidingCameraBehaviour.GetScreenCornerForMousePosition(
                    _borderPanningOffset, _borderPanningCornerSize);

            if (corner != ScreenCorner.None || xin != 0 || zin != 0)
            {
                gameObject.GetComponentInParent<SlidingCameraBehaviour>().enabled = true;
                enabled = false;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            _camOffset *= Mathf.Pow(_zoomFactor, -scroll);

            if (_camOffset.magnitude > _maxZoom) _camOffset *= (_maxZoom / _camOffset.magnitude);
            if (_camOffset.magnitude < _minZoom) _camOffset *= (_minZoom / _camOffset.magnitude);

            if (Input.GetMouseButton(2))
            {
                float dy = -Input.GetAxis("Mouse Y");
                if ((Vector3.Angle(_camOffset, Vector3.up) > _upperAngleLimit || dy < 0)
                    && (Vector3.Angle(_camOffset, Vector3.up) < _lowerAngleLimit || dy > 0))
                {
                    _camOffset = Vector3.RotateTowards(
                            _camOffset, Vector3.up, dy * _verticalROtationSpeed, 0f);
                }
                float dx = Input.GetAxis("Mouse X");
                _camOffset = Quaternion.AngleAxis(dx * _horizontalROtationSpeed, Vector3.up) * _camOffset;
            }

            Vector3 off = _camOffset;
            off.y = 0;
            movement = Quaternion.FromToRotation(Vector3.forward, off) * -movement;
            _orbitPoint.position += Time.deltaTime * BASE_MOVEMENT_SPEED * movement * _camOffset.magnitude;
            if (FollowObject)
            {
                // maybe add a lerp here for smoothness
                _orbitPoint.position = Vector3.Lerp(
                        _orbitPoint.position,
                        FollowObject.transform.position,
                        Time.deltaTime * _rotLerpSpeed);

            }
            transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    _camOffset,
                    Time.deltaTime * _zoomSpeed);

            // smooth the lookat/rotation
            Quaternion lookOnLook = Quaternion.LookRotation(
                    _orbitPoint.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(
                    transform.rotation, lookOnLook, Time.deltaTime * _rotLerpSpeed);

            // old code just in case
            //transform.LookAt(orbitPoint, Vector3.up);
        }

        // this is needed to reset our zoom level after we 
        // are enabled again. We dont want to suddenly jump
        // back to the zoom from before we were disabled.
        private void OnEnable()
        {
            _camOffset = transform.localPosition;
        }
    }
}
