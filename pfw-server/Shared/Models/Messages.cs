using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models
{
    public class PrivateMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string TargetId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
    }


    public class WarchatMsg
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public DateTime Time { get; set; }
        public string Content { get; set; }
    }
}
