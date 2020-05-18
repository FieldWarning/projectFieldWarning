//#define 

using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
//using PFW_OfficialHub.Controllers




namespace Shared
{
    public class GameLobby  
    {
        [BsonId][BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public int Points { get; set; }

        /// <summary>
        /// Per 30 second per player
        /// </summary>
        public int Income { get; set; }
        
        /// <summary>
        /// This is not to be used unless work on game modes have started 
        /// </summary>
        public GameMode GameMode { get; set; }
        ///// <summary>
        ///// Use only for deck building --experimental
        ///// </summary>
        //public ArmyStyle ArmyStyle { get; set; }
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

    public class Player
    {
        [BsonId][BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime LastSeen { get; set; }
        public string CurrentLobbyId { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string CurrentEndpoint { get; set; }
    }

    public class User
    {
        [BsonId][BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }

    /// <summary>
    /// Pass this serialized object on every request if doing custom requests
    /// </summary>
    public class Jwt
    {
        [BsonId][BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime Issued { get; set; }
    
        public bool Verify()
        {
            return true;
        }
    }


}
