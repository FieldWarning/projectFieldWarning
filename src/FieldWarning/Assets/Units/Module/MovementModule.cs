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
using System.Linq;

using PFW.Units.Component.Movement;
using PFW.Units;

public sealed class MovementModule : Matchable<Vector3>
{
    public readonly PlatoonBehaviour Platoon;

    public MovementModule(PlatoonBehaviour p)
    {
        Platoon = p;
    }

    // Set the destination of the platoon, overwriting any previous move target.
    public void SetDestination(
            Vector3 destination, 
            float heading = MovementComponent.NO_HEADING, 
            MoveMode mode = MoveMode.NORMAL_MOVE)
    {
        MoveWaypoint waypoint = new MoveWaypoint(Platoon, destination, heading, mode);
        Platoon.Waypoints.Clear();
        Platoon.ActiveWaypoint = null;
        Platoon.Waypoints.Enqueue(waypoint);
    }

    // Add a destination for the platoon, appending to any existing move orders.
    public void AddDestination(
            Vector3 destination, 
            float heading = MovementComponent.NO_HEADING, 
            MoveMode mode = MoveMode.NORMAL_MOVE)
    {
        MoveWaypoint waypoint = new MoveWaypoint(Platoon, destination, heading, mode);
        Platoon.Waypoints.Enqueue(waypoint);
    }

    private Vector3 GetFunctionalPosition()
    {
        var moveWaypoint = Platoon.Waypoints.Where(x => x is MoveWaypoint);
        if (moveWaypoint.Count() > 0)
        {
            return (moveWaypoint.Last() as MoveWaypoint).Destination;
        }
        else
        {
            return Platoon.transform.position;
        }
    }

    public void SetMatch(Vector3 match)
    {
        Platoon.GhostPlatoon.SetVisible(false);
        SetDestination(match, (match - GetFunctionalPosition()).getRadianAngle());
    }

    public float GetScore(Vector3 matchees)
    {
        Vector3 pos = GetFunctionalPosition();

        return (matchees - pos).magnitude;
    }
}
