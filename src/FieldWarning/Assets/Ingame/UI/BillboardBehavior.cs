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

namespace PFW.Ingame.UI
{
    /*
     * A billboard is a 2d texture that is always facing the camera.
     */
    public class BillboardBehavior : SelectableBehavior
    {
        [SerializeField]
        private float ALTITUDE = 10f * TerrainConstants.MAP_SCALE;
        [SerializeField]
        private float SIZE = 0.1f;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.localPosition = ALTITUDE * Camera.main.transform.up;
            faceCamera();
        }

        private void faceCamera()
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            var distance = (Camera.main.transform.position - transform.position).magnitude;
            transform.localScale = SIZE * distance * Vector3.one;
        }
    }
}