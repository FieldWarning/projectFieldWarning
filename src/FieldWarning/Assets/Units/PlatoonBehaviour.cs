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
using System.Linq;
using Mirror;

using PFW.UI.Ingame;
using PFW.Model.Game;

using PFW.Model.Armory;

namespace PFW.Units
{
    public partial class PlatoonBehaviour : NetworkBehaviour
    {
        public Unit Unit;
        public IconBehaviour Icon;
        public MovementModule Movement;
        public Waypoint ActiveWaypoint;
        public TransporterModule Transporter;
        public TransportableModule Transportable;
        public GhostPlatoonBehaviour GhostPlatoon;
        public Queue<Waypoint> Waypoints = new Queue<Waypoint>();
        public List<UnitDispatcher> Units = new List<UnitDispatcher>();
        private List<PlatoonModule> _modules = new List<PlatoonModule>();
        public bool IsInitialized = false;

        public static readonly float UNIT_DISTANCE = 40 * TerrainConstants.MAP_SCALE;

        public PlayerData Owner { get; private set; }
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            bool canSend = false;
            if (initialState)
            {
                writer.Write(Owner.Id);
                writer.Write(Unit.CategoryId);
                writer.Write(Unit.Id);
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

        public void Update()
        {
            Vector3 pos = new Vector3();

            Units.ForEach(x => pos += x.Transform.position);
            transform.position = pos / Units.Count;
            _modules.ForEach(x => x.Update());

            if (ActiveWaypoint == null || ActiveWaypoint.OrderComplete()) {
                if (Waypoints.Any()) {
                    ActiveWaypoint = Waypoints.Dequeue();
                    ActiveWaypoint.ProcessWaypoint();
                } else {
                    ActiveWaypoint = null;
                    //units.ForEach (x => x.gotDestination = false);
                }
                //setFinalOrientation(waypoint.destination,waypoint.heading);
            }
        }

        public void BuildModules()
        {
            Movement = new MovementModule(this);

            //if (t == UnitType.AFV) {
            //    Transporter = new TransporterModule(this);
            //    _modules.Add(Transporter);
            //}

            //if (t == UnitType.Infantry) {
            //    Transportable = new TransportableModule(this);
            //    _modules.Add(Transportable);
            //}
        }

        // Call after creating an object of this class, pretend it is a constructor
        public void Initialize(Unit unit, PlayerData owner)
        {
            Unit = unit;
            Owner = owner;
            Icon.BaseColor = Owner.Team.Color;
        }

        // Create an inactive unit (to be activated when Spawn() is called)
        public void AddSingleUnit()
        {
            var unitInstance = MatchSession.Current.Factory.MakeUnit(
                gameObject, Unit.Prefab, Owner.Team.Color);
            //Networking.CommandConnection.Connection.CmdSpawnObject(unitInstance);

            var collider = unitInstance.GetComponentInChildren<BoxCollider>();

            unitInstance.SetActive(false);
            collider.enabled = true;

            Units.Add(new UnitDispatcher(unitInstance, this));
        }

        // Activates all units, moving from ghost/preview mode to a real platoon
        // Only use from PlatoonRoot (the lifetime manager class for platoons)
        public void Spawn(Vector3 center)
        {
            Units.ForEach(x => {
                x.GameObject.SetActive(true);
                //Networking.CommandConnection.Connection.CmdSpawnObject(x.GameObject);
            });

            BuildModules();

            //if (t == UnitType.AFV) {
            //    var ghost = GhostPlatoonBehaviour.Build(UnitType.Infantry, owner, n);
            //    Transporter.SetTransported(ghost.GetRealPlatoon());
            //    ghost.SetOrientation(100 * Vector3.down, 0);
            //    ghost.SetVisible(false);
            //}

            Movement.SetDestination(Vector3.forward);

            Icon.AssociateToRealUnits(Units);

            IsInitialized = true;


            transform.position = center;
            var heading = GhostPlatoon.GetComponent<GhostPlatoonBehaviour>().FinalHeading;

            var positions = Formations.GetLineFormation(center, heading, Units.Count);
            Units.ForEach(u => u.WakeUp());
            for (int i = 0; i < Units.Count; i++)
                Units[i].SetOriginalOrientation(positions[i], heading - Mathf.PI / 2);

            Movement.BeginQueueing(false);
            Movement.GetDestinationFromGhost();
            Movement.EndQueueing();
            GhostPlatoon.SetVisible(false);

            MatchSession.Current.RegisterPlatoonBirth(this);
        }

        // Called when a platoon enters or leaves the player's selection.
        // justPreviewing - true when the unit should be shaded as if selected, but the
        //                  actual selected set has not been changed yet
        public void SetSelected(bool selected, bool justPreviewing)
        {
            Icon?.SetSelected(selected);
            Units.ForEach(unit => unit.SetSelected(selected, justPreviewing));
        }

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
            Icon?.SetVisible(enabled);
        }

        public void SendFirePosOrder(Vector3 position)
        {
            Units.ForEach(u => u.SendFirePosOrder(position));
            PlayAttackCommandVoiceline();
        }

        /// <summary>
        /// Destroy just the platoon object, without touching its units.
        /// </summary>
        private void DestroyWithoutUnits()
        {
            MatchSession.Current.RegisterPlatoonDeath(this);
            Destroy(gameObject);
        }

        /// <summary>
        /// Destroy the platoon and all units in it.
        /// </summary>
        public void Destroy()
        {
            foreach (var p in Units)
                Destroy(p.GameObject);

            DestroyWithoutUnits();
        }

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