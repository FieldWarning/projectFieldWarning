using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Emit;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;
using Shared;
using Shared.Models;

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
            public static List<GameLobby> FindResult = new List<GameLobby>();

            public static void GetLobbies()
            {
                var pres = Post(new Dictionary<string, string>{
                    {"jwt", JsonConvert.SerializeObject(Session.Token) },
                    {"filter", JsonConvert.SerializeObject(Filter) }
                }, "servers/getall");
                FindResult = JsonConvert.DeserializeObject<List<GameLobby>>(pres);
            }
            //public static bool JoinLobby(string id)
            //{
            //    var pres = Post(new Dictionary<string, string>(), $"server/join/{id}");
            //    if (pres == "200") CurrentLobby = GetLobbyById(id);
            //}



            public static GameLobby Get(string id)
            {
                return new GameLobby();
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

        public static class WarChat
        {
            public static List<Message>
        }

        public static class LobbyChat
        {
            
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
