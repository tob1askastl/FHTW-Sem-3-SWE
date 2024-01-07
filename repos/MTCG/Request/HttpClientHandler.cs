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

            // Curl 1 Registration
            if (method == "POST" && path == "/users")
            {
                HandleRegistration(requestLines);
            }

            // Curl 2 Login
            else if (method == "POST" && path == "/sessions")
            {
                HandleLogin(requestLines);
            }

            // CURL 3 Create Packages
            else if (method == "POST" && path == "/packages")
            {
                HandlePackage(requestLines);
            }

            // CURL 4 - 7 Buy Packages / Not enough Money / No more cards
            else if (method == "POST" && path == "/transactions/packages")
            {
                HandleBuyAndOpenPackages(requestLines);
            }

            // CURL 8, 9 View all Cards from user
            else if (method == "GET" && path == "/cards")
            {
                ShowAllCardsFromUser(requestLines);
            }

            // CURL 11, 12 Configure Deck and Show
            else if (method == "PUT" && path == "/deck")
            {
                HandleConfigureDeck(requestLines);
            }

            // CURL 10, 13 Different Outputstyle
            else if (method == "GET" && path.StartsWith("/deck"))
            {
                // Überprüfe, ob der Query-Parameter ?format=plain vorhanden ist
                bool isPlainText = path.Contains("format=plain");

                if (isPlainText)
                {
                    ShowDeckInPlainText(requestLines);
                }

                else
                {
                    ShowDeck(requestLines);
                }
            }

            // CURL 14 Show User Data
            else if (method == "GET" && path.StartsWith("/users/"))
            {
                ShowUserData(requestLines);
            }

            // CURL 14 Edit User Data
            else if (method == "PUT" && path.StartsWith("/users/"))
            {
                HandleEditUserData(requestLines);
            }


            // CURL 15 Stats (single User info)
            else if (method == "GET" && path == "/stats")
            {
                HandleViewStats(requestLines);
            }

            // CURL 16 Scoreboard (all users info)
            else if (method == "GET" && path == "/scoreboard")
            {
                HandleViewScoreboard(requestLines);
            }

            // CURL 17 Battle
            else if (method == "POST" && path == "/battles")
            {
                string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
                string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();

                // Überprüfe die Autorisierung und hole den Benutzer aus der Datenbank
                if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
                {
                    // Autorisierung fehlgeschlagen
                    string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                    client.Send(Encoding.ASCII.GetBytes(response));
                    return;
                }

                // Der Spieler startet den Kampf
                battleManager.StartBattle(token);

                // Hier kannst du eine Bestätigung oder andere Logik hinzufügen
                string battleResponse = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nBattle started\r\n";
                client.Send(Encoding.ASCII.GetBytes(battleResponse));
            }

            client.Close();
        }

        private void HandleRegistration(string[] requestLines)
        {
            string requestBody = requestLines[requestLines.Length - 1].Trim();

            JsonUserData userData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonUserData>(requestBody);

            string response;
            User newUser = new User();

            newUser.Username = userData.Username;
            newUser.Password = userData.Password;

            // Registration erfolgreich
            if (userRepository.AddUser(newUser, out string responseMessage))
            {
                Console.WriteLine($"Registration erfolgreich für Benutzer '{newUser.Username}'.");

                response = "HTTP/1.1 201 OK\r\nContent-Type: text/plain\r\n\r\nUser successfully created\r\n";
            }

            // Registration fehlgeschlagen
            else
            {
                Console.WriteLine($"Registration fehlgeschlagen für Benutzer '{newUser.Username}': {responseMessage}");

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

                Console.WriteLine($"Anmeldung erfolgreich für Benutzer '{username}'.");
                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser login successful\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            // Anmeldung fehlgeschlagen
            else
            {
                Console.WriteLine($"Anmeldung fehlgeschlagen für Benutzer '{username}'.");
                string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username/password provided\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            /*
            foreach (string usertoken in HttpServer.Server.TokensLoggedInUsers)
            {
                Console.WriteLine(usertoken);
            }
            */
        }

        private string GenerateToken(string username)
        {
            return $"{username}-mtcgToken";
        }

        private void HandlePackage(string[] requestLines)
        {
            string response;
            string requestBody = requestLines[requestLines.Length - 1].Trim();
            Console.WriteLine(requestBody);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None
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
                            break;
                    }
                }

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
            // Überprüfe die Autorisierung
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Überprüfe, ob der Benutzer genügend Geld hat
            if (!userRepository.HasEnoughMoneyForPackage(token))
            {
                // Benutzer hat nicht genug Geld
                response = "HTTP/1.1 403 Forbidden\r\nContent-Type: text/plain\r\n\r\nNot enough money for buying a card package\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Kaufe und öffne das Paket
            List<Card> boughtCards = cardRepository.BuyAndOpenPackage(token);

            // Überprüfe, ob Karten gekauft und geöffnet wurden
            if (boughtCards == null || boughtCards.Count == 0)
            {
                // Keine verfügbaren Karten mehr
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nNo Packages left\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Handle die Rückgabe (z.B., sende eine Liste der erhaltenen Karten als JSON)
            //string jsonResponse = JsonConvert.SerializeObject(boughtCards);
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\nA package has been successfully bought\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void ShowAllCardsFromUser(string[] requestLines)
        {
            // Überprüfe die Autorisierung
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole den Benutzer aus der Datenbank
            User user = userRepository.GetUserByToken(token);

            if (user == null)
            {
                // Benutzer nicht gefunden
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser not found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole alle Karten des Benutzers aus der Datenbank
            List<Card> userCards = cardRepository.GetCardsByUserId(user.Id);

            // Handle die Rückgabe (z.B., sende die Liste der Karten als JSON)
            string jsonResponse = JsonConvert.SerializeObject(userCards);
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private void ShowDeck(string[] requestLines)
        {
            // Überprüfe die Autorisierung
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole die Karten des Benutzers für das Deck aus der Datenbank
            List<Card> userDeck = cardRepository.GetDeck(token);

            // Handle die Rückgabe basierend auf dem Zustand des Decks
            if (userDeck == null || !userDeck.Any())
            {
                // Keine Karten ausgewählt
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
            // Überprüfe die Autorisierung
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole die Karten des Benutzers für das Deck aus der Datenbank
            List<Card> userDeck = cardRepository.GetDeck(token);

            // Handle die Rückgabe basierend auf dem Zustand des Decks
            if (userDeck == null || !userDeck.Any())
            {
                // Keine Karten ausgewählt
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nDeck ist noch leer\r\n";
            }

            // Erstelle den Text für jedes Kartenobjekt
            StringBuilder deckText = new StringBuilder();

            foreach (Card card in userDeck)
            {
                deckText.AppendLine(card.ToString());
            }

            // Sende den erstellten Text als Antwort
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

            // Extrahiere die ausgewählten Karten aus dem Anfragekörper
            string requestBody = requestLines[requestLines.Length - 1];
            List<int> selectedCardIds = JsonConvert.DeserializeObject<List<int>>(requestBody);

            // Konfiguriere das Deck des Benutzers und handle die Rückgabe
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
            // Überprüfe die Autorisierung und hole den Benutzernamen aus dem Pfad
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string userName = GetUserNameFromPath(requestLines[0]);

            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            Console.WriteLine(userName);

            // Hole den Benutzer aus der Datenbank
            User user = userRepository.GetUserByToken(token);

            if (user == null || !user.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                // Benutzer nicht gefunden oder falscher Benutzername im Pfad
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
            // Überprüfe die Autorisierung und hole den Benutzernamen aus dem Pfad
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string userName = GetUserNameFromPath(requestLines[0]);

            string response;

            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole den Benutzer aus der Datenbank
            User user = userRepository.GetUserByToken(token);

            if (user == null || !user.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                // Benutzer nicht gefunden oder falscher Benutzername im Pfad
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nUser not found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole den JSON-Body der Anfrage und deserialisiere ihn in ein User-Objekt
            string requestBody = GetRequestBody(requestLines);

            User updatedUser = JsonConvert.DeserializeObject<User>(requestBody);

            if (updatedUser == null)
            {
                // Fehler beim Deserialisieren des JSON-Bodys
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nInvalid JSON data\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Aktualisiere die Benutzerdaten
            user.SetUsername(updatedUser.Username);
            user.SetBio(updatedUser.Bio);
            user.SetImage(updatedUser.Image);

            // Speichere die aktualisierten Benutzerdaten in der Datenbank
            userRepository.EditUser(user);

            // Sende die Bestätigung als Antwort
            response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser sucessfully updated\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }

        private string GetUserNameFromPath(string path)
        {
            // Extrahiere den Benutzernamen aus dem Pfad, z.B., /users/kienboec
            string[] segments = path.Split('/');
            
            return segments.Length >= 3 ? segments[2].Split(' ')[0].Trim() : null;
        }

        private string GetRequestBody(string[] requestLines)
        {
            // Extrahiere den JSON-Body aus den Anforderungslinien
            int bodyIndex = Array.IndexOf(requestLines, requestLines.First(line => line.StartsWith("{")));
            return bodyIndex >= 0 ? string.Join(Environment.NewLine, requestLines.Skip(bodyIndex)) : null;
        }

        private void HandleViewStats(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string username = GetUsernameFromToken(token);
            string response;

            // Hole den Benutzer aus der Datenbank
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

        private string GetUsernameFromToken(string token)
        {
            // Extrahiere den Benutzernamen aus dem Token
            return token?.Split('-')[0];
        }

        private void HandleViewScoreboard(string[] requestLines)
        {
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            // Überprüfe die Autorisierung und hole den Benutzer aus der Datenbank
            if (string.IsNullOrEmpty(token) || !userRepository.ValidateToken(token))
            {
                // Autorisierung fehlgeschlagen
                response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nUnauthorized\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Hole alle Benutzer aus der Datenbank
            List<User> allUsers = userRepository.GetAllUsers();

            if (allUsers != null && allUsers.Count > 0)
            {
                // Erstelle eine Liste von JSON-Strings für die Statistiken aller Benutzer
                List<string> userStatsJson = allUsers.Select(u => u.PrintStats()).ToList();

                // Kombiniere die JSON-Strings zu einer einzelnen Antwort
                string jsonResponse = string.Join(Environment.NewLine, userStatsJson);

                // Sende die Antwort
                response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{jsonResponse}\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
            else
            {
                // Keine Benutzer gefunden
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nNo users found\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
        }
    }
}
