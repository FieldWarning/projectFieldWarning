using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shared;

namespace PFW_OfficialHub.Controllers
{
    public static class Db
    {
        static Db()
        {
            Task.Run(delegate {
                while (true) {
                    Task.Delay(10000);
                    OnlinePlayers.DeleteManyAsync(x => x.LastOnline < DateTime.UtcNow - TimeSpan.FromSeconds(15));
                }
            });
        }

        public static IMongoClient Client = new MongoClient();
        public static IMongoDatabase Database = Client.GetDatabase("FieldWarningServer");

        public static IMongoCollection<GameLobby> GameLobbies = Database.GetCollection<GameLobby>("GameLobbies");
        
        public static IMongoCollection<User> Users = Database.GetCollection<User>("Users");
        public static IMongoCollection<Player> OnlinePlayers = Database.GetCollection<Player>("Players");
    }
}
