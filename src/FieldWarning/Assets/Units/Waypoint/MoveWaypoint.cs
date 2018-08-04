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
using System.Collections.Generic;
using UnityEngine;

public class MoveWaypoint : Waypoint {
    public Vector3 destination;
    public float heading;

    public MoveWaypoint(PlatoonBehaviour p) : base(p)
    {
    }

    public override void ProcessWaypoint()
    {
        Vector3 v = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        var left = new Vector3(-v.z, 0, v.x);

        var pos = destination + (platoon.Units.Count - 1) * (PlatoonBehaviour.BaseDistance / 2) * left;
        var destinations = new List<Vector3>();
        for (int i = 0; i < platoon.Units.Count; i++) {
            destinations.Add(pos - PlatoonBehaviour.BaseDistance * i * left);
        }

        platoon.Units.ConvertAll(x => x as Matchable<Vector3>).Match(destinations);
        platoon.Units.ForEach(x => x.SetUnitFinalHeading(heading));
    }

    public override bool orderComplete()
    {
        return platoon.Units.All(x => x.OrdersComplete());
    }

    public override bool interrupt()
    {
        //platoon.units.ForEach (x => x.gotDestination = false);
        return true;
    }
}
