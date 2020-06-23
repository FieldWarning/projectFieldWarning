using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Shared.WebServer
{
    public static class Listener
    {
        static HttpListener listener = new HttpListener();
        public static void Run(int port = 29990)
        {
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();
            while (listener.IsListening)
            {
                var req = listener.GetContext();
                //req.



            }

        }

    }
}
