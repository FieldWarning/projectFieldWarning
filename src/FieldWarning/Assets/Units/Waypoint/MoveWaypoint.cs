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
using UnityEngine;

using PFW.Units;

public class MoveWaypoint : Waypoint
{
    public Vector3 Destination;
    public float Heading;

    public enum MoveMode { normalMove, reverseMove, fastMove };

    // TODO: make this immutable again, would require untangling the inheritance
    public MoveMode moveMode { get; set; } = MoveMode.normalMove;

    public MoveWaypoint(PlatoonBehaviour p) : base(p) { }

    public override void ProcessWaypoint()
    {
        var destinations = Formations.GetLineFormation(Destination, Heading, platoon.Units.Count);
        for (int i = 0; i < platoon.Units.Count; i++) {
            platoon.Units[i].SetDestination(destinations[i], Heading);
        }
        //platoon.Units.ConvertAll(x => x as Matchable<Vector3>).Match(destinations);
        //platoon.Units.ForEach(x => x.SetUnitFinalHeading(Heading));
    }

    public override bool OrderComplete()
    {
        return platoon.Units.All(x => x.AreOrdersComplete());
    }

    public override bool Interrupt()
    {
        //platoon.units.ForEach (x => x.gotDestination = false);
        return true;
    }
}
