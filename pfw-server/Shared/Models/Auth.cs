using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
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
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime Issued { get; set; }

        public string Serialize() => JsonConvert.SerializeObject(this);
        public static Jwt Parse(string json) => JsonConvert.DeserializeObject<Jwt>(json);
        public Task<bool> Verify()
        {


            return Task.FromResult(true);
        }
    }
}
