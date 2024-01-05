using MTCG.Repositories;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MTCG.Request
{
    public class HttpServer
    {
        public static readonly int PORT = 10001;
        public static readonly string URL = "http://127.0.0.1:" + HttpServer.PORT;
        public ConcurrentBag<string> TokensLoggedInUsers;

        private Socket listeningSocket;

        private static HttpServer server;
        public static HttpServer Server
        {
            get
            {
                if (server == null) 
                {
                    server = new HttpServer();
                }

                return server;
            }
        }

        public HttpServer()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   
            TokensLoggedInUsers = new ConcurrentBag<string>();
        }

        public void StartServer()
        {
            listeningSocket.Bind(new IPEndPoint(IPAddress.Loopback, PORT));
            listeningSocket.Listen(PORT);

            Console.WriteLine("Http Server is running");

            Console.WriteLine("Tables are being truncated...");
            UserRepository.TruncateTable();
            CardRepository.TruncateTable();

            while (true)
            {
                Socket client = listeningSocket.Accept();
                HttpClientHandler handler = new HttpClientHandler(client);
                ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Start));
            }
        }

        public void StopServer() 
        {
            if (listeningSocket.Connected)
            {
                listeningSocket.Close();
            }
        }
    }
}
