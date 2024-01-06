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

            // CURL 14 Edit User Data
            else if (method == "GET" && path.StartsWith("/users/"))
            {
                ShowUserData(requestLines);
            }
            else if (method == "PUT" && path.StartsWith("/users/"))
            {
                HandleEditUserData(requestLines);
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
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nRegistration successful\r\n";
            }

            // Registration fehlgeschlagen
            else
            {
                Console.WriteLine($"Registration fehlgeschlagen für Benutzer '{username}': {responseMessage}");

                response = "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\n" + responseMessage + "\r\n";
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
                string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nLogin successful\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }

            // Anmeldung fehlgeschlagen
            else
            {
                Console.WriteLine($"Anmeldung fehlgeschlagen für Benutzer '{username}'.");
                string response = "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nLogin failed\r\n";
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

            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nSuccessfully created Package\r\n";
            client.Send(Encoding.ASCII.GetBytes(response));
        }


        private void HandleBuyAndOpenPackages(string[] requestLines)
        {
            // Überprüfe die Autorisierung
            string authorizationHeader = requestLines.FirstOrDefault(line => line.StartsWith("Authorization:"));
            string token = authorizationHeader?.Replace("Authorization: Bearer ", "").Trim();
            string response;

            //Console.WriteLine(token);

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
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nNicht genuegend RitoPunkte\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Kaufe und öffne das Paket
            List<Card> boughtCards = userRepository.BuyAndOpenPackage(token);

            // Überprüfe, ob Karten gekauft und geöffnet wurden
            if (boughtCards == null || boughtCards.Count == 0)
            {
                // Keine verfügbaren Karten mehr
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nKeine Karten mehr uebrig\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
                return;
            }

            // Handle die Rückgabe (z.B., sende eine Liste der erhaltenen Karten als JSON)
            //string jsonResponse = JsonConvert.SerializeObject(boughtCards);
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\nGekauft und erhalten\r\n";
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
            List<Card> userCards = userRepository.GetCardsByUserId(user.Id);

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
            List<Card> userDeck = userRepository.GetDeck(token);

            // Handle die Rückgabe basierend auf dem Zustand des Decks
            if (userDeck == null || !userDeck.Any())
            {
                // Keine Karten ausgewählt
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nDeck ist noch leer\r\n";
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
            List<Card> userDeck = userRepository.GetDeck(token);

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
            if (userRepository.ConfigureDeck(token, selectedCardIds))
            {
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nDeck erfolgreich konfiguriert\r\n";
                client.Send(Encoding.ASCII.GetBytes(response));
            }
            else
            {
                response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/plain\r\n\r\nFehler beim Konfigurieren des Decks\r\n";
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
            Console.WriteLine("requestline[0]" + requestLines[0]);
            Console.WriteLine("requestline[1]" + requestLines[1]);
            Console.WriteLine("requestline[2]" + requestLines[2]);
            Console.WriteLine("requestline[3]" + requestLines[3]);
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

            // Handle die Rückgabe (z.B., sende die Benutzerdaten als JSON)
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

            // HELP

            Console.WriteLine(updatedUser.Username);
            Console.WriteLine(updatedUser.Bio);
            Console.WriteLine(updatedUser.Image);

            // Aktualisiere die Benutzerdaten
            user.EditUsername(updatedUser.Username);
            user.EditBio(updatedUser.Bio);
            user.EditImage(updatedUser.Image);

            // Speichere die aktualisierten Benutzerdaten in der Datenbank
            userRepository.EditUser(user);

            // Sende die Bestätigung als Antwort
            response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser data updated successfully\r\n";
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

    }
}
