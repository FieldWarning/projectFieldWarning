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

using System.Collections.Generic;
using UnityEngine;

namespace PFW.Loading
{
    /// <summary>
    /// Exists to speed up the vision code.
    /// 
    /// Simple implementation that skips some repeated calculation
    /// of Xiaolin Wu's algorithm.
    /// </summary>
    public class VisionCache
    {
        private const int MAX_CACHE_ENTRIES = 400;
        private const int MAX_INNER_CACHE_ENTRIES = 400;
        private TerrainMap _terrainMap;
        private Dictionary<Vector3, Dictionary<Vector3, float>> _cachedLines;

        public VisionCache(TerrainMap terrainMap)
        {
            _terrainMap = terrainMap;
            _cachedLines = new Dictionary<Vector3, Dictionary<Vector3, float>>();
        }

        public float GetForestLengthOnLine(Vector3 start, Vector3 end)
        {
            float result;

            // Avoid having separate entries for A to B and B to A,
            // by trying to use them in the same order:
            if (start.x > end.x 
                || (start.x == end.x && (start.y > end.y
                                        || (start.y == end.y && start.z > end.z))))
            {
                Util.Swap(ref start, ref end);
            }

            if (_cachedLines.TryGetValue(start, out Dictionary<Vector3, float> childMap))
            {
                if (childMap.TryGetValue(end, out result))
                {
                    // done, we found a cached value from a previous run
                }
                else
                {
                    if (childMap.Count >= MAX_INNER_CACHE_ENTRIES)
                    {
                        childMap.Clear();
                    }

                    result = _terrainMap.GetForestLengthOnLine(start, end);
                    childMap[end] = result;
                }
            }
            else
            {
                if (_cachedLines.Count >= MAX_CACHE_ENTRIES)
                {
                    _cachedLines.Clear();
                }

                result = _terrainMap.GetForestLengthOnLine(start, end);
                _cachedLines[start] = new Dictionary<Vector3, float>
                {
                    [end] = result
                };
            }

            return result;
        }
    }
}
