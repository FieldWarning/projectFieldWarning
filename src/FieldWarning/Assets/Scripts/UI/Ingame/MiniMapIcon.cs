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
    public class MiniMapIcon : MonoBehaviour
    {
        private Quaternion _rotation;

        public void Start()
        {
            _rotation = Quaternion.AngleAxis(90, Vector3.right);
        }

        public void LateUpdate()
        {
            transform.rotation = _rotation;
        }
    }
}