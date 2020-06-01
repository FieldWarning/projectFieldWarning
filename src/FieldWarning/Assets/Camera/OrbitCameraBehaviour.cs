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
        private GameObject _target = null;

        [SerializeField]
        private float _borderPanningOffset = 2; // Pixels
        [SerializeField]
        private float _borderPanningCornerSize = 200; // Pixels

        private float _zoomFactor = 3.6f;
        private float _horizontalRotationSpeed = 5f;
        private float _verticalRotationSpeed = .1f;
        private float _upperAngleLimit = 20f;
        private float _lowerAngleLimit = 80f;
        public float _maxZoom = 350f;
        public float minZoom = 2;

        [SerializeField]
        private float _rotLerpSpeed = 10f;

        [SerializeField]
        private float _zoomSpeed => GameSession.Singleton.Settings.CameraSettings.ZoomSpeed;


        public void SetTarget(GameObject t)
        {
            _target = t;
            _camOffset = transform.position - _target.transform.position;
        }


        private void Start()
        {
            _camOffset = transform.position;
            if (_target)
            {
                _camOffset = transform.position - _target.transform.position;
            }
        }

        private void Update()
        {
            float xin = Input.GetAxis("Horizontal");
            float zin = Input.GetAxis("Vertical");


            ScreenCorner corner = SlidingCameraBehaviour.GetScreenCornerForMousePosition(
                    _borderPanningOffset, _borderPanningCornerSize);

            // if we translate the camera manually then take us off orbit mode.. This will need
            // to be refactored to make this entire component more flexible.
            if (corner != ScreenCorner.None || xin != 0 || zin != 0)
            {
                gameObject.GetComponentInParent<SlidingCameraBehaviour>().enabled = true;
                enabled = false;
            }

            if (!_target)
            {
                return;

            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                // do not allow us to scroll past a certain point
                if ((_camOffset.magnitude < _maxZoom || scroll > 0) && (_camOffset.magnitude > minZoom || scroll < 0))
                {
                    Vector3 calcOffset = transform.position;

                    // lerp the zoom factor to zoom fast when far away from target and much slower when close
                    float zoomFNew = Mathf.Lerp(_zoomFactor / 10, _zoomFactor * 4, _camOffset.magnitude / _maxZoom);

                    calcOffset += transform.forward * scroll * _zoomSpeed * zoomFNew;
                    _camOffset = (calcOffset - _target.transform.position);
                }
            }

            if (Input.GetMouseButton(2))
            {
                float dy = -Input.GetAxis("Mouse Y");
                float dx = Input.GetAxis("Mouse X");

                if ((Vector3.Angle(_camOffset, Vector3.up) > _upperAngleLimit || dy < 0)
                    && (Vector3.Angle(_camOffset, Vector3.up) < _lowerAngleLimit || dy > 0))
                {
                    _camOffset = Vector3.RotateTowards(_camOffset, Vector3.up, dy * _verticalRotationSpeed, 0f);
                }

                _camOffset = Quaternion.AngleAxis(dx * _horizontalRotationSpeed, Vector3.up) * _camOffset;

            }

            transform.position = Vector3.Lerp(transform.position, _target.transform.position + _camOffset, Time.deltaTime * _rotLerpSpeed);

            // if we are not rotating.. then we want a smooth look at.. means we are switching units
            // if we dont have this logic here our rotation becomes too smooth and looks unnatural
            if (!Input.GetMouseButton(2))
            {
                //// smooth the lookat/rotation
                Quaternion lookOnLook = Quaternion.LookRotation(
                        _target.transform.position - transform.position);
                transform.rotation = Quaternion.Slerp(
                        transform.rotation, lookOnLook, Time.deltaTime * _rotLerpSpeed);
            }
            else
            {
                transform.LookAt(_target.transform.position);
            }
        }

        // this is needed to reset our zoom level after we are enabled again. 
        // We dont want to suddenly jump
        // back to the zoom before we were disabled.
        private void OnEnable()
        {
            _camOffset = transform.position;
            if (_target)
            {
                _camOffset = transform.position - _target.transform.position;
            }
        }
    }
}
