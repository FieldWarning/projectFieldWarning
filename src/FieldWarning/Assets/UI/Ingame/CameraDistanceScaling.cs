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

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Changes the scale of the GO this script is attached to,
    /// based on the distance to the main camera.
    /// </summary>
    public class CameraDistanceScaling : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If this is set to 50, then for every 50 units the camera moves away, this GO" +
            " will have its x, y, z scale increased by 1. Use 0 for no scaling.")]
        private Vector3 _scalingFactor = Vector3.zero;

        [SerializeField]
        [Tooltip("If this is set to 1000, then the scaling will be calculated as if" +
            " a camera a camera at 500 distance is not at any distance, and" +
            " a camera at 1500 distance is only 500 distance away.")]
        private int _graceFactor = 0;

        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            float distance = Vector3.Distance(
                    gameObject.transform.position,
                    _mainCamera.transform.position);

            distance -= _graceFactor;
            if (distance < 0)
            {
                distance = 0;
            }

            transform.localScale = new Vector3(
                    1 + distance / _scalingFactor.x,
                    1 + distance / _scalingFactor.y,
                    1 + distance / _scalingFactor.z);
        }
    }
}
