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

using Mirror;
using UnityEngine;

using PFW.Model.Armory;
using PFW.Model.Match;
using PFW.UI.Ingame;
using PFW.Units;
using PFW.Units.Component.Movement;
using static PFW.Constants;

namespace PFW.Networking
{
    /**
     * This class exists to allow clients to send commands to the server.
     *
     * Weird name to make it easier to distinguish from the mirror classes.
     */
    public class CommandConnection : NetworkBehaviour
    {
        public ChatManager ChatManager;
        public static CommandConnection Connection;

        private void Start()
        {
            ChatManager = FindObjectOfType<ChatManager>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Connection = this;
        }

        // (On Server) Update chat messages
        [Command]
        public void CmdUpdateChat(string msg)
        {
            ChatManager.UpdateMessageText(msg);
        }

        /// <summary>
        /// From a platoon with N units, make N platoons with 1 unit each.
        /// </summary>
        /// <param name="platoonNetId"></param>
        [Command]
        public void CmdSplitPlatoon(uint platoonNetId)
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(platoonNetId, out identity))
            {
                PlatoonBehaviour platoon = identity.gameObject.GetComponent<PlatoonBehaviour>();

                // We do not do something like 'while (Units.Count > 0)'
                // because the RPCs finish executing and hence update the unit count
                // way after the loop has concluded!
                int newPlatoonsCount = platoon.Units.Count - 1;
                while (newPlatoonsCount > 0)
                {
                    UnitDispatcher u = platoon.Units[newPlatoonsCount];
                    uint unitNetId = u.GetComponent<NetworkIdentity>().netId;
                    platoon.RpcRemoveUnit(unitNetId);

                    PlatoonBehaviour newPlatoon = PlatoonBehaviour.CreateGhostMode(
                            platoon.Unit, platoon.Owner);
                    GhostPlatoonBehaviour ghostPlatoon = newPlatoon.GhostPlatoon;

                    NetworkServer.Spawn(ghostPlatoon.gameObject);
                    NetworkServer.Spawn(newPlatoon.gameObject);

                    newPlatoon.RpcEstablishReferences(
                            ghostPlatoon.netId,
                            new[] { unitNetId });
                    newPlatoon.RpcActivate(u.Transform.position);

                    newPlatoonsCount--;
                }
            }
        }

        [Command]
        public void CmdSpawnPlatoon(
                byte playerId,
                byte categoryId,
                int unitId,
                int unitCount,
                Vector3 spawnPos,
                Vector3 destinationCenter,
                float destinationHeading)
        {
            Logger.LogNetworking(
                    LogLevel.DEBUG,
                    this,
                    $"Spawning platoon at {spawnPos}");
            if (MatchSession.Current.Players.Count > playerId 
                && unitCount >= MIN_PLATOON_SIZE
                && unitCount <= MAX_PLATOON_SIZE)
            {
                PlayerData owner = MatchSession.Current.Players[playerId];
                if (categoryId < owner.Deck.Categories.Length
                    && unitId < MatchSession.Current.Armory.Categories[categoryId].Count)
                {
                    Unit unit = MatchSession.Current.Armory.Categories[categoryId][unitId];

                    PlatoonBehaviour newPlatoon = PlatoonBehaviour.CreateGhostMode(unit, owner);
                    GhostPlatoonBehaviour ghostPlatoon = newPlatoon.GhostPlatoon;

                    NetworkServer.Spawn(ghostPlatoon.gameObject);
                    NetworkServer.Spawn(newPlatoon.gameObject);

                    uint[] unitIds = new uint[unitCount];
                    for (int i = 0; i < unitCount; i++)
                    {
                        GameObject unitGO = Instantiate(unit.Prefab);
                        // Any added unit initialization must be done in an RPC,
                        // otherwise it won't show up on the clients!

                        NetworkServer.Spawn(unitGO);
                        unitIds[i] = unitGO.GetComponent<NetworkIdentity>().netId;
                    }

                    newPlatoon.RpcEstablishReferences(ghostPlatoon.netId, unitIds);
                    newPlatoon.RpcInitializeUnits();

                    ghostPlatoon.RpcSetOrientation(destinationCenter, destinationHeading);

                    newPlatoon.RpcActivate(spawnPos);
                }
                else
                {
                    Debug.LogError("Got bad unit id from a client.");
                }
            }
            else
            {
                // Got an invalid player id, server is trying to crash us?
                Debug.LogError(
                    "Client asked to create a platoon with an invalid player id.");
            }
        }

        /// <summary>
        /// See PlatoonBehaviour::OrderMovement
        /// </summary>
        [Command]
        public void CmdOrderMovement(
                uint platoonNetId,
                Vector3 destination,
                float heading,
                MoveCommandType mode,
                bool enqueue)
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(platoonNetId, out identity))
            {
                PlatoonBehaviour platoon = identity.gameObject.GetComponent<PlatoonBehaviour>();
                platoon.RpcOrderMovement(destination, heading, mode, enqueue);
            }
        }

        /// <summary>
        /// See PlatoonBehaviour::CancelOrders
        /// </summary>
        [Command]
        public void CmdCancelOrders(
                uint platoonNetId)
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(platoonNetId, out identity))
            {
                PlatoonBehaviour platoon = identity.gameObject.GetComponent<PlatoonBehaviour>();
                platoon.RpcCancelOrders();
            }
        }
    }
}
