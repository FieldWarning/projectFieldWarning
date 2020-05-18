using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PFW_OfficialHub.Controllers;
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
                Console.Write("Db connection string file not found, add path to file (will crash if no file): ");
                path = Console.ReadLine();
            }

            try
            {
                string conStr = File.ReadAllText(path);
                Console.WriteLine("Found db file in => " + path);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                throw;
            }

            Task.Run(delegate {
                while (true) {
                    Task.Delay(10000).Wait();
                    Players.DeleteManyAsync(x => x.LastSeen < DateTime.UtcNow - TimeSpan.FromSeconds(15));
                }
            });

        }


        /// <summary>
        /// To get access to online db, ask a dev
        /// </summary>
        public static IMongoClient Client = new MongoClient();
        public static IMongoDatabase Database = Client.GetDatabase("FieldWarningServer");

        public static IMongoCollection<GameLobby> GameLobbies = Database.GetCollection<GameLobby>("GameLobbies");
        public static ConcurrentDictionary<string, GameLobby> LiveLobbies = new ConcurrentDictionary<string, GameLobby>();

        public static IMongoCollection<User> Users = Database.GetCollection<User>("Users");
        public static IMongoCollection<Player> Players = Database.GetCollection<Player>("Players");
        public static IMongoCollection<WarchatMsg> Warchat = Database.GetCollection<WarchatMsg>("WarchatMessages");
        public static IMongoCollection<PrivateMessage> Messages = Database.GetCollection<PrivateMessage>("PrivateMessages");
        public static void Init() => Console.WriteLine("Db Init");
    }
}
