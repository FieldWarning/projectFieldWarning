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

public class MovementModule : PlatoonModule, Matchable<Vector3>
{
    public Vector3 FinalHeading;

    public MoveWaypoint Waypoint
    {
        get
        {
            return base.NewWaypoint as MoveWaypoint;
        }
    }

    public MovementModule(PlatoonBehaviour p)
        : base(p)
    {
    }

    public void SetDestination(Vector3 v)
    {
        var finalHeading = v - GetFunctionalPosition();
        SetFinalOrientation(v, finalHeading.getRadianAngle());
        //SetFinalOrientation(v, UnitBehaviour.NO_HEADING);
    }

    public void GetDestinationFromGhost()
    {
        var heading = Platoon.GhostPlatoon.GetComponent<GhostPlatoonBehaviour>().FinalHeading;
        SetFinalOrientation(Platoon.GhostPlatoon.transform.position, heading);
    }

    public void GetHeadingFromGhost()
    {
        var heading = Platoon.GhostPlatoon.GetComponent<GhostPlatoonBehaviour>().FinalHeading;
        SetFinalOrientation(Waypoint.Destination, heading);
    }

    public void UseDefaultHeading()
    {
        SetFinalOrientation(Waypoint.Destination, UnitBehaviour.NO_HEADING);
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

    public void SetFinalOrientation(Vector3 v, float h)
    {
        Waypoint.Destination = v;
        Waypoint.Heading = h;
    }

    public void SetMatch(Vector3 match)
    {
        Platoon.GhostPlatoon.SetVisible(false);
        SetDestination(match);
    }

    public float GetScore(Vector3 matchees)
    {
        Vector3 pos = GetFunctionalPosition();

        return (matchees - pos).magnitude;
    }

    protected override Waypoint GetModuleWaypoint()
    {
        return new MoveWaypoint(Platoon);
    }
}
