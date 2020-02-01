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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PFW.Units.Component.OrderQueue
{
    public sealed class OrderQueue
    {
        private readonly Queue<OrderData> _orders = new Queue<OrderData>();

        public IEnumerable<OrderData> Orders => _orders;

        public OrderData ActiveOrder { get; private set; }

        public void SendOrder(OrderData orderData, bool enqueue)
        {
            if (!enqueue)
                Clear();
            _orders.Enqueue(orderData);
        }

        public void HandleUpdate()
        {
            if (ActiveOrder != null && !OrderComplete(ActiveOrder))
                return;

            if (_orders.Any())
            {
                ActiveOrder = _orders.Dequeue();
                ProcessOrder(ActiveOrder);
            }
            else
            {
                ActiveOrder = null;
            }
        }

        private bool OrderComplete(OrderData orderData)
        {
            switch (orderData.OrderType)
            {
                case OrderType.MOVE_ORDER:
                    return orderData.Platoon.Units.All(x => x.AreOrdersComplete());
                case OrderType.FIRE_POSITION_ORDER:
                    return orderData.Platoon.Units.All(u => !u.HasTarget);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessOrder(OrderData orderData)
        {
            switch (orderData.OrderType)
            {
                case OrderType.MOVE_ORDER:
                    List<Vector3> destinations = Formations.GetLineFormation(
                        orderData.TargetPosition, orderData.Heading, orderData.Platoon.Units.Count);

                    for (int i = 0; i < orderData.Platoon.Units.Count; i++)
                        orderData.Platoon.Units[i]
                            .SetDestination(destinations[i], orderData.Heading, orderData.MoveCommandType);

                    return;
                case OrderType.FIRE_POSITION_ORDER:
                    orderData.Platoon.Units
                        .ForEach(u => u.SendFirePosOrder(orderData.TargetPosition));
                    orderData.Platoon.PlayAttackCommandVoiceline();

                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Clear()
        {
            _orders.Clear();
            ActiveOrder = null;
        }
    }
}
