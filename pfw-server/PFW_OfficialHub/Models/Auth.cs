///*
// * Copyright (c) 2017-present, PFW Contributors.
// *
// * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
// * compliance with the License. You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software distributed under the License is
// * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
// * the License for the specific language governing permissions and limitations under the License.
//*/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;

//namespace PFW_OfficialHub.Models
//{
//    public class Jwt
//    {
//        [BsonId]
//        [BsonRepresentation(BsonType.ObjectId)]
//        public string Id { get; set; }

//        [BsonRepresentation(BsonType.ObjectId)]
//        public string UserId { get; set; }
//        public string Username { get; set; }
//        public string Token { get; set; }
//        public DateTime Issued { get; set; }

//        public bool Verify()
//        {
//            return true;
//        }
//    }
//}
