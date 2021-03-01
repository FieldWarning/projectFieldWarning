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

using PFW.Loading;
using UnityEngine;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// A billboard is a 2d texture that is always facing the camera.
    /// </summary>
    [SelectionBase]
    public class BillboardBehavior : MonoBehaviour
    {
        [SerializeField]
        private float ALTITUDE = 10f * Constants.MAP_SCALE;
        [SerializeField]
        private float SIZE = 0.1f;

        private LoadedData _loadedData;
        private float _adjustedAltitude;

        private void OnEnable()
        {
            _loadedData = FindObjectOfType<LoadedData>();
            if (_loadedData)
                _adjustedAltitude = ALTITUDE + _loadedData.TerrainData.GetTerrainHeight(transform.position);
        }

        private void Update()
        {
            transform.localPosition = _adjustedAltitude * Camera.main.transform.up;
            FaceCamera();
        }

        private void FaceCamera()
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            var distance = (Camera.main.transform.position - transform.position).magnitude;
            transform.localScale = SIZE * distance * Vector3.one;
        }
    }
}
