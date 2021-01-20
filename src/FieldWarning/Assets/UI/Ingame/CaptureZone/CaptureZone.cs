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

using PFW.Model.Match;

using System.Collections.Generic;
using UnityEngine;

using PFW.Units;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// A capturable area on the ground.
    /// </summary>
    public class CaptureZone : MonoBehaviour
    {
        public Material Red;
        public Material Blue;
        public Material Neutral;
        // How many points per tick this zone gives
        public int Worth = 3;
        public Team.TeamName OwningTeam { get; private set; } = Team.TeamName.UNDEFINED;

        // Command units currently in the zone
        private List<UnitDispatcher> _units = new List<UnitDispatcher>();

        private MeshRenderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Pop any units that have been killed since the last update:
            _units.RemoveAll(x => x == null);

            // Check if Blue Red None or Both occupy the zone
            bool redIncluded = false;
            bool blueIncluded = false;
            for (int i = 0; i < _units.Count; i++)
            {
                UnitDispatcher unit = _units.ToArray()[i];

                if (!unit.IsMoving())
                {
                    if (unit.Platoon.Team.Name == Team.TeamName.USSR) 
                    {
                        redIncluded = true;
                    }
                    else
                    {
                        blueIncluded = true;
                    }
                }
            }

            if (redIncluded && blueIncluded || (!redIncluded && !blueIncluded))
            {
                if (OwningTeam != Team.TeamName.UNDEFINED) 
                {
                    ChangeTeam(Team.TeamName.UNDEFINED);
                }
            } 
            else if (redIncluded)
            {
                if (OwningTeam != Team.TeamName.USSR)
                {
                    ChangeTeam(Team.TeamName.USSR);
                }
            }
            else
            {
                if (OwningTeam != Team.TeamName.NATO)
                {
                    ChangeTeam(Team.TeamName.NATO);
                }
            }
        }

        // Needs to play sound
        private void ChangeTeam(Team.TeamName newTeam)
        {
            OwningTeam = newTeam;
            switch (OwningTeam)
            {
                case Team.TeamName.USSR:
                    _renderer.material = Red;
                    break;
                case Team.TeamName.NATO:
                    _renderer.material = Blue;
                    break;
                case Team.TeamName.UNDEFINED:
                    _renderer.material = Neutral;
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.parent == null)
                return;

            UnitDispatcher unit =
                    other.transform.root.GetComponent<UnitDispatcher>();
            if (unit != null && unit.isActiveAndEnabled && unit.CanCaptureZones)
            {
                Logger.LogWithoutSubsystem(
                        LogLevel.DEBUG, 
                        $"Command unit {unit} has entered capture zone {this}.");
                _units.Add(unit);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            UnitDispatcher unit =
                    other.transform.root.GetComponent<UnitDispatcher>();

            if (unit != null && unit.isActiveAndEnabled && unit.CanCaptureZones)
            {
                Logger.LogWithoutSubsystem(
                        LogLevel.DEBUG,
                        $"Command unit {unit} has left capture zone {this}.");
                _units.Remove(unit);
            }
        }
    }
}
