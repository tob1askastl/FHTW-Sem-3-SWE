using MTCG.Json;
using MTCG.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace MTCG.Request
{
    internal class HttpClientHandler
    {
        private Socket client;
        private const int bufferSize = 4096;

        UserRepository userRepository;
        CardRepository cardRepository;

        private readonly BattleManager battleManager;

        public HttpClientHandler(Socket client)
        {
            this.client = client;
            userRepository = new UserRepository();
            cardRepository = new CardRepository();
            battleManager = new BattleManager();
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

            // Curl 1 - Registration
            if (method == "POST" && path == "/users")
            {
                HandleRegistration(requestLines);
            }

            // Curl 2 - Login
            else if (method == "POST" && path == "/sessions")
            {
                HandleLogin(requestLines);
            }

            // CURL 3 - Create Packages
            else if (method == "POST" && path == "/packages")
            {
                HandlePackage(requestLines);
            }

            // CURL 4 bis 7 - Buy Packages / Not enough Money / No more cards
            else if (method == "POST" && path == "/transactions/packages")
            {
                HandleBuyAndOpenPackages(requestLines);
            }

            // CURL 8, 9 - View all Cards from user
            else if (method == "GET" && path == "/cards")
            {
                ShowAllCardsFromUser(requestLines);
            }

            // CURL 11, 12 - Configure Deck and Show
            else if (method == "PUT" && path == "/deck")
            {
                HandleConfigureDeck(requestLines);
            }

            // CURL 10, 13 - Different Outputstyle
            else if (method == "GET" && path.StartsWith("/deck"))
            {
                // 2 Varianten im Curl: "/deck" und "/deck?format=plain"
                if (path.Contains("format=plain"))
                {
                    ShowDeckInPlainText(requestLines);
                }

                else
                {
                    ShowDeck(requestLines);
                }
            }

            // CURL 14 - Show User Data
            else if (method == "GET" && path.StartsWith("/users/"))
            {
                ShowUserData(requestLines);
            }

            // CURL 14 - Edit User Data
            else if (method == "PUT" && path.StartsWith("/users/"))
            {
                HandleEditUserData(requestLines);
            }

            // CURL 15 - Stats (single User info)
            else if (method == "GET" && path == "/stats")
            {
                HandleViewStats(requestLines);
            }

            // CURL 16 - Scoreboard (all users info)
            else if (method == "GET" && path == "/scoreboard")
            {
                HandleViewScoreboard(requestLines);
            }

            // CURL 17 - Battle
            else if (method == "POST" && path == "/battles")
            {
                HandleStartBattle(requestLines);
            }

            client.Close();
        }

        private void HandleRegistration(string[] requestLines)
        {
            string requestBody = requestLines[requestLines.Length - 1].Trim();

            // Try-Catch
            JsonUserData userData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonUserData>(requestBody);

            string response;
            User newUser = new User();

            newUser.Username = userData.Username;
            newUser.Password = userData.Password;

            // Registration erfolgreich
            if (userRepository.AddUser(newUser, out string responseMessage))
            {
                Console.WriteLine($"Registration erfolgreich für User '{newUser.Username}'.");
                response = "HTTP/1.1 201 OK\r\nContent-Type: text/plain\r\n\r\nUser successfully created\r\n";
            }

            // Registration fehlgeschlagen
            else
            {
                Console.WriteLine($"Registration fehlgeschlagen für User '{newUser.Username}': {responseMessage}");
                response = "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nUser with same username already registered\r\n";
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

            User user = userRepository.GetUserByUsernameAndPassword(username, password);

            // Anmeldung erfolgreich
            if (userRepository.ValidateUserCredentials(username, password))
            {
                string token = GenerateToken(username);
                HttpServer.Server.userTokens.TryAdd(token, user.Id);

                Console.WriteLine($"Anmeldung erfolgreich für User '{username}'.");
                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser login successful\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            // Anmeldung fehlgeschlagen
            else
            {
                Console.WriteLine($"Anmeldung fehlgeschlagen für User '{username}'.");
                string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username/password provided\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
        }

        // Generiere einen Token basierend auf dem Username
        private string GenerateToken(string username)
        {
            return $"{username}-mtcgToken";
        }

        private void HandlePackage(string[] requestLines)
        {
            string response;
            string requestBody = requestLines[requestLines.Length - 1].Trim();
            
            // Output der Json-Pakete
            Console.WriteLine(requestBody);

            // Typinformationen in Json "deaktiveren"
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None
            };

            JArray jsonArray = JArray.Parse(requestBody);

            foreach (JObject jsonObject in jsonArray)
            {
                // Card_Type aus Json extrahieren
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
                            break;
                    }
                }

                // Fehler beim Extrahieren des Card_Types
                else
                {
                    response = "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nCardType does not exist\r\n";
                    client.Send(Encoding.ASCII.GetBytes(response));
                    return;
                }
            }

            response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nSuccessfully created Package\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void HandleBuyAndOpenPackages(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            if (!userRepository.HasEnoughMoneyForPackage(token))
            {
                response = "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nNot enough money for buying a card package\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            List<Card> boughtCards = cardRepository.BuyAndOpenPackage(token);

            // Keine verfügbaren Karten mehr
            if (boughtCards == null || boughtCards.Count == 0)
            {
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nNo Packages left\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\nA package has been successfully bought\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void ShowAllCardsFromUser(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            User user = userRepository.GetUserByToken(token);

            if (user == null)
            {
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser not found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            List<Card> userCards = cardRepository.GetCardsByUserId(user.Id);

            string jsonResponse = JsonConvert.SerializeObject(userCards);
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void ShowDeck(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            List<Card> userDeck = cardRepository.GetDeck(token);

            if (userDeck == null || !userDeck.Any())
            {
                // Noch keine Karten fürs Deck ausgewählt
                response = "HTTP/1.1 204 OK\r\nContent-Type: text/plain\r\n\r\nThe request was fine, but the deck doesn't have any cards\r\n";
            }

            else
            {
                // Karten sind im Deck vorhanden
                string jsonResponse = JsonConvert.SerializeObject(userDeck);
                response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
            }

            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void ShowDeckInPlainText(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            List<Card> userDeck = cardRepository.GetDeck(token);

            if (userDeck == null || !userDeck.Any())
            {
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nDeck ist noch leer\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            StringBuilder deckText = new StringBuilder();

            foreach (Card card in userDeck)
            {
                deckText.AppendLine(card.ToString());
            }

            response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{deckText}\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void HandleConfigureDeck(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Daten aus Request extrahieren
            string requestBody = requestLines[requestLines.Length - 1];
            List<int> selectedCardIds = JsonConvert.DeserializeObject<List<int>>(requestBody);

            if (cardRepository.ConfigureDeck(token, selectedCardIds))
            {
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nThe deck has been successfully configured\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            else
            {
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nError in configuring deck\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
        }

        private void ShowUserData(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();

            // Username aus Request auslesen
            string userName = GetUserNameFromPath(requestLines[0]);

            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            User user = userRepository.GetUserByToken(token);

            // User nicht gefunden oder falscher Username im Pfad
            if (user == null || !user.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser not found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            string jsonResponse = JsonConvert.SerializeObject(user);
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void HandleEditUserData(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();

            // Username aus Request auslesen
            string userName = GetUserNameFromPath(requestLines[0]);

            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            User user = userRepository.GetUserByToken(token);

            // User nicht gefunden oder falscher Username im Pfad
            if (user == null || !user.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser not found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            string requestBody = GetRequestBody(requestLines);

            User updatedUser = JsonConvert.DeserializeObject<User>(requestBody);

            if (updatedUser == null)
            {
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid JSON data\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Aktualisiere die Userdaten
            user.SetUsername(updatedUser.Username);
            user.SetBio(updatedUser.Bio);
            user.SetImage(updatedUser.Image);

            // Speichere die aktualisierten Userdaten in der Datenbank
            userRepository.EditUser(user);

            response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser sucessfully updated\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        // Extrahiere Username aus Pfad, zB /users/kienboec
        private string GetUserNameFromPath(string path)
        {
            // In Segmente aufteilen ("/" als Trennzeichen)
            string[] segments = path.Split('/');
            
            // Mindestens drei Segmente, Username befindet sich bei Index 2, Leerzeichen removed
            return segments.Length >= 3 ? segments[2].Split(' ')[0].Trim() : null;
        }

        // Extrahiere JSON-Body aus den Anforderungslinien
        private string GetRequestBody(string[] requestLines)
        {
            // Index der ersten Zeile suchen
            int bodyIndex = Array.IndexOf(requestLines, requestLines.First(line => line.StartsWith("{")));

            return bodyIndex >= 0 ? string.Join(Environment.NewLine, requestLines.Skip(bodyIndex)) : null;
        }

        private void HandleViewStats(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string username = GetUsernameFromToken(token);
            string response;

            User user = userRepository.GetUserByToken(token);

            if (user != null)
            {
                string jsonResponse = JsonConvert.SerializeObject(user.PrintStats());
                response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            else
            {
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser not found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }
        }

        // Extrahiere Username aus Token
        private string GetUsernameFromToken(string token)
        {
            return token?.Split('-')[0];
        }

        private void HandleViewScoreboard(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Alle User auslesen
            List<User> allUsers = userRepository.GetAllUsers();

            if (allUsers != null && allUsers.Count > 0)
            {
                // Liste für Statistiken aller User
                List<string> userStatsJson = allUsers.Select(u => u.PrintStats()).ToList();

                string jsonResponse = string.Join(Environment.NewLine, userStatsJson);

                response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            else
            {
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nNo users found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
        }

        private void HandleStartBattle(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            battleManager.StartBattle(token);

            string battleResponse = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nBattle started\r\n";
            client.Send(Encoding.ASCII.GetBytes(battleResponse));
        }
    }
}
