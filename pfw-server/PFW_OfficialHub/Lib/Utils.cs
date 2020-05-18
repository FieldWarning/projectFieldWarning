using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using MongoDB.Driver;
using Shared;

namespace PFW_OfficialHub.Lib
{
    public static class Utils
    {
        public static bool VerifyJwt(Jwt jwt)
        {
            return true;
        }

        public static Task<Player> GetPlayer(Jwt jwt)
        {
            var p = Db.Players.Find(x => x.Username == jwt.Username).FirstOrDefault();
            return Task.FromResult(p);
        }
        public static User GetUser(Jwt jwt)
        {
            return Db.Users.Find(x=> x.Username == jwt.Username).FirstOrDefault();
        }

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
