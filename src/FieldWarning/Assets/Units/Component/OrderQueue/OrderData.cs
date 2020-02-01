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

using PFW.Units.Component.Movement;
using UnityEngine;

namespace PFW.Units.Component.OrderQueue
{
    public sealed class OrderData
    {
        private OrderData(
            OrderType orderType,
            PlatoonBehaviour platoon,
            Vector3 targetPosition,
            float heading = MovementComponent.NO_HEADING,
            MoveCommandType moveCommandType = MoveCommandType.NORMAL)
        {
            OrderType = orderType;
            Platoon = platoon;
            TargetPosition = targetPosition;
            Heading = heading;
            MoveCommandType = moveCommandType;
        }

        public PlatoonBehaviour Platoon { get; }
        public OrderType OrderType { get; }
        public Vector3 TargetPosition { get; }
        
        #region MOVE_ORDER data
        public float Heading { get; }
        public MoveCommandType MoveCommandType { get; }
        #endregion

        public static OrderData MoveOrder(
            PlatoonBehaviour platoon,
            Vector3 destination,
            float heading = MovementComponent.NO_HEADING,
            MoveCommandType moveCommandType = MoveCommandType.NORMAL)
        {
            return new OrderData(
                OrderType.MOVE_ORDER,
                platoon,
                destination,
                heading,
                moveCommandType);
        }

        public static OrderData FirePositionOrder(
            PlatoonBehaviour platoonBehaviour,
            Vector3 targetGround)
        {
            return new OrderData(
                OrderType.FIRE_POSITION_ORDER,
                platoonBehaviour,
                targetGround);
        }
    }
}