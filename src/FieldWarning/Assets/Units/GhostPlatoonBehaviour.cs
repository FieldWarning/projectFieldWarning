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
using System.Collections.Generic;
using Mirror;

using PFW.Model.Armory;
using PFW.Model.Game;
using PFW.UI.Ingame;
using PFW.Units.Component.Movement;

namespace PFW.Units
{

    /**
     * When showing previews of move orders or purchases, we need 'ghost' units that
     * are grayed out and not functional. This is where the GhostPlatoon comes in.
     */
    public class GhostPlatoonBehaviour : NetworkBehaviour
    {
        public float FinalHeading;

        [SerializeField]
        private IconBehaviour _icon = null;
        private Unit _unit;
        private PlayerData _owner;
        private List<GameObject> _units = new List<GameObject>();

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            bool canSend = false;
            if (initialState)
            {
                writer.WriteByte(_owner.Id);
                writer.WriteByte(_unit.CategoryId);
                writer.WriteInt32(_unit.Id);
                writer.WriteSingle(FinalHeading);
                canSend = true;
            }
            else 
            {
                // TODO
            }

            return canSend;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                int playerId = reader.ReadByte();
                if (MatchSession.Current.Players.Count > playerId)
                {
                    PlayerData owner = MatchSession.Current.Players[playerId];
                    byte unitCategoryId = reader.ReadByte();
                    int unitId = reader.ReadInt32();
                    if (unitCategoryId < owner.Deck.Categories.Length
                        && unitId < owner.Deck.Categories[unitCategoryId].Count)
                    {
                        Unit unit = owner.Deck.Categories[unitCategoryId][unitId];
                        Initialize(unit, owner);

                        FinalHeading = reader.ReadSingle();
                    }
                    else 
                    {
                        Debug.LogError("Got bad unit id from the server.");
                    }
                }
                else
                {
                    // Got an invalid player id, server is trying to crash us?
                    Debug.LogError(
                        "Network tried to create a ghostplatoon with an invalid player id.");
                }
            }
            else
            {
                // TODO
            }
        }

        public override void OnStartClient()
        {
            Logger.LogNetworking(
                $"Spawned a ghost platoon of {_unit.Name} with netId {netId}", this);
            transform.position = 100 * Vector3.down;

            InitializeIcon(_icon);
        }

        // Call after creating an object of this class, pretend it is a constructor
        public void Initialize(Unit unit, PlayerData owner)
        {
            _owner = owner;
            _unit = unit;
            transform.position = 100 * Vector3.down;

            InitializeIcon(_icon);
        }

        // Each GhostPlatoon gameobject has an icon under it, spawned in the prefab.
        private void InitializeIcon(IconBehaviour icon)
        {
            _icon.BaseColor = _owner.Team.Color;
            _icon.SetGhost();
        }

        public void AddSingleUnit()
        {
            GameObject _unitPrefab = _unit.Prefab;
            GameObject unit = MatchSession.Current.Factory.MakeGhostUnit(
                    gameObject, _unitPrefab);
            unit.GetComponent<MovementComponent>().InitializeGhost(
                    MatchSession.Current.TerrainMap);
            _units.Add(unit);
        }

        // TODO remove. Currently this is needed because spawned platoons 
        // infer their destination from the positions of the ghosts, which are not
        // synchronized. Once those are synchronized (so allies can see buy orders),
        // this will no longer need to be called as an RPC.
        [ClientRpc]
        public void RpcSetOrientation(Vector3 center, float heading)
        {
            SetPositionAndOrientation(center, heading);
        }

        public void SetPositionAndOrientation(Vector3 center, float heading)
        {
            FinalHeading = heading;
            transform.position = center;

            var positions = Formations.GetLineFormation(center, heading, _units.Count);
            for (int i = 0; i < _units.Count; i++) {
                _units[i].GetComponent<MovementComponent>()
                        .Teleport(positions[i], Mathf.PI / 2 - heading);
            }
        }

        public void SetVisible(bool vis)
        {
            _icon.SetVisible(vis);
            _units.ForEach(x => x.GetComponent<MovementComponent>().SetVisible(vis));

            // FIXME: It looks like UnitLabelAttacher looks for a GameObject ("UIWrapper") that
            //      no longer exists in the scene. Is this deprecated? Should it be removed?
            // _units.ForEach(x => x.GetComponent<UnitLabelAttacher>().SetVisibility(vis));
        }

        public void Destroy()
        {
            foreach (var u in _units)
                Destroy(u);

            Destroy(gameObject);
        }

        public void RemoveOneGhostUnit()
        {
            GameObject u = _units[0];
            _units.Remove(u);
            Destroy(u);
            if (_units.Count == 0)
                Destroy();
        }
    }
}
