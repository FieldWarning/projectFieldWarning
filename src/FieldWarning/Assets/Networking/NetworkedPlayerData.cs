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
    /// Player data attached to the player prefab.
    /// Figure out how to merge with the PlayerData class.
    /// (Main problem: Adapting the PlatoonBehavior.Owner field)
    /// </summary>
    public class NetworkedPlayerData : NetworkBehaviour
    {
        [SyncVar]
        public Team.TeamName Team;
    }
}
