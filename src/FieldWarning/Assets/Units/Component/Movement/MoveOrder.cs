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
using PFW.Units.Component.OrderQueue;
using UnityEngine;

namespace PFW.Units.Component.Movement
{
    public sealed class MoveOrder : OrderBase
    {
        private readonly float _heading;
        private readonly MoveCommandType _moveMode;
        private readonly PlatoonBehaviour _platoon;

        public MoveOrder(
            PlatoonBehaviour platoon,
            Vector3 destination,
            float heading = MovementComponent.NO_HEADING,
            MoveCommandType mode = MoveCommandType.NORMAL)
        {
            _platoon = platoon;
            Destination = destination;
            _heading = heading;
            _moveMode = mode;
        }

        public override Vector3 Destination { get; }

        public override void ProcessWaypoint()
        {
            List<Vector3> destinations = Formations.GetLineFormation(
                Destination, _heading, _platoon.Units.Count);

            for (int i = 0; i < _platoon.Units.Count; i++)
                _platoon.Units[i].SetDestination(destinations[i], _heading, _moveMode);
        }

        public override bool OrderComplete()
        {
            return _platoon.Units.All(x => x.AreOrdersComplete());
        }
    }
}