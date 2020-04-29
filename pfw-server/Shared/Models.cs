using System;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        //public ArmyStyle ArmyStyle { get; set; }
        public string Password { get; set; }

        public bool IsRunning { get; set; }


        public void Join(string playerId)
        {

        }
        public void QuitGame(string lobbyId)
        {

        }
        public void StartLobby()
        {

        }

        public static void GlobalQuit(string playerId)
        {

        }
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

        public DateTime LastOnline { get; set; }
        public string CurrentLobbyId { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
    }

    public class User
    {
        [BsonId][BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
