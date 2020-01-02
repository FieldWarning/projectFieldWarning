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
using UnityEngine;

namespace PFW.Units.Component.Movement
{
    public sealed class MoveWaypoint
    {
        public readonly PlatoonBehaviour Platoon;
        public readonly Vector3 Destination;
        public readonly float Heading;

        public readonly MoveMode MoveMode;

        public MoveWaypoint(
                PlatoonBehaviour platoon,
                Vector3 destination,
                float heading = MovementComponent.NO_HEADING,
                MoveMode mode = MoveMode.NORMAL_MOVE)
        {
            Platoon = platoon;
            Destination = destination;
            Heading = heading;
            MoveMode = mode;
        }

        public void ProcessWaypoint()
        {
            List<Vector3> destinations = Formations.GetLineFormation(
                    Destination, Heading, Platoon.Units.Count);
            for (int i = 0; i < Platoon.Units.Count; i++)
            {
                Platoon.Units[i].SetDestination(destinations[i], Heading);
            }
        }

        public bool OrderComplete()
        {
            return Platoon.Units.All(x => x.AreOrdersComplete());
        }
    }
}