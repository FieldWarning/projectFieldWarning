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
using PFW.Model.Match;
using PFW.UI.Ingame;
using PFW.Units.Component.Movement;
using PFW.Networking;
using PFW.Model;

namespace PFW.Units
{

    /**
     * When showing previews of move orders or purchases, we need 'ghost' units that
     * are grayed out and not functional. This is where the GhostPlatoon comes in.
     */
    public class GhostPlatoonBehaviour : NetworkBehaviour
    {
        private const ulong UNITS_DIRTY_BIT   = 0b01;
        private const ulong HEADING_DIRTY_BIT = 0b10;

        public float FinalHeading;

        [SerializeField]
        private PlatoonLabel _platoonLabel = null;
        [SerializeField]
        private NetworkTeamVisibility _visibility = null;
        private Unit _unit;
        private PlayerData _owner;
        private readonly List<GameObject> _units = new List<GameObject>();
        public int UnitCount => _units.Count;

        /// <summary>
        /// Create a ghost platoon without a corresponding real platoon, to be used
        /// for buy previews.
        /// </summary>
        public static GhostPlatoonBehaviour CreatePreviewMode(
            Unit unit, PlayerData owner, int unitCount)
        {
            GhostPlatoonBehaviour ghost = Instantiate(
                      Resources.Load<GameObject>(
                              "GhostPlatoon")).GetComponent<GhostPlatoonBehaviour>();
            ghost.Initialize(unit, owner);
            while (unitCount > 0)
            {
                ghost.AddSingleUnit();
                unitCount--;
            }
            return ghost;
        }
        public static GhostPlatoonBehaviour CreatePreviewMode(Unit unit, PlayerData owner)
            => CreatePreviewMode(unit, owner, Constants.MIN_PLATOON_SIZE);

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                writer.WriteByte(_owner.Id);
                writer.WriteByte(_unit.CategoryId);
                writer.WriteInt32(_unit.Id);
                writer.WriteSingle(FinalHeading);
            }
            else
            {
                writer.WriteSingle(FinalHeading);
                writer.WriteByte((byte)_units.Count);
            }

            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                int playerId = reader.ReadByte();

                if (MatchSession.Current.Players.Count > playerId)
                {
                    byte unitCategoryId = reader.ReadByte();
                    int unitId = reader.ReadInt32();

                    PlayerData owner = MatchSession.Current.Players[playerId];
                    if (unitCategoryId < GameSession.Singleton.Armory.Categories.Length
                        && unitId < GameSession.Singleton.Armory.Categories[unitCategoryId].Count)
                    {
                        FinalHeading = reader.ReadSingle();

                        Unit unit = GameSession.Singleton.Armory.Categories[unitCategoryId][unitId];
                        Initialize(unit, owner);
                        UpdateGhostLocations();
                    }
                    else
                    {
                        if (unitCategoryId < GameSession.Singleton.Armory.Categories.Length)
                        {
                            Logger.LogNetworking(LogLevel.ERROR,
                                $"Got bad unit id = {unitId} from " +
                                $"the server. Total units = {GameSession.Singleton.Armory.Categories[unitCategoryId].Count} " +
                                $"(category = {unitCategoryId}).");
                        }
                        else
                        {
                            Logger.LogNetworking(LogLevel.ERROR,
                                $"Got bad category id = {unitCategoryId} from " +
                                $"the server. Total categories = {GameSession.Singleton.Armory.Categories.Length}");
                        }
                    }
                }
                else
                {
                    // Got an invalid player id, server is trying to crash us?
                    Logger.LogNetworking(LogLevel.ERROR,
                        $"Network tried to create a ghostplatoon with an invalid player id {playerId}.");
                }
            }
            else
            {
                float heading = reader.ReadSingle();
                if (heading != FinalHeading)
                {
                    FinalHeading = heading;
                    UpdateGhostLocations();
                }

                int unitCount = reader.ReadByte();
                while (unitCount != _units.Count && unitCount != 0)
                {
                    if (unitCount > _units.Count)
                    {
                        AddSingleUnit();
                    }
                    else if (unitCount < _units.Count)
                    {
                        RemoveOneGhostUnit();
                    }
                }
            }
        }

        /// <summary>
        ///     Call after creating an object of this class, 
        ///     pretend this is a constructor
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="owner"></param>
        public void Initialize(Unit unit, PlayerData owner)
        {
            _owner = owner;
            _unit = unit;

            _visibility.Initialize(_owner.Team.Name);
            _platoonLabel.InitializeAsGhost(_unit, _owner.Team.ColorScheme);
        }

        public void AddSingleUnit()
        {
            GameObject unit = Instantiate(_unit.Prefab);
            MatchSession.Current.Factory.MakeGhostUnit(_unit, unit);
            unit.GetComponent<MovementComponent>().InitializeGhost(
                    MatchSession.Current.TerrainMap);
            _units.Add(unit);
            if (_platoonLabel.Visible == true)
            {
                UpdateGhostLocations();
            }
            SetDirtyBit(UNITS_DIRTY_BIT);
        }

        public void SetPositionAndOrientation(Vector3 center, float heading)
        {
            FinalHeading = heading;
            SetDirtyBit(HEADING_DIRTY_BIT);
            transform.position = center;

            UpdateGhostLocations();
        }

        private void UpdateGhostLocations()
        {
            List<Vector3> positions = Formations.GetLineFormation(
                    transform.position, FinalHeading, _units.Count);
            for (int i = 0; i < _units.Count; i++)
            {
                _units[i].GetComponent<MovementComponent>()
                        .Teleport(positions[i], Mathf.PI / 2 - FinalHeading);
            }
        }

        public void SetVisible(bool vis)
        {
            _platoonLabel.Visible = vis;
            _units.ForEach(unit =>
            {
                foreach (Renderer renderer in unit.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = vis;
                }
            });
        }

        public void Destroy()
        {
            foreach (GameObject u in _units)
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
            else
                SetDirtyBit(UNITS_DIRTY_BIT);
        }

        /// <summary>
        ///     Spawn a platoon from this buy preview.
        /// </summary>
        /// <param name="spawnPos"></param>
        public void Spawn(Vector3 spawnPos)
        {
            CommandConnection.Connection.CmdSpawnPlatoon(
                    _owner.Id,
                    _unit.CategoryId,
                    _unit.Id,
                    _units.Count,
                    spawnPos,
                    transform.position,
                    FinalHeading);

            Destroy();
        }
    }
}
