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
using System.Linq;
using System.Threading.Tasks;
using Database;
using MongoDB.Driver;
using Shared;
using Shared.Models;
using Jwt = PFW_OfficialHub.Models.Jwt;

namespace PFW_OfficialHub.Lib
{
    public static class Utils
    {
        public static bool VerifyJwt(Jwt jwt)
        {
            return true;
        }

        public static Task<Player> GetPlayer(Jwt jwt) => Task.FromResult(Db.Players.Find(x => x.Username == jwt.Username).FirstOrDefault());
        public static Task<User> GetUser(Jwt jwt) => Task.FromResult(Db.Users.Find(x => x.Username == jwt.Username).FirstOrDefault());
    }

    public static class ClientIterop
    {
        static void PushLobbyCommand(LobbyCommand command, string endpoint)
        {

        }

        static void PushSocialCommand(SocialCommand command, string endpoint)
        {

        }
    }
}
