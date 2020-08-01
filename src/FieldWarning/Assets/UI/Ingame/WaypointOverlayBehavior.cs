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
using PFW.Units;
using PFW.Units.Component.OrderQueue;
using UnityEngine;

namespace PFW.UI.Ingame.UnitLabel
{
    /// <summary>
    /// The line that shows ongoing move orders
    /// when a platoon is selected.
    /// </summary>
    public class WaypointOverlayBehavior : MonoBehaviour
    {
        private PlatoonBehaviour _platoon;
        private LineRenderer _lineR;

        private void Update()
        {
            List<OrderData> moveOrders = _platoon.CalculateOrderPreview();

            int moveOrderCount = moveOrders.Count;

            if (moveOrderCount == 0)
            {
                // TODO: should prob just set inactive...
                _lineR.gameObject.SetActive(false);
                return;
            }

            _lineR.gameObject.SetActive(true);


            // +1 for current position (self)
            _lineR.positionCount = moveOrderCount + 1;

            _lineR.SetPosition(0, _platoon.transform.position);

            int idx = 1;

            foreach (OrderData order in moveOrders)
            {
                // +2 for the destination and ourselves previously inserted into this line
                _lineR.SetPosition(idx, order.TargetPosition);
                idx++;
            }
        }

        public void Initialize(PlatoonBehaviour platoon)
        {
            _platoon = platoon;

            _lineR = transform.Find("Line").GetComponent<LineRenderer>();
            _lineR.startColor = Color.yellow;
            _lineR.endColor = Color.yellow;
            _lineR.useWorldSpace = true;
            _lineR.sortingLayerName = "OnTop";
            _lineR.sortingOrder = 20;

            _lineR.startWidth = 0.10f;
            _lineR.endWidth = 0.10f;
        }
    }
}
