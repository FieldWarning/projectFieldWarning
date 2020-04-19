using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

        /// <summary>
        /// Gets disposed on app exit
        /// </summary>
        public static HttpClient Client = new HttpClient();
        public static class Auth
        {
            
        }



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
