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
using PFW.Model.Match;
using PFW.Model.Armory;
using PFW.UI.Ingame.UnitLabel;
using PFW.Units.Component.OrderQueue;
using PFW.Networking;
using UnityEngine.EventSystems;
using System.Linq;
using PFW.Model;

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

        public static readonly float UNIT_DISTANCE = 40 * Constants.MAP_SCALE;

        public PlayerData Owner { get; private set; }
        public Team Team => Owner.Team;

        private WaypointOverlayBehavior _waypointOverlay;

        private OrderQueue _orderQueue { get; } = new OrderQueue();

        private bool _pointerOnLabel = false;
        private GameObject _mainCamera;

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
                    if (unitCategoryId < GameSession.Singleton.Armory.Categories.Length
                        && unitId < GameSession.Singleton.Armory.Categories[unitCategoryId].Count)
                    {
                        Unit unit = GameSession.Singleton.Armory.Categories[unitCategoryId][unitId];
                        Initialize(unit, owner);
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
                // TODO
            }
        }

        private void Update()
        {
            Vector3 pos = new Vector3();

            Units.ForEach(x => pos += x.Transform.position);
            transform.position = pos / Units.Count;

            _orderQueue.HandleUpdate();

            if (_pointerOnLabel)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");

                if (scroll != 0)
                {
                    _mainCamera.GetComponent<OrbitCameraBehaviour>().enabled = true;
                    _mainCamera.GetComponent<SlidingCameraBehaviour>().enabled = false;
                    _mainCamera.GetComponent<OrbitCameraBehaviour>().SetTarget(this.gameObject);
                }
            }
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
        public void ToggleGhostVisibility(bool visible) =>
                GhostPlatoon.SetVisible(visible);

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
            _platoonLabel.InitializeAsReal(unit, Team.ColorScheme, this);
            _waypointOverlay = OverlayFactory.Instance.CreateWaypointOverlay(this);
            _waypointOverlay.gameObject.transform.parent = gameObject.transform;
            _mainCamera = Camera.main.gameObject;
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

            OrderMovement(
                    GhostPlatoon.transform.position, 
                    GhostPlatoon.FinalHeading, 
                    MoveCommandType.FAST);
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

        public void PointerEnterEvent(BaseEventData baseEvent)
        {
            _pointerOnLabel = true;
        }

        public void PointerExitEvent(BaseEventData baseEvent)
        {
            _pointerOnLabel = false;
        }

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
            _platoonLabel.Visible = enabled;
            _waypointOverlay.gameObject.SetActive(enabled);
        }

        public int PlaceTargetingPreview(Vector3 targetPosition, bool respectMaxRange)
        {
            int minRange = 99999;
            foreach (UnitDispatcher unit in Units)
            {
                int range = unit.PlaceTargetingPreview(targetPosition, respectMaxRange);
                if (range < minRange)
                {
                    minRange = range;
                }
            }
            return minRange;
        }

        public void ToggleTargetingPreview(bool enabled)
        {
            Units.ForEach(x => x.ToggleTargetingPreview(enabled));
        }

        public void SendFirePosOrder(Vector3 position, bool enqueue = false)
        {
            _orderQueue.SendOrder(OrderData.MakeFirePositionOrder(this, position), enqueue);
        }

        /// <summary>
        /// Make the platoon (in)visible (assuming the
        /// units are also (in)visible).
        /// </summary>
        /// <param name="visible"></param>
        public void ToggleLabelVisibility(bool visible)
        {
            _platoonLabel.gameObject.SetActive(visible);
        }

        #region Movement

        /// <summary>
        /// Called on clients by the server.
        /// </summary>
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
            _orderQueue.SendOrder(order, enqueue);
        }

        #endregion

        #region CancelOrders

        /// <summary>
        /// Called on clients by the server.
        /// </summary>
        [ClientRpc]
        public void RpcCancelOrders()
        {
            CancelOrders();
        }

        /// <summary>
        /// Tells the platoon that all orders from the player have been cancelled.
        /// </summary>
        public void CancelOrders()
        {
            _orderQueue.Clear();
            Units.ForEach(u => u.CancelOrders());
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


        /// <summary>
        /// Return the points the platoon will pass through if it
        /// executes its order queue. This is useful to show
        /// command previews when the player selects a platoon.
        /// 
        /// TODO: Cache the result instead of recalculating every time
        /// TODO: Return and preview all planned orders, not just movement.
        /// TODO: Show real pathfinder paths
        /// </summary>
        public List<OrderData> CalculateOrderPreview()
        {
            List<OrderData> moveOrders = _orderQueue.Orders
                .Where(o => o.OrderType == OrderType.MOVE_ORDER)
                .ToList();

            return moveOrders;
        }
    }
}
