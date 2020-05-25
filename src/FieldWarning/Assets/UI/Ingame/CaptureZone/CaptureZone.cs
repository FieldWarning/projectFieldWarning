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

        // Maybe take out all that owner stuff and simply use
        // an int or otherwise shorten the code
        private PlayerData _owner;

        // Units currently in the zone
        // (Maybe exclude all non-commander units)
        private List<UnitDispatcher> _units =
            new List<UnitDispatcher>();

        // Update is called once per frame
        private void Update()
        {
            // Check if Blue Red None or Both occupy the zone
            bool redIncluded = false;
            bool blueIncluded = false;
            PlayerData newOwner = null;
            for (int i = 0; i < _units.Count; i++) {
                UnitDispatcher unit = _units.ToArray()[i];
                if (unit.AreOrdersComplete()) {
                    newOwner = unit.Platoon.Owner;
                    // Names are USSR and NATO
                    if (newOwner.Team.Name == "USSR") {
                        redIncluded = true;
                    } else {
                        blueIncluded = true;
                    }
                }
            }
            if (redIncluded && blueIncluded || (!redIncluded && !blueIncluded)) {
                if (_owner != null) {
                    ChangeOwner(null);
                }
            } else if (redIncluded) {
                if (_owner != newOwner) {
                    ChangeOwner(newOwner);
                }
            } else {
                if (_owner != newOwner) {
                    ChangeOwner(newOwner);
                }
            }
        }

        // Needs to play sound
        private void ChangeOwner(PlayerData newOwner)
        {
            if (_owner != null) {
                _owner.IncomeTick -= Worth;
            }
            if (newOwner != null) {
                newOwner.IncomeTick += Worth;
            }
            _owner = newOwner;
            if (_owner != null) {
                if (_owner.Team.Name == "USSR") {
                    this.GetComponent<MeshRenderer>().material = Red;
                } else {
                    this.GetComponent<MeshRenderer>().material = Blue;
                }
            } else {
                this.GetComponent<MeshRenderer>().material = Neutral;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.parent == null)
                return;

            UnitDispatcher component =
                    other.transform.parent.GetComponent<UnitDispatcher>();
            if (component != null && component.isActiveAndEnabled)
                _units.Add(component);
        }

        // TODO: If the unit is killed, it will never be removed from the zone:
        private void OnTriggerExit(Collider other)
        {
            UnitDispatcher component =
                    other.transform.parent.GetComponent<UnitDispatcher>();

            if (component != null)
                _units.Remove(component);
        }
    }
}
