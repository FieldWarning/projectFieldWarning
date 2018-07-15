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

public class TransporterWaypoint : Waypoint {
    public bool loading;
    PlatoonBehaviour.TransporterModule module;
    public TransportableWaypoint transportableWaypoint;
    //public PlatoonBehaviour target;
    public TransporterWaypoint(PlatoonBehaviour p, PlatoonBehaviour.TransporterModule m) : base(p)
    {
        module = m;
    }

    public override void processWaypoint()
    {
        if (loading) {
            if (transportableWaypoint == null)
                return;
            for (int i = 0; i < transportableWaypoint.platoon.units.Count; i++) {
                platoon.units[i].GetComponent<TransporterBehaviour>().load(transportableWaypoint.platoon.units[i] as InfantryBehaviour);
            }
        } else {
            if (module.transported == null)
                return;
            module.transported.setEnabled(true);
            module.transported = null;
            platoon.units.ForEach(x => x.GetComponent<TransporterBehaviour>().unload());
        }
    }

    public override bool orderComplete()
    {
        if (transportableWaypoint != null && transportableWaypoint.interrupted) {
            platoon.units.ForEach(x => x.GetComponent<TransporterBehaviour>().target = null);
            return true;
        }
        if (loading) {
            if (transportableWaypoint.orderComplete()) {
                module.setTransported(transportableWaypoint.platoon);
                platoon.units.ForEach(x => x.GetComponent<TransporterBehaviour>().target = null);
                transportableWaypoint.platoon.setEnabled(false);
                return true;
            } else {
                return false;
            }
            //platoon.units.All(x => x.GetComponent<TransporterBehaviour>().loadingComplete());//premature true
        } else {
            if (platoon.units.All(x => x.GetComponent<TransporterBehaviour>().unloadingComplete())) {
                module.setTransported(null);
                return true;
            } else {
                return false;
            }
        }
    }

    public override bool interrupt()
    {
        if (transportableWaypoint != null && transportableWaypoint.interrupt()) {
            platoon.units.ForEach(x => x.GetComponent<TransporterBehaviour>().target = null);
            Debug.Log("transport interupted");
            interrupted = true;
            return true;
        } else {
            return false;
        }

    }
}