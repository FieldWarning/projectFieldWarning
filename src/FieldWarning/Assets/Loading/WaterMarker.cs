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

namespace PFW.Loading 
{
    /// <summary>
    ///     This script helps us find all water when
    ///     generating the terrain cache. It should be placed
    ///     on an object that is the parent of all water
    ///     meshes in the scene.
    /// </summary>
    public class WaterMarker : MonoBehaviour
    {
        /// <summary>
        ///     All children of this object should be
        ///     water meshes, and this finds the 
        ///     one that is placed the highest.
        /// </summary>
        /// <returns></returns>
        public float GetMaxChildHeight()
        {
            return GetMaxChildHeightRecursive(transform);
        }

        private static float GetMaxChildHeightRecursive(Transform transform)
        {
            if (transform.childCount == 0)
                return transform.position.y;

            float result = -10000;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                float childHeight = GetMaxChildHeightRecursive(childTransform);
                if (childHeight > result)
                {
                    result = childHeight;
                }
            }
            return result;
        }
    }
}
