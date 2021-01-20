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
using PFW.Model;

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
        public void CmdUpdateChat(
                byte playerId, string msg)
        {
            if (MatchSession.Current.Players.Count > playerId)
            {
                ChatManager.UpdateMessageText(MatchSession.Current.Players[playerId], msg);
            }
            else
            {
                Logger.LogNetworking(LogLevel.ERROR,
                    $"Client tried to send a chat msg with an invalid player id {playerId}.");
            }
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

        /// <summary>
        /// Enqueues some units to be bought from the nearest spawn point.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="unitCategoryId"></param>
        /// <param name="unitId"></param>
        /// <param name="unitCount"></param>
        /// <param name="spawnPos"></param>
        /// <param name="destinationCenter"></param>
        /// <param name="destinationHeading"></param>
        [Command]
        public void CmdEnqueuePlatoonPurchase(
                byte playerId,
                byte unitCategoryId,
                int unitId,
                int unitCount,
                byte spawnPointId,
                Vector3 destinationCenter,
                float destinationHeading)
        {
            Logger.LogNetworking(
                    LogLevel.DEBUG,
                    this,
                    $"Enqueueing platoon purchase at spawn pt = {spawnPointId}.");
            if (MatchSession.Current.Players.Count > playerId
                && unitCount >= MIN_PLATOON_SIZE
                && unitCount <= MAX_PLATOON_SIZE)
            {
                if (MatchSession.Current.SpawnPoints.Length > spawnPointId)
                {
                    PlayerData owner = MatchSession.Current.Players[playerId];
                    SpawnPointBehaviour spawn = MatchSession.Current.SpawnPoints[spawnPointId];

                    if (unitCategoryId < owner.Deck.Categories.Length
                        && unitId < GameSession.Singleton.Armory.Categories[unitCategoryId].Count)
                    {
                        Unit unit = GameSession.Singleton.Armory.Categories[unitCategoryId][unitId];

                        GhostPlatoonBehaviour g = 
                            GhostPlatoonBehaviour.CreatePreviewMode(unit, owner, unitCount);
                        g.SetPositionAndOrientation(destinationCenter, destinationHeading);
                        NetworkServer.Spawn(g.gameObject);
                        spawn.BuyPlatoon(g);
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
                    Logger.LogNetworking(LogLevel.ERROR,
                        $"Client asked to create a platoon with an invalid spawn id {spawnPointId}.");
                }
            }
            else
            {
                // Got an invalid player id, client is trying to crash us?
                Logger.LogNetworking(LogLevel.ERROR,
                    $"Client asked to create a platoon with an invalid player id {playerId}.");
            }
        }

        [Command]
        public void CmdSpawnPlatoon(
                byte playerId,
                byte unitCategoryId,
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
                if (unitCategoryId < owner.Deck.Categories.Length
                    && unitId < GameSession.Singleton.Armory.Categories[unitCategoryId].Count)
                {
                    Unit unit = GameSession.Singleton.Armory.Categories[unitCategoryId][unitId];
                    Logger.LogNetworking(LogLevel.INFO, 
                        $"Spawning a platoon with category = {unitCategoryId}, unit id = {unitId}.");

                    PlatoonBehaviour newPlatoon = PlatoonBehaviour.CreateGhostMode(unit, owner);
                    GhostPlatoonBehaviour ghostPlatoon = newPlatoon.GhostPlatoon;
                    ghostPlatoon.transform.position = destinationCenter;
                    ghostPlatoon.FinalHeading = destinationHeading;

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

                    newPlatoon.RpcActivate(spawnPos);
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
                // Got an invalid player id, client is trying to crash us?
                Logger.LogNetworking(LogLevel.ERROR,
                    $"Client asked to create a platoon with an invalid player id {playerId}.");
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

        #region flares
        /// <summary>
        /// Spawn a flare, which is a way for players to draw on the map.
        /// </summary>
        [Command]
        public void CmdSpawnFlare(
                string flareMessage,
                byte playerId,
                Vector3 flarePos)
        {
            Logger.LogNetworking(
                    LogLevel.DEBUG,
                    this,
                    $"Spawning flare '{flareMessage}' at {flarePos}, " +
                    $"requested by player {playerId}.");

            if (MatchSession.Current.Players.Count > playerId)
            {
                UI.Ingame.Flare flare = UI.Ingame.Flare.Create(
                        flareMessage,
                        flarePos,
                        MatchSession.Current.Players[playerId].Team);
                NetworkServer.Spawn(flare.gameObject);
            }
            else
            {
                // Got an invalid player id, client is trying to crash us?
                Debug.LogError(
                        $"Client asked to create a flare " +
                        $"with an invalid player id ({playerId}).");
            }
        }

        /// <summary>
        /// Called when any player right-clicks a flare
        /// </summary>
        [Command]
        public void CmdDestroyFlare(uint flareNetId)
        {
            NetworkIdentity identity;
            if (NetworkIdentity.spawned.TryGetValue(flareNetId, out identity))
            {
                Destroy(identity.gameObject);
            }
        }
        #endregion flares

        #region cheats
        /// <summary>
        /// Change a player's team.
        /// </summary>
        [Command]
        public void CmdChangeTeam(
                Team.TeamName newTeam)
        {
            GetComponent<NetworkedPlayerData>().Team = newTeam;

            // Recalculate visibility for objects whose visibility is managed by mirror:
            // (example: flares)
            NetworkTeamVisibility[] vis = FindObjectsOfType<NetworkTeamVisibility>();
            foreach (NetworkTeamVisibility v in vis)
            {
                v.netIdentity.RebuildObservers(false);
            }
        }
        #endregion
    }
}
