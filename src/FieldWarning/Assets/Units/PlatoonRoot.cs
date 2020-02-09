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


using System.Collections.Generic;
using UnityEngine;
using Mirror;

using PFW.Model.Armory;
using PFW.Model.Game;
using PFW.Networking;

namespace PFW.Units
{
    /**
     * Each platoon has a GhostPlatoonBehaviour and a PlatoonBehaviour. This class
     * manages the pairing and lifetime of those two classes.
     */
    public sealed class PlatoonRoot : NetworkBehaviour
    {
        [SerializeField]
        private GhostPlatoonBehaviour _ghostPlatoon = null;
        [SerializeField]
        private PlatoonBehaviour _realPlatoon = null;

        /// <summary>
        ///     After all units are spawned by the server, call this to get the
        ///     clients to associate their platoon object to its units and ghost.
        /// </summary>
        [ClientRpc]
        public void RpcEstablishReferences(
                uint realPlatoonNetId, uint ghostPlatoonNetId, uint[] unitNetIds) 
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(ghostPlatoonNetId, out identity))
            {
                _ghostPlatoon = identity.gameObject.GetComponent<GhostPlatoonBehaviour>();
            }
            if (NetworkIdentity.spawned.TryGetValue(realPlatoonNetId, out identity))
            {
                _realPlatoon = identity.gameObject.GetComponent<PlatoonBehaviour>();
                _realPlatoon.GhostPlatoon = _ghostPlatoon;
            }

            // Also find, augment and link to the units:
            foreach (uint unitNetId in unitNetIds) 
            {
                if (NetworkIdentity.spawned.TryGetValue(unitNetId, out identity)) 
                {
                    UnitDispatcher unit = identity.GetComponent<UnitDispatcher>();
                    MatchSession.Current.Factory.MakeUnit(
                        _realPlatoon.Unit, unit.gameObject, _realPlatoon);
                    AddSingleExistingUnit(unit);
                }
            }
        }

        /// <summary>
        ///     Create a pair of platoon and ghost platoon with units, but don't
        ///     activate any real units yet (only ghost mode).
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static PlatoonRoot CreateGhostMode(Unit unit, PlayerData owner)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("PlatoonRoot"));
            PlatoonRoot root = go.GetComponent<PlatoonRoot>();

            // Hack: Ideally they should be under this object,
            // but mirror does not support networking nested objects..
            root._ghostPlatoon = Instantiate(
                    Resources.Load<GameObject>(
                            "GhostPlatoon")).GetComponent<GhostPlatoonBehaviour>();
            root._realPlatoon = Instantiate(
                    Resources.Load<GameObject>(
                            "Platoon")).GetComponent<PlatoonBehaviour>();

            root._ghostPlatoon.Initialize(unit, owner);
            root._realPlatoon.Initialize(unit, owner);

            return root;
        }

        /// <summary>
        ///     Create the preview on the other clients and immediately activate it (see RpcSpawn)
        /// </summary>
        /// <param name="spawnPos"></param>
        public void Spawn(Vector3 spawnPos)
        {
            CommandConnection.Connection.CmdSpawnPlatoon(
                    _realPlatoon.Owner.Id,
                    _realPlatoon.Unit.CategoryId,
                    _realPlatoon.Unit.Id,
                    _realPlatoon.Units.Count,
                    spawnPos, 
                    _ghostPlatoon.transform.position,
                    _ghostPlatoon.FinalHeading);

            Destroy();
        }

        /// <summary>
        ///     For a ghost mode platoon root, activate the real units also.
        ///     Effectively spawns the platoon, turning it from a preview into a real one.
        /// </summary>
        /// <param name="spawnPos"></param>
        [ClientRpc]
        public void RpcSpawn(Vector3 spawnPos)
        {
            _realPlatoon.gameObject.SetActive(true);
            _realPlatoon.GhostPlatoon = _ghostPlatoon;
            _realPlatoon.Spawn(spawnPos);
        }

        public void Destroy()
        {
            _ghostPlatoon.Destroy();
            _realPlatoon.Destroy();
            Destroy(gameObject);
        }

        /// <summary>
        ///     Meant for a platoon still in ghost mode: Spawn() should be called 
        ///     to activate the units
        /// </summary>
        public void AddSingleUnit()
        {
            _ghostPlatoon.AddSingleUnit();
            _realPlatoon.AddSingleUnit();
        }

        /// <summary>
        ///     Meant to put already existing units into the platoon
        ///     (such as when merging or splitting platoons).
        /// </summary>
        /// <param name="realUnit"></param>
        private void AddSingleExistingUnit(UnitDispatcher realUnit)
        {
            _ghostPlatoon.AddSingleUnit();
            _realPlatoon.Units.Add(realUnit);
        }

        /// <summary>
        ///     Makes platoons of N units into N platoons of 1 unit
        ///     TODO multiplayer
        /// </summary>
        public void Split()
        {
            while (_realPlatoon.Units.Count > 1) {
                UnitDispatcher u = _realPlatoon.Units[0];
                _realPlatoon.Units.RemoveAt(0);
                _ghostPlatoon.RemoveOneGhostUnit();

                PlatoonRoot newPlatoon = CreateGhostMode(_realPlatoon.Unit, _realPlatoon.Owner);
                newPlatoon.AddSingleExistingUnit(u);
                // We aren't really spawning the units but binding them 
                // to the platoon and activating it:
                newPlatoon.Spawn(u.Transform.position);
            }
        }

        public void SetGhostOrientation(Vector3 center, float heading) =>
                _ghostPlatoon.SetPositionAndOrientation(center, heading);
    }
}
