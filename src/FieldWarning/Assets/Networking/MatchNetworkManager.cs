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
using PFW.Model.Match;
using UnityEngine;

namespace PFW.Networking
{
    /// <summary>
    /// Standard mirror network manager + logic for initializing our own player
    /// objects.
    /// </summary>
    public class MatchNetworkManager : NetworkManager
    {
        public struct PlayerJoinMessage : NetworkMessage
        {
            public string Deck;
            public Team.TeamName Team;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler<PlayerJoinMessage>(OnPlayerJoin);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            // you can send the message here, or wherever else you want
            PlayerJoinMessage characterMessage = new PlayerJoinMessage
            {
                Deck = "",
                Team = MatchSession.Current.LocalPlayer.Data.Team.Name
            };

            conn.Send(characterMessage);
        }

        void OnPlayerJoin(NetworkConnection conn, PlayerJoinMessage message)
        {
            // playerPrefab is the one assigned in the inspector in Network
            // Manager but you can use different prefabs per race for example
            GameObject gameobject = Instantiate(playerPrefab);

            // Apply data from the message however appropriate for your game
            // Typically Player would be a component you write with syncvars or properties
            NetworkedPlayerData player = gameobject.GetComponent<NetworkedPlayerData>();
            player.Team = message.Team;

            // TODO sync decks

            // call this to use this gameobject as the primary controller
            NetworkServer.AddPlayerForConnection(conn, gameobject);
        }
    }
}
