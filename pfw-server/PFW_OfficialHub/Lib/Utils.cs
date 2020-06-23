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
