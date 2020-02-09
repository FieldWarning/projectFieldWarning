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
using Mirror;
using UnityEngine;

using PFW.Model.Armory;
using PFW.Model.Game;
using PFW.UI.Ingame;
using PFW.Units;
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

        [Command]
        public void CmdSpawnUnit() 
        {
            // TODO
            return;
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
            Logger.LogNetworking($"Spawning platoon at {spawnPos}", this);
            if (MatchSession.Current.Players.Count > playerId 
                && unitCount >= MIN_PLATOON_SIZE
                && unitCount <= MAX_PLATOON_SIZE)
            {
                PlayerData owner = MatchSession.Current.Players[playerId];
                if (categoryId < owner.Deck.Categories.Length
                    && unitId < owner.Deck.Categories[categoryId].Count)
                {
                    Unit unit = owner.Deck.Categories[categoryId][unitId];

                    GameObject go = Instantiate(Resources.Load<GameObject>("PlatoonRoot"));
                    PlatoonRoot root = go.GetComponent<PlatoonRoot>();

                    // mirror does not support networking nested objects, so 
                    // everything has to be spawned at the toplevel..
                    GhostPlatoonBehaviour ghostPlatoon = Instantiate(
                            Resources.Load<GameObject>(
                                    "GhostPlatoon")).GetComponent<GhostPlatoonBehaviour>();
                    PlatoonBehaviour realPlatoon = Instantiate(
                            Resources.Load<GameObject>(
                                    "Platoon")).GetComponent<PlatoonBehaviour>();

                    ghostPlatoon.Initialize(unit, owner);
                    realPlatoon.Initialize(unit, owner);

                    NetworkServer.Spawn(ghostPlatoon.gameObject);
                    NetworkServer.Spawn(realPlatoon.gameObject);
                    NetworkServer.Spawn(go);

                    uint[] unitIds = new uint[unitCount];
                    for (int i = 0; i < unitCount; i++)
                    {
                        GameObject unitGO = Instantiate(unit.Prefab);
                        // Any added unit initialization must be done in an RPC,
                        // otherwise it won't show up on the clients!

                        NetworkServer.Spawn(unitGO);
                        unitIds[i] = unitGO.GetComponent<NetworkIdentity>().netId;
                    }

                    root.RpcEstablishReferences(realPlatoon.netId, ghostPlatoon.netId, unitIds);

                    ghostPlatoon.RpcSetOrientation(destinationCenter, destinationHeading);

                    root.RpcSpawn(spawnPos);
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
    }
}
