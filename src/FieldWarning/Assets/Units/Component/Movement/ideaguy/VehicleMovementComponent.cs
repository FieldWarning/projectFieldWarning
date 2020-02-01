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

using System;
using PFW.Units.Component.OrderQueue;

namespace PFW.Units.Component.Movement
{
    public class VehicleMovementComponent2 : IMoveComponent
    {
        public VehicleMovementComponent2()
        {
        }

        // TODO ideaguy zone

        //private IMoveComponent _groundMove;
        //private IMoveComponent _amphibMove;

        //private List<Waypoint> _waypoints;

        //public void applyMovement(Transform transform)
        //{
        //    if (!_waypoints.empty()) {
        //        Waypoint waypoint = _waypoints.first();
        //        waypoint.movementComponent.applyMovement(transform, waypoint.position, Time.deltaTime);

        //        if (waypoint.position == transform.position)
        //            _waypoints.pop();
        //    }
        //}

        public void ApplyMovement(OrderData w, Time deltaTime)
        {
            throw new NotImplementedException();
        }

        public int EstimateTravelTime(Vector3 start, Vector3 dest)
        {
            throw new NotImplementedException();
        }
    }
}
