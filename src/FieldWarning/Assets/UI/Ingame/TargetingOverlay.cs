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
using System.Linq;
using PFW.Units;
using PFW.Units.Component.OrderQueue;
using PFW.Units.Component.Vision;
using UnityEngine;

namespace PFW.UI.Ingame.UnitLabel
{
    /// <summary>
    /// The line with range and los indicator
    /// that shows up when previewing a fire pos command.
    /// </summary>
    public class TargetingOverlay : MonoBehaviour
    {
        private UnitDispatcher _unit;
        [SerializeField]
        private LineRenderer _successLine = null;
        [SerializeField]
        private LineRenderer _errorLine = null;

        public int PlaceTargetingPreview(Vector3 targetPosition)
        {
            Vector3 farthestVisiblePoint =
                    FindFarthestVisiblePosition(targetPosition);

            float fireRange = _unit.MaxFirePosRange();
            bool isArty = fireRange == _unit.MaxFirePosRangeIndirectFire();

            float visionBlockerDistance = Vector3.Distance(
                _unit.transform.position, farthestVisiblePoint);
            if (fireRange > visionBlockerDistance && !isArty)
            {
                // Cant reach due to vision blockers,
                // or can reach (=> farthestVisiblePoint = targetPosition)
                _successLine.SetPosition(0, _unit.transform.position);
                _successLine.SetPosition(1, farthestVisiblePoint);
                _errorLine.SetPosition(0, farthestVisiblePoint);
                _errorLine.SetPosition(1, targetPosition);
            }
            else
            {
                // Cant reach due to firing range
                Vector3 farthestHittableTarget = Vector3.MoveTowards(
                        _unit.transform.position, targetPosition, fireRange);

                _successLine.SetPosition(0, _unit.transform.position);
                _successLine.SetPosition(1, farthestHittableTarget);
                _errorLine.SetPosition(0, farthestHittableTarget);
                _errorLine.SetPosition(1, targetPosition);
            }

            float fullDistance = Vector3.Distance(
                _unit.transform.position, targetPosition);
            return (int)(fullDistance / Constants.MAP_SCALE);
        }

        /// <summary>
        /// Same as PlaceTargetingPreview, except does not draw a red line
        /// when the max weapon range is reached.
        /// </summary>
        public int PlaceVisionPreview(Vector3 targetPosition)
        {
            Vector3 farthestVisiblePoint =
                    FindFarthestVisiblePosition(targetPosition);

            _successLine.SetPosition(0, _unit.transform.position);
            _successLine.SetPosition(1, farthestVisiblePoint);
            _errorLine.SetPosition(0, farthestVisiblePoint);
            _errorLine.SetPosition(1, targetPosition);

            float fullDistance = Vector3.Distance(
                _unit.transform.position, targetPosition);
            return (int)(fullDistance / Constants.MAP_SCALE);
        }

        /// <summary>
        /// Given a target point, find the farthest visible point on the axis
        /// between the unit and the target. If the line of sight is good,
        /// the result will be the point originally given as target.
        /// </summary>
        private Vector3 FindFarthestVisiblePosition(Vector3 targetPosition)
        {
            VisionComponent.IsInHardLineOfSightFast(
                    _unit.gameObject.transform.position, 
                    targetPosition, 
                    out Vector3 farthestVisiblePointHard);
            IsInSoftLineOfSightIterative(
                    targetPosition, out Vector3 farthestVisiblePointSoft);

            // The farthest visible point for the unit is the >closer< 
            // of the failure points of the two los systems
            Vector3 farthestVisiblePoint;
            if (Vector3.Distance(
                        _unit.transform.position, farthestVisiblePointHard) >
                Vector3.Distance(
                        _unit.transform.position, farthestVisiblePointSoft))
            {
                farthestVisiblePoint = farthestVisiblePointSoft;
            }
            else
            {
                farthestVisiblePoint = farthestVisiblePointHard;
            }

            return farthestVisiblePoint;
        }

        /// <summary>
        /// Call IsInSoftLineOfSight() repeatedly until we find
        /// the approximately farthest visible point. 
        /// </summary>
        /// This is kinda bad, but performance shouldnt matter.
        private bool IsInSoftLineOfSightIterative(
                Vector3 targetPosition,
                out Vector3 farthestVisiblePoint) 
        {
            bool result = _unit.VisionComponent.IsInSoftLineOfSight(
                    targetPosition, 1);

            if (result)
            {
                farthestVisiblePoint = targetPosition;
            }
            else 
            {
                const int GRANULARITY = 20;
                Vector3 partwayPoint = Vector3.zero;

                for (int i = 1; i < GRANULARITY; i++)
                {
                    partwayPoint = Vector3.Lerp(
                            targetPosition, 
                            _unit.transform.position, 
                            (i / (float)GRANULARITY));

                    if (_unit.VisionComponent.IsInSoftLineOfSight(
                            partwayPoint, 1))
                    {
                        break;
                    }
                }

                farthestVisiblePoint = partwayPoint;
            }

            return result;
        }

        public void Initialize(UnitDispatcher unit)
        {
            _unit = unit;

            _successLine.startColor = Color.cyan;
            _successLine.endColor = Color.cyan;
            _successLine.useWorldSpace = true;
            _successLine.sortingLayerName = "OnTop";
            _successLine.sortingOrder = 21;

            _successLine.startWidth = 0.005f;
            _successLine.endWidth = 0.10f;
            _successLine.positionCount = 2;

            // The line shown after a los block
            _errorLine.startColor = Color.red;
            _errorLine.endColor = Color.red;
            _errorLine.useWorldSpace = true;
            _errorLine.sortingLayerName = "OnTop";
            _errorLine.sortingOrder = 21;

            _errorLine.startWidth = 0.10f;
            _errorLine.endWidth = 0.10f;
            _errorLine.positionCount = 2;
        }
    }
}
