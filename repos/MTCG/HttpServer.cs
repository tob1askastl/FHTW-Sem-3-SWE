using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class HttpServer : IDisposable
    {
        private readonly TcpListener tcpListener;
        private bool isRunning;

        public HttpServer(string ipAddress, int port)
        {
            tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            isRunning = true;
            tcpListener.Start();

            while (isRunning)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient tcpClient)
        {
            using (NetworkStream networkStream = tcpClient.GetStream())
            using (StreamReader reader = new StreamReader(networkStream))
            using (StreamWriter writer = new StreamWriter(networkStream))
            {
                // Read the HTTP request
                string request = reader.ReadLine();
                Console.WriteLine($"Received request: {request}");

                // Parse the request and route to the appropriate handler
                if (request.StartsWith("POST /register"))
                {
                    // Handle user registration
                    HandleUserRegistration(reader, writer);
                }
                else if (request.StartsWith("POST /login"))
                {
                    // Handle user login
                    HandleUserLogin(reader, writer);
                }
                else if (request.StartsWith("GET /cards"))
                {
                    // Handle getting user cards
                    HandleGetUserCards(reader, writer);
                }
                // Add more handlers for other endpoints...

                // Send a simple response
                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, world!";
                writer.WriteLine(response);
                writer.Flush();
            }

            tcpClient.Close();
        }

        private void HandleUserRegistration(StreamReader reader, StreamWriter writer)
        {
            // Implement user registration logic
        }

        private void HandleUserLogin(StreamReader reader, StreamWriter writer)
        {
            // Implement user login logic
        }

        private void HandleGetUserCards(StreamReader reader, StreamWriter writer)
        {
            // Implement logic to get user cards
        }

        // Add more handler methods for different endpoints...

        public void Dispose()
        {
            isRunning = false;
            tcpListener.Stop();
        }
    }
}
