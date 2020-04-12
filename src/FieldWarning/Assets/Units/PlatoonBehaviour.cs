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
using PFW.UI.Ingame;
using PFW.Units.Component.Movement;
using PFW.Model.Game;
using PFW.Model.Armory;
using PFW.UI.Ingame.UnitLabel;
using PFW.Units.Component.OrderQueue;
using PFW.Networking;

namespace PFW.Units
{
    public partial class PlatoonBehaviour : NetworkBehaviour
    {
        public Unit Unit;
        [SerializeField]
        private PlatoonLabel _platoonLabel = null;
        public RectTransform SelectableRect;
        public GhostPlatoonBehaviour GhostPlatoon;
        public List<UnitDispatcher> Units = new List<UnitDispatcher>();
        public bool IsInitialized = false;

        public static readonly float UNIT_DISTANCE = 40 * TerrainConstants.MAP_SCALE;

        public PlayerData Owner { get; private set; }

        private WaypointOverlayBehavior _waypointOverlay;

        public OrderQueue OrderQueue { get; } = new OrderQueue();

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            bool canSend = false;
            if (initialState)
            {
                writer.WriteByte(Owner.Id);
                writer.WriteByte(Unit.CategoryId);
                writer.WriteInt32(Unit.Id);
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
                        "Network tried to create a platoon with an invalid player id.");
                }
            }
            else
            {
                // TODO
            }
        }

        private void Update()
        {
            Vector3 pos = new Vector3();

            Units.ForEach(x => pos += x.Transform.position);
            transform.position = pos / Units.Count;

            OrderQueue.HandleUpdate();
        }

        #region Lifetime logic + platoon splitting

        /// <summary>
        ///     Create a pair of platoon and ghost platoon with units, but don't
        ///     activate any real units yet (only ghost mode).
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static PlatoonBehaviour CreateGhostMode(Unit unit, PlayerData owner)
        {
            PlatoonBehaviour realPlatoon= Instantiate(
                    Resources.Load<GameObject>(
                            "Platoon")).GetComponent<PlatoonBehaviour>();
            realPlatoon.GhostPlatoon = Instantiate(
                    Resources.Load<GameObject>(
                            "GhostPlatoon")).GetComponent<GhostPlatoonBehaviour>();

            realPlatoon.GhostPlatoon.Initialize(unit, owner);
            realPlatoon.Initialize(unit, owner);

            return realPlatoon;
        }

        /// <summary>
        ///     Create the preview on the other clients and immediately activate it (see RpcSpawn)
        /// </summary>
        /// <param name="spawnPos"></param>
        public void Spawn(Vector3 spawnPos)
        {
            CommandConnection.Connection.CmdSpawnPlatoon(
                    Owner.Id,
                    Unit.CategoryId,
                    Unit.Id,
                    Units.Count,
                    spawnPos,
                    GhostPlatoon.transform.position,
                    GhostPlatoon.FinalHeading);

            DestroyPreview();
        }

        /// <summary>
        ///     After all units are spawned by the server, call this to get the
        ///     clients to associate their platoon object to its units and ghost.
        /// </summary>
        [ClientRpc]
        public void RpcEstablishReferences(
                uint ghostPlatoonNetId, uint[] unitNetIds)
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(ghostPlatoonNetId, out identity))
            {
                GhostPlatoon = identity.gameObject.GetComponent<GhostPlatoonBehaviour>();
            }

            // Also find, augment and link to the units:
            foreach (uint unitNetId in unitNetIds)
            {
                if (NetworkIdentity.spawned.TryGetValue(unitNetId, out identity))
                {
                    UnitDispatcher unit = identity.GetComponent<UnitDispatcher>();
                    AddSingleExistingUnit(unit);
                }
            }
        }

        public void SetGhostOrientation(Vector3 center, float heading) =>
                GhostPlatoon.SetPositionAndOrientation(center, heading);

        /// <summary>
        ///     Initialization of units beyond the compiled prefab contents
        ///     can only be done using an RPC like this one.
        /// </summary>
        [ClientRpc]
        public void RpcInitializeUnits()
        {
            foreach (UnitDispatcher unit in Units)
            {
                MatchSession.Current.Factory.MakeUnit(
                    Unit, unit.gameObject, this);
            }
        }

        /// <summary>
        ///     Call after creating an object of this class, 
        ///     pretend this is a constructor.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="owner"></param>
        public void Initialize(Unit unit, PlayerData owner)
        {
            Unit = unit;
            Owner = owner;
            _platoonLabel.InitializeAsReal(unit, Owner.Team.ColorScheme, this);
            _waypointOverlay = OverlayFactory.Instance.CreateWaypointOverlay(this);
            _waypointOverlay.gameObject.transform.parent = gameObject.transform;
        }

        /// <summary>
        ///     Meant for a platoon still in ghost mode: Spawn() should be called 
        ///     to activate the units.
        /// </summary>
        public void AddSingleUnit()
        {
            GhostPlatoon.AddSingleUnit();

            GameObject unit = Instantiate(Unit.Prefab);
            MatchSession.Current.Factory.MakeUnit(
                    Unit, unit, this);

            BoxCollider collider =
                    unit.GetComponentInChildren<BoxCollider>();

            unit.SetActive(false);
            collider.enabled = true;

            Units.Add(unit.GetComponent<UnitDispatcher>());
        }

        /// <summary>
        ///     Meant to put already existing units into the platoon
        ///     (such as when merging or splitting platoons).
        /// </summary>
        /// <param name="realUnit"></param>
        public void AddSingleExistingUnit(UnitDispatcher realUnit)
        {
            GhostPlatoon.AddSingleUnit();
            Units.Add(realUnit);
        }

        /// <summary>
        ///     For a ghost mode platoon root, activate the real units also.
        ///     Effectively spawns the platoon, turning it from a preview into a real one.
        /// </summary>
        /// <param name="spawnPos"></param>
        [ClientRpc]
        public void RpcActivate(Vector3 spawnPos)
        {
            gameObject.SetActive(true);
            Activate(spawnPos);
        }

        /// <summary>
        ///     Activates all units, moving from ghost/preview mode to a real platoon.
        /// </summary>
        /// <param name="spawnCenter"></param>
        private void Activate(Vector3 spawnCenter)
        {
            Units.ForEach(x =>
            {
                x.GameObject.SetActive(true);
                //Networking.CommandConnection.Connection.CmdSpawnObject(x.GameObject);
            });

            _platoonLabel.AssociateToRealUnits(Units);

            IsInitialized = true;

            transform.position = spawnCenter;

            List<Vector3> positions = Formations.GetLineFormation(
                spawnCenter, GhostPlatoon.FinalHeading, Units.Count);
            Units.ForEach(u => u.WakeUp());
            for (int i = 0; i < Units.Count; i++)
                Units[i].Teleport(
                    positions[i], GhostPlatoon.FinalHeading - Mathf.PI / 2);

            OrderMovement(GhostPlatoon.transform.position, GhostPlatoon.FinalHeading);
            GhostPlatoon.SetVisible(false);

            MatchSession.Current.RegisterPlatoonBirth(this);
        }

        /// <summary>
        ///     Destroy just the platoon object, without touching its units.
        /// </summary>
        private void DestroyWithoutUnits()
        {
            MatchSession.Current.RegisterPlatoonDeath(this);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroy(_waypointOverlay.gameObject);
        }

        /// <summary>
        ///     TODO wont need this once the unit list is a syncvar..
        /// </summary>
        [ClientRpc]
        public void RpcRemoveUnit(uint unitNetId)
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(unitNetId, out identity))
            {
                UnitDispatcher unit = identity.GetComponent<UnitDispatcher>();
                RemoveUnit(unit);
            }
        }

        /// <summary>
        ///     Call this to notify when a unit is destroyed
        ///     or otherwise removed from the platoon.
        /// </summary>
        public void RemoveUnit(UnitDispatcher unit)
        {
            Units.Remove(unit);
            GhostPlatoon.RemoveOneGhostUnit();

            if (Units.Count == 0)
            {
                Destroy(gameObject);
                MatchSession.Current.RegisterPlatoonDeath(this);
            }
        }

        /// <summary>
        ///     Destroy the platoon and all units in it.
        /// </summary>
        public void Destroy()
        {
            foreach (UnitDispatcher u in Units)
                Destroy(u.GameObject);

            DestroyWithoutUnits();
        }

        /// <summary>
        ///     Destroy a platoon that is a buy preview.
        ///     TODO simplify, this method can probably be merged with another.
        /// </summary>
        public void DestroyPreview()
        {
            Destroy();
            GhostPlatoon.Destroy();
        }

        #endregion


        /// <summary>
        ///     Called when a platoon enters or leaves the player's selection.
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="justPreviewing"> 
        ///     true when the unit should be shaded as if selected, but the
        ///     actual selected set has not been changed yet
        /// </param>
        public void SetSelected(bool selected, bool justPreviewing)
        {
            _platoonLabel.SetSelected(selected);
            Units.ForEach(unit => unit.SetSelected(selected, justPreviewing));

            _waypointOverlay.gameObject.SetActive(selected);
        }

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
            _platoonLabel.SetVisible(enabled);
            _waypointOverlay.gameObject.SetActive(enabled);
        }

        public void SendFirePosOrder(Vector3 position, bool enqueue = false)
        {
            OrderQueue.SendOrder(OrderData.MakeFirePositionOrder(this, position), enqueue);
        }

        #region Movement

        [ClientRpc]
        public void RpcOrderMovement(
            Vector3 destination,
            float heading,
            MoveCommandType mode,
            bool enqueue)
        {
            OrderMovement(destination, heading, mode, enqueue);
        }

        /// <summary>
        /// Give the platoon a movement order.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="heading">
        /// What direction should the units face when they arrive?
        /// </param>
        /// <param name="mode"></param>
        /// <param name="enqueue"></param>
        public void OrderMovement(
            Vector3 destination,
            float heading = MovementComponent.NO_HEADING,
            MoveCommandType mode = MoveCommandType.NORMAL,
            bool enqueue = false)
        {
            OrderData order = OrderData.MakeMoveOrder(this, destination, heading, mode);
            OrderQueue.SendOrder(order, enqueue);
        }

        #endregion

        #region PlayVoicelines

        // For the time being, always play the voiceline of the first unit
        // Until we agree on a default unit in platoon that plays
        public void PlaySelectionVoiceline()
        {
            Units[0].PlaySelectionVoiceline();
        }

        public void PlayMoveCommandVoiceline()
        {
            Units[0].PlayMoveCommandVoiceline();
        }

        public void PlayAttackCommandVoiceline()
        {
            Units[0].PlayAttackCommandVoiceline();
        }

        #endregion
    }
}
