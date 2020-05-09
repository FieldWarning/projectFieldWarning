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
using UnityEngine;

namespace PFW.UI.Ingame.UnitLabel
{
    /// <summary>
    /// The line with range and los indicator
    /// that shows up when previewing a fire pos command.
    /// </summary>
    public class TargetingOverlay : MonoBehaviour
    {
        private PlatoonBehaviour _platoon;
        private LineRenderer _lineR;
        
        public int PlaceTargetingPreview(Vector3 targetPosition)
        {
            _lineR.SetPosition(0, _platoon.Units[0].transform.position);
            _lineR.SetPosition(1, targetPosition);
            float distance = Vector3.Distance(
                _platoon.Units[0].transform.position, targetPosition);
            return (int)(distance / Constants.MAP_SCALE);
        }

        public void Initialize(PlatoonBehaviour platoon)
        {
            _platoon = platoon;

            _lineR = transform.Find("Line").GetComponent<LineRenderer>();
            _lineR.startColor = Color.cyan;
            _lineR.endColor = Color.cyan;
            _lineR.useWorldSpace = true;
            _lineR.sortingLayerName = "OnTop";
            _lineR.sortingOrder = 21;

            _lineR.startWidth = 0.005f;
            _lineR.endWidth = 0.10f;
            _lineR.positionCount = 2;
        }
    }
}
