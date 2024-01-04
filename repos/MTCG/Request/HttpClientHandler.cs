using MTCG.Json;
using MTCG.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Request
{
    internal class HttpClientHandler
    {
        private static readonly string TOKEN = "token";
        private Socket client;
        private const int bufferSize = 4096;

        UserRepository userRepository;
        ChampionRepository championRepository;
        SpellRepository spellRepository;

        public HttpClientHandler(Socket client)
        {
            this.client = client;
            userRepository = new UserRepository();
        }

        public void Start(object state)
        {
            HandleRequest();
        }

        private void HandleRequest()
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead = client.Receive(buffer);

            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            string[] requestLines = request.Split('\n');

            string[] requestLine = requestLines[0].Trim().Split(' ');
            string method = requestLine[0];
            string path = requestLine[1];

            // Registration-Pfad "/users"
            if (method == "POST" && path == "/users")
            {
                HandleRegistration(requestLines);
            }

            // Login-Pfad "/sessions"
            if (method == "POST" && path == "/sessions")
            {
                HandleLogin(requestLines);
            }
            
            client.Close();
        }

        private void HandleRegistration(string[] requestLines)
        {
            string username;
            string password;

            string requestBody = requestLines[requestLines.Length - 1].Trim();

            UserRegistrationData userData = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRegistrationData>(requestBody);

            // Extrahiere Username und Password
            username = userData.Username;
            password = userData.Password;

            User newUser = new User(username, password);
            userRepository.AddUser(newUser);

            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nRegistration successful";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void HandleLogin(string[] requestLines)
        {

        }
    }
}
