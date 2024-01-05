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
            else if (method == "POST" && path == "/sessions")
            {
                HandleLogin(requestLines);
            }

            // 
            else if (method == "POST" && path == "/")
            {

            }

            // 
            else if (method == "POST" && path == "/")
            {

            }

            // 
            else if (method == "POST" && path == "/")
            {

            }

            // 
            else if (method == "POST" && path == "/")
            {

            }

            // 
            else if (method == "POST" && path == "/")
            {

            }

            // 
            else if (method == "POST" && path == "/")
            {

            }

            client.Close();
        }

        private void HandleRegistration(string[] requestLines)
        {
            string requestBody = requestLines[requestLines.Length - 1].Trim();

            JsonUserData userData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonUserData>(requestBody);

            // Extrahiere Username und Password
            string username = userData.Username;
            string password = userData.Password;

            User newUser = new User(username, password);
            userRepository.AddUser(newUser);

            Console.WriteLine($"Anmeldung erfolgreich für Benutzer '{username}'.");

            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nRegistration successful\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void HandleLogin(string[] requestLines)
        {
            string requestBody = requestLines[requestLines.Length - 1].Trim();

            JsonUserData loginData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonUserData>(requestBody);

            string username = loginData.Username;
            string password = loginData.Password;

            // Überprüfe die Anmeldeinformationen im UserRepository
            if (userRepository.ValidateUserCredentials(username, password))
            {
                // Anmeldung erfolgreich
                string token = GenerateToken(username);
                userRepository.AddTokenToUser(username, token);

                Console.WriteLine($"Anmeldung erfolgreich für Benutzer '{username}'.");
                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nLogin successful\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
            else
            {
                // Anmeldung fehlgeschlagen
                Console.WriteLine($"Anmeldung fehlgeschlagen für Benutzer '{username}'.");
                string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\nLogin failed\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
        }

        private string GenerateToken(string username)
        {
            // Beispiel: Generiere ein einfaches Token mit dem Format "<username>-mtcgToken"
            return $"{username}-mtcgToken";
        }

    }
}
