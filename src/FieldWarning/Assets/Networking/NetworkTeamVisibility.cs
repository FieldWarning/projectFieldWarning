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
using Mirror;
using System.Collections.Generic;
using static PFW.Model.Match.Team;
using PFW.Model.Match;

namespace PFW.Networking
{
    /// <summary>
    /// See https://mirror-networking.com/docs/Articles/Guides/Visibility.html
    /// 
    /// Implements callbacks that tell mirror which objects should be
    /// shown to which players, based on the team the players are on.
    /// </summary>
    public sealed class NetworkTeamVisibility : NetworkVisibility
    {
        private TeamName _team = TeamName.UNDEFINED;

        public void Initialize(TeamName team)
        {
            _team = team;
            netIdentity.RebuildObservers(true);
        }

        /// <summary>
        /// Callback used by the visibility system to determine if
        /// an observer (player) can see this object.
        /// <para>
        ///     If this function returns true, the network connection
        ///     will be added as an observer.
        /// </para>
        /// </summary>
        /// <param name="conn">Network connection of a player.</param>
        /// <returns>True if the player can see this object.</returns>
        public override bool OnCheckObserver(NetworkConnection conn)
        {
            NetworkedPlayerData data = conn.identity.GetComponent<NetworkedPlayerData>();
            return data.Team == _team;
        }

        /// <summary>
        /// Callback used by the visibility system to (re)construct the set of observers
        /// that can see this object.
        /// <para>
        ///     Implementations of this callback should add network connections of
        ///     players that can see this object to the observers set.
        /// </para>
        /// </summary>
        /// <param name="observers">The new set of observers for this object.</param>
        /// <param name="initialize">True if the set of observers is being built for the first time.</param>
        public override void OnRebuildObservers(
                HashSet<NetworkConnection> observers, bool initialize)
        {
            if (_team == TeamName.UNDEFINED)
                return;

            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                NetworkedPlayerData data = conn.identity.GetComponent<NetworkedPlayerData>();
                if (data.Team == _team)
                {
                    observers.Add(conn);
                }
            }
        }

        /// <summary>
        /// Callback used by the visibility system for objects on a host.
        /// <para>Objects on a host (with a local client) cannot be disabled or destroyed when they are not visible to the local client. So this function is called to allow custom code to hide these objects. A typical implementation will disable renderer components on the object. This is only called on local clients on a host.</para>
        /// </summary>
        /// <param name="visible">New visibility state.</param>
        public override void OnSetHostVisibility(bool visible)
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
            foreach (Canvas canv in GetComponentsInChildren<Canvas>())
                canv.enabled = visible;
            foreach (Light light in GetComponentsInChildren<Light>())
                light.enabled = visible;
        }
    }
}
