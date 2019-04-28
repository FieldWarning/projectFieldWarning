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

using PFW.Model.Game;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PFW.Units.Component.Movement;

namespace PFW.UI.Ingame
{
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

        // Vehicles currently in the zone
        // (Maybe exclude all non-commander vehicles)
        private List<VehicleMovementComponent> _vehicles = new List<VehicleMovementComponent>();

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            // Check if Blue Red None or Both occupy the zone
            bool redIncluded = false;
            bool blueIncluded = false;
            PlayerData newOwner = null;
            for (int i = 0; i < _vehicles.Count; i++) {
                VehicleMovementComponent vehicle = _vehicles.ToArray()[i];
                if (vehicle.AreOrdersComplete()) {
                    newOwner = vehicle.Platoon.Owner;
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
                    changeOwner(null);
                }
            } else if (redIncluded) {
                if (_owner != newOwner) {
                    changeOwner(newOwner);
                }
            } else {
                if (_owner != newOwner) {
                    changeOwner(newOwner);
                }
            }
        }

        // Needs to play sound
        private void changeOwner(PlayerData newOwner)
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
            if (other.transform.parent != null && other.transform.parent.GetComponent<VehicleMovementComponent>() != null && other.transform.parent.GetComponent<VehicleMovementComponent>().isActiveAndEnabled) {
                _vehicles.Add(other.transform.parent.GetComponent<VehicleMovementComponent>());
            }
        }

        // TODO: If the unit is killed, it will never be removed from the zone:
        private void OnTriggerExit(Collider other)
        {
            if (other.transform.parent.GetComponent<VehicleMovementComponent>() != null) {
                _vehicles.Remove(other.transform.parent.GetComponent<VehicleMovementComponent>());
            }
        }
    }
}
