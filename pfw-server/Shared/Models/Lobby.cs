/*
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

using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models
{
    public class GameLobby
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string HostUserId { get; set; }
        public int StartingPoints { get; set; }

        /// <summary>
        /// Per 10 second per player
        /// </summary>
        public int Income { get; set; }

        /// <summary>
        /// This is not to be used unless work on game modes have started 
        /// </summary>
        public GameMode GameMode { get; set; }
        ///// <summary>
        ///// Use only for deck building --experimental
        ///// </summary>
        public ArmyStyle ArmyStyle { get; set; }
        public string Password { get; set; }

        public bool IsRunning { get; set; }
        public DateTime LastHeartbeat { get; set; }
    }

    /// <summary>
    /// Filter for searching in lobbies
    /// </summary>
    public class LobbySearchFilter
    {
        /// <summary>
        /// Add all game modes by default
        /// </summary>
        public LobbySearchFilter() { foreach (var value in Enum.GetNames(typeof(GameMode))) GameModes.Add((GameMode)Enum.Parse(typeof(GameMode), value)); }

        public string NameContains = string.Empty;
        public List<GameMode> GameModes = new List<GameMode>();
        public bool HasPassword = false;
        public bool RunningGames = false;
    }


    /// <summary>
    /// This is not to be used unless work on game modes have started 
    /// </summary>
    public enum GameMode
    {
        Free,
        Conquest,
        Destruction,
        Economy
    }

    /// <summary>
    /// Don't use unless work on deckbuilding has started
    /// </summary>
    public enum ArmyStyle
    {
        /// <summary>
        /// Least available slots, it is a small tactical group
        /// </summary>
        Company,
        /// <summary>
        /// It is a small battle group that has more slots than Company
        /// </summary>
        Battalion,
        /// <summary>
        /// largest possible battle group intended for big battles where there are a lot of units
        /// </summary>
        Brigade
    }
}
