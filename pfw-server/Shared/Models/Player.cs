using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime LastSeen { get; set; }
        public string CurrentLobbyId { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string CurrentEndpoint { get; set; }
    }
}
