using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shared;

namespace Database
{
    /// <summary>
    /// !!!Do not include connection string in repository!!!
    /// </summary>
    public static class Db
    {
        static Db()
        {
            var path = "C:\\Users\\admin\\Documents\\pfwdb.txt";
            //TODO save default path for dev purposes

            while (!File.Exists(path))
            {
                Console.WriteLine("Db connection string file not found, add path to file (will crash if no file): ");
                path = Console.ReadLine();
            }
            Console.WriteLine("Found db file in => "+path);
            string conStr = File.ReadAllText(path);

            Task.Run(delegate {
                while (true) {
                    Task.Delay(10000).Wait();
                    OnlinePlayers.DeleteManyAsync(x => x.LastOnline < DateTime.UtcNow - TimeSpan.FromSeconds(15));
                }
            });
        }


        /// <summary>
        /// To get access to online db, ask a dev
        /// </summary>
        public static IMongoClient Client = new MongoClient();
        public static IMongoDatabase Database = Client.GetDatabase("FieldWarningServer");

        public static IMongoCollection<GameLobby> GameLobbies = Database.GetCollection<GameLobby>("GameLobbies");
        
        public static IMongoCollection<User> Users = Database.GetCollection<User>("Users");
        public static IMongoCollection<Player> OnlinePlayers = Database.GetCollection<Player>("Players");
        public static void Init() => Console.WriteLine("Db Init");
    }
}
