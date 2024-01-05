using MTCG.Json;
using MTCG.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        CardRepository cardRepository;

        public HttpClientHandler(Socket client)
        {
            this.client = client;
            userRepository = new UserRepository();
            cardRepository = new CardRepository();
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

            // Curl 1 Registration-Pfad "/users"
            if (method == "POST" && path == "/users")
            {
                HandleRegistration(requestLines);
            }

            // Curl 2 Login-Pfad "/sessions"
            else if (method == "POST" && path == "/sessions")
            {
                HandleLogin(requestLines);
            }

            // CURL 3 
            else if (method == "POST" && path == "/packages")
            {
                HandlePackage(requestLines);
            }

            // CURL 4
            else if (method == "POST" && path == "/transactions/packages")
            {
                HandleBuyAndOpenPackages(requestLines);
            }

            // CURL 8
            else if (method == "GET" && path == "/cards")
            {
                //ShowAllCardsFromUser(requestLines);
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
            string response;

            User newUser = new User(username, password);

            // Registration erfolgreich
            if (userRepository.AddUser(newUser, out string responseMessage))
            {
                Console.WriteLine($"Registration erfolgreich für Benutzer '{username}'.");
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nRegistration successful\r\n";
            }

            // Registration fehlgeschlagen
            else
            {
                Console.WriteLine($"Registration fehlgeschlagen für Benutzer '{username}': {responseMessage}");

                response = "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n" + responseMessage + "\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void HandleLogin(string[] requestLines)
        {
            string requestBody = requestLines[requestLines.Length - 1].Trim();

            JsonUserData loginData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonUserData>(requestBody);

            string username = loginData.Username;
            string password = loginData.Password;

            // Anmeldung erfolgreich
            if (userRepository.ValidateUserCredentials(username, password))
            {
                string token = GenerateToken(username);
                HttpServer.Server.TokensLoggedInUsers.Add(token);

                Console.WriteLine($"Anmeldung erfolgreich für Benutzer '{username}'.");
                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nLogin successful\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            // Anmeldung fehlgeschlagen
            else
            {
                Console.WriteLine($"Anmeldung fehlgeschlagen für Benutzer '{username}'.");
                string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\nLogin failed\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            foreach (string usertoken in HttpServer.Server.TokensLoggedInUsers)
            {
                Console.WriteLine(usertoken);
            }
        }

        private string GenerateToken(string username)
        {
            return $"{username}-mtcgToken";
        }

        private void HandlePackage(string[] requestLines)
        {
            string requestBody = requestLines[requestLines.Length - 1].Trim();
            Console.WriteLine(requestBody);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None // Disable TypeNameHandling
            };

            JArray jsonArray = JArray.Parse(requestBody);

            foreach (JObject jsonObject in jsonArray)
            {
                if (jsonObject.TryGetValue("card_type", StringComparison.OrdinalIgnoreCase, out JToken typeToken))
                {
                    string cardType = typeToken.ToString();

                    switch (cardType)
                    {
                        case "Champion":
                            Champion champion = jsonObject.ToObject<Champion>(JsonSerializer.Create(settings));
                            cardRepository.AddCard(champion);
                            break;

                        case "Spell":
                            Spell spell = jsonObject.ToObject<Spell>(JsonSerializer.Create(settings));
                            cardRepository.AddCard(spell);
                            break;

                        default:
                            // Handle unknown card types or log an error
                            break;
                    }
                }
                else
                {
                    // Handle cases where "card_type" attribute is missing
                    // You may want to log an error or handle it based on your application logic
                }
            }

            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nSuccessfully created Package\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        // In deinem `HttpClientHandler`-Code

        private void HandleBuyAndOpenPackages(string[] requestLines)
        {
            // Überprüfe die Autorisierung
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            Console.WriteLine(token);

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Überprüfe, ob der Benutzer genügend Geld hat
            if (!userRepository.HasEnoughMoneyForPackage(token))
            {
                // Benutzer hat nicht genug Geld
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nNot enough money to buy a package";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Kaufe und öffne das Paket
            //List<Card> boughtCards = userRepository.BuyAndOpenPackage(token);

            // Handle die Rückgabe (z.B., sende eine Liste der erhaltenen Karten als JSON)
            //string jsonResponse = JsonConvert.SerializeObject(boughtCards);
            //response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}";
            //client.Send(Encoding.ASCII.GetBytes(response));
        }



        /*
        private void ShowAllCardsFromUser(string[] requestLines)
        {
            // Annahme: Der Header enthält das Autorisierungstoken
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            // Überprüfe die Autorisierung
            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Autorisierung erfolgreich, erhalte die Benutzerkarten
            List<Card> userCards = userRepository.GetUserCardsByToken(token);

            // Erstelle die Antwortnachricht mit den Benutzerkarteninformationen
            response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n" + JsonConvert.SerializeObject(userCards);
            client.Send(Encoding.ASCII.GetBytes(response));
        }
        */

    }
}
