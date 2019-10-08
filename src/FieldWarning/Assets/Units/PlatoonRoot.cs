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

using PFW.Model.Armory;
using PFW.Model.Game;

namespace PFW.Units
{
    /**
     * Each platoon has a GhostPlatoonBehaviour and a PlatoonBehaviour. This class
     * manages the pairing and lifetime of those two classes.
     */
    public sealed class PlatoonRoot : MonoBehaviour
    {
        [SerializeField]
        private GhostPlatoonBehaviour _ghostPlatoon = null;
        [SerializeField]
        private PlatoonBehaviour _realPlatoon = null;

        // Create a pair of platoon and ghost platoon with units, but don't
        // activate any real units yet (only ghost mode).
        public static PlatoonRoot CreateGhostMode(Unit unit, PlayerData owner)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("PlatoonRoot"));
            PlatoonRoot root = go.GetComponent<PlatoonRoot>();

            root._ghostPlatoon.Initialize(unit, owner);
            root._realPlatoon.Initialize(unit, owner);

            Mirror.NetworkServer.Spawn(go);  /* must call from authority */

            return root;
        }

        // For a ghost mode platoon root, activate the real units also.
        // Effectively spawns the platoon, turning it from a preview into a real one.
        public void Spawn(Vector3 pos)
        {
            _realPlatoon.gameObject.SetActive(true);
            _realPlatoon.GhostPlatoon = _ghostPlatoon;
            _realPlatoon.Spawn(pos);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        // Meant for a platoon still in ghost mode: Spawn() should be called to activate the units
        public void AddSingleUnit()
        {
            _ghostPlatoon.AddSingleUnit();
            _realPlatoon.AddSingleUnit();
        }

        // Meant to put already existing units into the platoon (when merging or splitting platoons)
        private void AddSingleExistingUnit(UnitDispatcher realUnit)
        {
            _ghostPlatoon.AddSingleUnit();
            realUnit.GameObject.transform.parent = _realPlatoon.transform;
            _realPlatoon.Units.Add(realUnit);
        }

        // Makes platoons of N units into N platoons of 1 unit
        public void Split()
        {
            while (_realPlatoon.Units.Count > 1) {
                UnitDispatcher u = _realPlatoon.Units[0];
                _realPlatoon.Units.RemoveAt(0);
                _ghostPlatoon.RemoveOneGhostUnit();

                PlatoonRoot newPlatoon = CreateGhostMode(_realPlatoon.Unit, _realPlatoon.Owner);
                newPlatoon.AddSingleExistingUnit(u);
                // We aren't really spawning the units but binding them to the platoon and activating it:
                newPlatoon.Spawn(u.Transform.position);
            }
        }

        public void SetGhostOrientation(Vector3 center, float heading) =>
                _ghostPlatoon.SetOrientation(center, heading);
    }
}
