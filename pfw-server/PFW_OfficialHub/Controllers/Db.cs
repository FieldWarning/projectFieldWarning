using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shared;

namespace PFW_OfficialHub.Controllers
{
    public static class Db
    {
        /// <summary>
        /// !!!Do not include connection string in repository!!!
        /// </summary>
        static Db()
        {
            string conStr;
            try
            {
                conStr = File.ReadAllText("C:\\Users\\admin\\Documents\\pfwdb.txt");

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Db connection string file not found, add path to file (will crash if no file): ");
                var path = Console.ReadLine();
                conStr = File.ReadAllText();
            }

            Client = new MongoClient(conStr);

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
