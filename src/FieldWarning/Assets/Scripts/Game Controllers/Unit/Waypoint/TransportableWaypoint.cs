﻿/**
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

public class TransportableWaypoint : Waypoint
{
    public TransporterWaypoint transporterWaypoint;

    public TransportableWaypoint(PlatoonBehaviour p)
        : base(p)
    {

    }

    public override void ProcessWaypoint()
    {
        for (int i = 0; i < platoon.Units.Count; i++) {
            platoon.Units[i].AsInfantry().setTransportTarget(transporterWaypoint.platoon.Units[i].GetComponent<TransporterBehaviour>());
        }
    }

    public override bool OrderComplete()
    {
        if (transporterWaypoint.interrupted) {
            platoon.Units.ForEach(x => x.AsInfantry().setTransportTarget(null));
            return true;
        } else {
            if (!platoon.Units.Any(x => x.AsInfantry().interactsWithTransport(false))) {

                return true;
            } else {
                return false;
            }
        }
        //return transporterWaypoint.interrupted || platoon.units.All(x => !(x as InfantryBehaviour).interactsWithTransport(true));
    }

    public override bool Interrupt()
    {
        if (!platoon.Units.Any(x => x.AsInfantry().interactsWithTransport(true)))
            interrupted = true;

        return interrupted;
    }
}
