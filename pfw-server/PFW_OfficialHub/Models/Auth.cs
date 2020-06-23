using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PFW_OfficialHub.Models
{
    public class Jwt
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
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
