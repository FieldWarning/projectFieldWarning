using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Emit;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;
using Shared;

namespace Ez
{
    public static class Ez
    {
        //meta
        public static string SelectedServer = "http://localhost:5000/";
        public static List<string> HubServerList = new List<string>
        {
            SelectedServer,
            "https://pfw.pw"
        };
        //meta


        public static class Session
        {
            static Session()
            {

            }

            public static Player Self = new Player();
            public static User User = new User();
            public static Jwt Token = new Jwt();

            public static void Login(string username, string password)
            {

            }

            public static void Logout()
            {

            }
        }
        public static class Lobbies
        {
            static Lobbies()
            {
                Task.Run(() => GetLobbies());

            }

            public static GameLobby CurrentLobby = new GameLobby();
            public static LobbySearchFilter Filter = new LobbySearchFilter();
            public static LockList<GameLobby> FindResult = new LockList<GameLobby>();

            public static void GetLobbies()
            {
                var lobbies = Post(new Dictionary<string, string>{
                    {"jwt", JsonConvert.SerializeObject(Session.Token) },
                    {"filter", JsonConvert.SerializeObject(Filter) }
                }, "servers/getall");
            }

        }
        public static class Auth
        {
            public static bool Authed = false;
            public static bool ShowOnline = false;

            public static bool Login()
            {
                return true;
            }

            public static bool Logout()
            {
                return true;
            }

            public static bool CheckLogin()
            {
                return true;
            }
        }



        /// <summary>
        /// Gets disposed on app exit
        /// </summary>
        public static HttpClient Client = new HttpClient();
        public static string Post(Dictionary<string, string> form, string url)
        {
            string ret;
            try
            {
                using (var c = new FormUrlEncodedContent(form))
                {
                    ret = Client.PostAsync($"{SelectedServer}{url}", c).Result.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e) { ret = e.ToString(); }
            return ret;
        }

        public static async Task<string> PostAsync(Dictionary<string, string> form, string url)
        {
            string ret;
            try
            {
                using (var c = new FormUrlEncodedContent(form))
                {
                    ret = await Client.PostAsync($"{SelectedServer}{url}", c).Result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e) { ret = e.ToString(); }

            return ret;
        }
    }
}
