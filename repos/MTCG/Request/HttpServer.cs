using MTCG.Database;
using MTCG.Repositories;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MTCG.Request
{

    /*
    docker exec -it MTCG psql -U postgres
    \c MTCG_DB
    \dt
    \d mtcg_users
     */

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

        private readonly DbHandler dbHandler;

        public HttpServer()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   
            TokensLoggedInUsers = new ConcurrentBag<string>();
            dbHandler = new DbHandler();
        }

        public void StartServer()
        {
            listeningSocket.Bind(new IPEndPoint(IPAddress.Loopback, PORT));
            listeningSocket.Listen(PORT);

            Console.WriteLine("Http Server is running");

            DropTables();
            CreateTableUsers();
            CreateTableCards();

            while (true)
            {
                Socket client = listeningSocket.Accept();
                HttpClientHandler handler = new HttpClientHandler(client);
                ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Start));
            }
        }

        public void CreateTableUsers()
        {
            using (NpgsqlConnection connection = dbHandler.GetConnection())
            {
                dbHandler.OpenConnection(connection);

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS mtcg_users (id SERIAL PRIMARY KEY, username VARCHAR(255) UNIQUE NOT NULL, password VARCHAR(255) NOT NULL, bio VARCHAR(255), image VARCHAR(255), ritopoints INTEGER);";

                    command.ExecuteNonQuery();
                }

                dbHandler.CloseConnection(connection);
            }
        }

        public void CreateTableCards()
        {
            using (NpgsqlConnection connection = dbHandler.GetConnection())
            {
                dbHandler.OpenConnection(connection);

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS mtcg_cards (card_id SERIAL PRIMARY KEY, name VARCHAR(255) NOT NULL, region INTEGER, damage INTEGER NOT NULL, card_type VARCHAR(20) NOT NULL DEFAULT 'Unknown', is_used BOOLEAN DEFAULT false, owner_id INTEGER, FOREIGN KEY (owner_id) REFERENCES mtcg_users(id) ON UPDATE CASCADE ON DELETE SET NULL, is_in_deck BOOLEAN DEFAULT false);";

                    command.ExecuteNonQuery();
                }

                dbHandler.CloseConnection(connection);
            }
        }


        public void DropTables()
        {
            using (NpgsqlConnection connection = dbHandler.GetConnection())
            {
                dbHandler.OpenConnection(connection);

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DROP TABLE IF EXISTS mtcg_users, mtcg_cards;";

                    command.ExecuteNonQuery();
                }

                dbHandler.CloseConnection(connection);
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
