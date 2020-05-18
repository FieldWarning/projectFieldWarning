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

namespace PFW
{
    public static class Util
    {
        public static bool GetTerrainClickLocation(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Find the first child by name, depth-first search.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform result = RecursiveFindChild(child, childName);
                    if (result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public static void Swap(ref int x, ref int y)
        {
            int tmp = x;
            x = y;
            y = tmp;
            return;
        }
    }
}
