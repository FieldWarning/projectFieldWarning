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

public abstract class PlatoonModule
{
    private Waypoint _newWaypoint;
    protected bool _isQueueing = false;

    public PlatoonBehaviour Platoon;

    public virtual Waypoint NewWaypoint
    {
        get
        {
            if (_newWaypoint == null)
            {
                _newWaypoint = GetModuleWaypoint();
            }
            return _newWaypoint;
        }
        set
        {
            _newWaypoint = value;
        }
    }

    public PlatoonModule(PlatoonBehaviour p)
    {
        Platoon = p;
    }

    public void BeginQueueing(bool isQueueing)
    {
        _isQueueing = isQueueing;

        if (!isQueueing)
            Platoon.Waypoints.Clear();
       
        NewWaypoint = GetModuleWaypoint();
    }

    public void EndQueueing()
    {
        if (_isQueueing || (Platoon.ActiveWaypoint != null && !Platoon.ActiveWaypoint.Interrupt()))
        {
            Platoon.Waypoints.Enqueue(NewWaypoint);
        }
        else
        {
            Platoon.ActiveWaypoint = NewWaypoint;
            NewWaypoint.ProcessWaypoint();
        }
    }

    public virtual void Update()
    {
    }

    protected abstract Waypoint GetModuleWaypoint();
}




