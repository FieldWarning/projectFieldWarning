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

            public static bool Login(string username, string password)
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
            public static SynchronizedCollection<GameLobby> FindResult = new SynchronizedCollection<GameLobby>();

            public static void GetLobbies()
            {
                var pres = Post(new Dictionary<string, string>{
                    {"jwt", Session.Token.Serialize() },
                    {"filter", JsonConvert.SerializeObject(Filter) }
                }, "servers/getall");
                FindResult.Clear();
                Parallel.ForEach(JsonConvert.DeserializeObject<List<GameLobby>>(pres), i => FindResult.Add(i));
            }

            public static bool JoinLobby(string id, string password = null)
            {
                var pres = Post(new Dictionary<string, string>(), $"server/join/{id}");
                if (pres != "200") return false;

                CurrentLobby = GetLobby(id);
                return true;
            }

            public static GameLobby GetLobby(string id)
            {
                var res = Post(new Dictionary<string, string>()
                    {{"jwt", Session.Token.Serialize()}}, $"info/{id}");
                return JsonConvert.DeserializeObject<GameLobby>(res);
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
            public static SynchronizedCollection<WarchatMsg> Messages = new SynchronizedCollection<WarchatMsg>();

            public static Task GetMessages(DateTime since)
            {
                var res = Post(new Dictionary<string, string>()
                {
                    {"time", since.ToString("O")},
                    {"jwt", Session.Token.Serialize()}
                }, "warchat/get");
                //if ()
                var msgs = JsonConvert.DeserializeObject<List<WarchatMsg>>(res);
                Parallel.ForEach(msgs, msg => Messages.Add(msg));
                return Task.CompletedTask;
            }

            public static Task SendMessage(WarchatMsg msg)
            {
                var res = Post(new Dictionary<string, string>()
                {
                    {"jwt", Session.Token.Serialize()},
                    {"msg", JsonConvert.SerializeObject(msg)}
                }, "warchat/send");
                return Task.CompletedTask;
            }
        }

        public static class LobbyChat
        {
            
        }

        public static class PrivateMessages
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
