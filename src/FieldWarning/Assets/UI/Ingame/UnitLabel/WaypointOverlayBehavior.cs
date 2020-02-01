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

using System.Linq;
using PFW.Units;
using PFW.Units.Component.Movement;
using PFW.Units.Component.OrderQueue;
using UnityEngine;

namespace PFW.UI.Ingame.UnitLabel
{
    public class WaypointOverlayBehavior : MonoBehaviour
    {
        private PlatoonBehaviour _platoon;
        private LineRenderer _lineR;

        public void Destroy()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            var activeOrder = _platoon.OrderQueue.ActiveOrder;
            if (activeOrder == null)
            {
                // TODO: should prob just set inactive...
                _lineR.gameObject.SetActive(false);
                return;
            }


            _lineR.gameObject.SetActive(true);

            // +2 for the active waypoint and our self
            _lineR.positionCount = _platoon.OrderQueue.Orders.Count() + 2;

            _lineR.SetPosition(0, _platoon.transform.position);

            // destination is normally dequeued so we need to get this separately from
            // the rest of the waypoints
            _lineR.SetPosition(1, activeOrder.Destination);

            int idx = 0;
            foreach (IOrder order in _platoon.OrderQueue.Orders)
            {
                // +2 for the destination and ourselves previously inserted into this line
                _lineR.SetPosition(idx + 2, order.Destination);
                idx++;
            }
        }

        public void Initialize(PlatoonBehaviour platoon)
        {
            _platoon = platoon;

            _lineR = transform.Find("Line").GetComponent<LineRenderer>();
            _lineR.startColor = Color.green;
            _lineR.endColor = Color.green;
            _lineR.useWorldSpace = true;
            _lineR.sortingLayerName = "OnTop";
            _lineR.sortingOrder = 20;
        
            _lineR.startWidth = 0.005f;
            _lineR.endWidth = 0.10f;
        }
    }
}
