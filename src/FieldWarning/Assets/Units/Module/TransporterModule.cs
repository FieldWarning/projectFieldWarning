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

using PFW.Units;

public class TransporterModule : PlatoonModule, Matchable<TransportableModule>
{
    public PlatoonBehaviour transported;

    public TransporterWaypoint Waypoint
    {
        get
        {
            return base.NewWaypoint as TransporterWaypoint;
        }
    }

    public TransporterModule(PlatoonBehaviour p) : base(p)
    {
    }

    public void Load()
    {
        Waypoint.loading = true;
    }

    public void Unload()
    {
        Waypoint.loading = false;
    }

    public void SetTransported(PlatoonBehaviour p)
    {
        transported = p;
        for (int i = 0; i < Platoon.Units.Count; i++)
        {

            if (p != null)
            {
                if (i == p.Units.Count)
                    break;
                Platoon.Units[i].GetComponent<TransporterBehaviour>().transported = p.Units[i].AsInfantry();
            }
            else
            {
                Platoon.Units[i].GetComponent<TransporterBehaviour>().transported = null;
            }
        }
    }

    protected override Waypoint GetModuleWaypoint()
    {
        return new TransporterWaypoint(Platoon, this);
    }

    public void SetMatch(TransportableModule match)
    {
        Waypoint.loading = true;
        match.BeginQueueing(_isQueueing);
        match.SetTransport(this);
        Waypoint.transportableWaypoint = match.Waypoint;
        match.EndQueueing();
    }

    public float GetScore(TransportableModule matchees)
    {
        return Platoon.Movement.GetScore(matchees.Platoon.transform.position);
    }
}


