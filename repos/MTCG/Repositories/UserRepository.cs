using MTCG.Database;
using MTCG.Request;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    public class UserRepository
    {
        private readonly DbHandler _dbHandler;

        public UserRepository()
        {
            _dbHandler = new DbHandler();
        }

        public bool AddUser(User user, out string responseMessage)
        {
            // Überprüfe, ob der Benutzer bereits existiert
            if (UserExists(user.Username))
            {
                responseMessage = "User existiert bereits.";
                return false;
            }

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "INSERT INTO mtcg_users (username, password, bio, image, ritopoints, elopoints, victories, defeats, draws) VALUES (@Username, @Password, @Bio, @Image, @RitoPoints, @EloPoints, @Victories, @Defeats, @Draws)";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@Bio", user.Bio);
                    command.Parameters.AddWithValue("@Image", user.Image);
                    command.Parameters.AddWithValue("@RitoPoints", user.RitoPoints);
                    command.Parameters.AddWithValue("@EloPoints", user.EloPoints);
                    command.Parameters.AddWithValue("@Victories", user.Victories);
                    command.Parameters.AddWithValue("@Defeats", user.Defeats);
                    command.Parameters.AddWithValue("@Draws", user.Draws);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }

            responseMessage = "User added successfully";
            return true;
        }

        public void UpdateUser(User user)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "UPDATE mtcg_users SET password = @Password WHERE username = @Username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public void EditUser(User user)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "UPDATE mtcg_users SET username = @Username, bio = @Bio, image = @Image WHERE id = @Userid";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Bio", user.Bio);
                    command.Parameters.AddWithValue("@Image", user.Image);
                    command.Parameters.AddWithValue("@Userid", user.Id);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public void DeleteUser(string username)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "DELETE FROM mtcg_users WHERE username = @Username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public bool UserExists(string username)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = $"SELECT COUNT(*) FROM mtcg_users WHERE username = @Username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    // Führe die Abfrage aus und erhalte die Anzahl der Benutzer mit dem angegebenen Benutzernamen
                    int userCount = Convert.ToInt32(command.ExecuteScalar());

                    // Wenn die Anzahl größer als 0 ist, existiert der Benutzer bereits
                    return userCount > 0;
                }
            }
        }

        // "Login"?
        public bool ValidateUserCredentials(string username, string password)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = $"SELECT COUNT(*) FROM mtcg_users WHERE username = @Username AND password = @Password";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    // Führe die Abfrage aus und erhalte die Anzahl der Benutzer mit den angegebenen Anmeldeinformationen
                    int userCount = Convert.ToInt32(command.ExecuteScalar());

                    // Wenn die Anzahl größer als 0 ist, sind die Anmeldeinformationen gültig
                    return userCount > 0;
                }
            }
        }

        public void AddTokenToUser(string token, int userId)
        {
            HttpServer.Server.userTokens[token] = userId;
        }

        // Is Token in Dictionary?
        public bool ValidateToken(string token)
        {
            // Überprüfe, ob der Token im Dictionary vorhanden ist
            return HttpServer.Server.userTokens.ContainsKey(token);
        }

        public int GetUserIdByToken(string token)
        {
            // Versuche, den UserID-Wert für den gegebenen Token abzurufen
            if (HttpServer.Server.userTokens.TryGetValue(token, out int userId))
            {
                return userId;
            }

            // Wenn der Token nicht gefunden wurde, gib -1 zurück oder wirf eine Ausnahme, je nach Bedarf
            return -1;
        }

        public User GetUserByToken(string token)
        {
            // Überprüfe, ob der Token im Dictionary vorhanden ist
            if (HttpServer.Server.userTokens.TryGetValue(token, out int userId))
            {
                // Holen Sie den Benutzer aus der Datenbank basierend auf der Benutzer-ID
                using (NpgsqlConnection connection = _dbHandler.GetConnection())
                {
                    _dbHandler.OpenConnection(connection);

                    string query = "SELECT * FROM mtcg_users WHERE id = @userId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            // Überprüfe, ob ein Datensatz gefunden wurde
                            if (reader.Read())
                            {
                                // Erstelle einen Benutzerobjekt und gib es zurück
                                User user = new User();
                                user.Id = Convert.ToInt32(reader["id"]);
                                user.Username = reader["username"].ToString();
                                user.Password = reader["password"].ToString();
                                user.Bio = reader["bio"].ToString();
                                user.Image = reader["image"].ToString();
                                user.RitoPoints = Convert.ToInt32(reader["ritopoints"]);
                                user.EloPoints = Convert.ToInt32(reader["elopoints"]);
                                user.Victories = Convert.ToInt32(reader["victories"]);
                                user.Defeats = Convert.ToInt32(reader["defeats"]);
                                user.Draws = Convert.ToInt32(reader["draws"]);

                                return user;
                            }
                        }
                    }

                    _dbHandler.CloseConnection(connection);
                }
            }

            // Wenn der Token nicht gefunden wurde oder der Benutzer nicht existiert, gib null zurück
            return null;
        }

        public User GetUserByUsernameAndPassword(string username, string password)
        {
            // Holen Sie den Benutzer aus der Datenbank basierend auf der Benutzer-ID
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT * FROM mtcg_users WHERE username = @Username AND password = @Password";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        // Überprüfe, ob ein Datensatz gefunden wurde
                        if (reader.Read())
                        {
                            // Erstelle einen Benutzerobjekt und gib es zurück
                            User user = new User();
                            user.Id = Convert.ToInt32(reader["id"]);
                            user.Username = reader["username"].ToString();
                            user.Password = reader["password"].ToString();
                            user.Bio = reader["bio"].ToString();
                            user.Image = reader["image"].ToString();
                            user.RitoPoints = Convert.ToInt32(reader["ritopoints"]);
                            user.EloPoints = Convert.ToInt32(reader["elopoints"]);
                            user.Victories = Convert.ToInt32(reader["victories"]);
                            user.Defeats = Convert.ToInt32(reader["defeats"]);
                            user.Draws = Convert.ToInt32(reader["draws"]);

                            return user;
                        }
                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            // Wenn der Token nicht gefunden wurde oder der Benutzer nicht existiert, gib null zurück
            return null;
        }


        /*
        // Add token to dictionary
        public void AddTokenToUser(string username, string token)
        {
            userTokens[username] = token;
        }

        // Is Token in Dictionary?
        public bool ValidateToken(string token)
        {
            return HttpServer.Server.TokensLoggedInUsers.Contains(token);
        }

        public User GetUserByToken(string token)
        {
            // Überprüfe, ob der Token in der Liste der eingeloggten Benutzer vorhanden ist
            if (HttpServer.Server.TokensLoggedInUsers.Contains(token))
            {
                // Extrahiere den Benutzernamen aus dem Token
                string username = token.Split('-')[0];

                // Holen Sie den Benutzer aus der Datenbank basierend auf dem Benutzernamen
                using (NpgsqlConnection connection = _dbHandler.GetConnection())
                {
                    _dbHandler.OpenConnection(connection);

                    string query = "SELECT * FROM mtcg_users WHERE username = @username";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            // Überprüfe, ob ein Datensatz gefunden wurde
                            if (reader.Read())
                            {
                                // Erstelle einen Benutzerobjekt und gib es zurück
                                User user = new User();
                                user.Id = Convert.ToInt32(reader["id"]);
                                user.Username = reader["username"].ToString();
                                user.Password = reader["password"].ToString();
                                user.Bio = reader["bio"].ToString();
                                user.Image = reader["image"].ToString();
                                user.RitoPoints = Convert.ToInt32(reader["ritopoints"]);
                                user.EloPoints = Convert.ToInt32(reader["elopoints"]);
                                user.Victories = Convert.ToInt32(reader["victories"]);
                                user.Defeats = Convert.ToInt32(reader["defeats"]);
                                user.Draws = Convert.ToInt32(reader["draws"]);

                                return user;
                            }
                        }
                    }

                    _dbHandler.CloseConnection(connection);
                }
            }

            // Wenn der Token nicht gefunden wurde oder der Benutzer nicht existiert, gib null zurück
            return null;
        }
        */

        public bool HasEnoughMoneyForPackage(string token)
        {
            int packageCost = 5;

            // Hole den Benutzer aus der Datenbank
            User user = GetUserByToken(token);

            Console.WriteLine("Username: " + user.Username);
            Console.WriteLine("Amount of RP left: " + user.RitoPoints + "\n");

            // Überprüfe, ob der Benutzer genug Geld für das Paket hat
            return user != null && user.RitoPoints >= packageCost;
        }

        public List<Card> BuyAndOpenPackage(string token)
        {
            // Überprüfe, ob genügend Karten verfügbar sind
            if (!AreEnoughCardsAvailable())
            {
                // Keine verfügbaren Karten mehr
                Console.WriteLine("Keine Karten mehr verfuegbar");
                return null; // Oder eine leere Liste oder eine andere Kennzeichnung für das Fehlschlagen
            }

            // Hole den Benutzer aus der Datenbank
            User user = GetUserByToken(token);

            // Annahme: packagePrice ist der Preis eines Kartenpakets
            int packagePrice = 5;
            
            // Ziehe den Preis des Kartenpakets ab
            user.DecreaseRitoPoints(packagePrice);

            // Aktualisiere den Münzstand des Benutzers in der Datenbank
            UpdateUserCoins(user.Username, user.RitoPoints);

            // Erstelle und öffne das Kartenpaket
            List<Card> openedCards = OpenCardPackage();

            foreach (Card card in openedCards) 
            {
                Console.WriteLine(card.Id + ", " + card.Name);
            }

            // Füge die geöffneten Karten dem Benutzer hinzu (z. B. in die Datenbank)
            AddOwnerToCard(user, openedCards);

            return openedCards;
        }

        private bool AreEnoughCardsAvailable()
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT COUNT(*) FROM mtcg_cards WHERE is_used = false";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    int availableCardCount = Convert.ToInt32(command.ExecuteScalar());

                    int requiredCardCount = 5;

                    return availableCardCount >= requiredCardCount;
                }
            }
        }

        public void UpdateUserCoins(string username, int newRitoPoints)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "UPDATE mtcg_users SET ritopoints = @newRitoPoints WHERE username = @username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@newRitoPoints", newRitoPoints);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public List<Card> OpenCardPackage()
        {
            List<Card> openedCards = new List<Card>();

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                // Eigentlich ORDER BY RANDOM() LIMIT 5
                // Aber wegen CURL 11 nicht möglich
                string query = "SELECT * FROM mtcg_cards WHERE is_used = false ORDER BY card_id LIMIT 5";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Lese die Daten für jede Karte aus der Datenbank
                        int cardID = Convert.ToInt32(reader["card_id"]);
                        string name = reader["name"].ToString();
                        ERegion region = (ERegion)Enum.Parse(typeof(ERegion), reader["region"].ToString());
                        int damage = Convert.ToInt32(reader["damage"]);
                        string cardType = reader["card_type"].ToString();
                        //bool is_used = Convert.ToBoolean(reader["is_used"]);

                        // Überprüfe, ob "owner_id" ein DBNull-Wert ist
                        int owner_id = reader["owner_id"] == DBNull.Value ? -1 : Convert.ToInt32(reader["owner_id"]);

                        // Erstelle ein Card-Objekt und setze IsUsed auf true
                        if (cardType.Equals("Champion"))
                        {
                            Champion champion = new Champion(name, region, damage, cardType);
                            champion.Id = cardID;
                            champion.IsUsed = true;
                            champion.SetOwnerID(owner_id);
                            openedCards.Add(champion);
                        }
                        else if (cardType.Equals("Spell"))
                        {
                            Spell spell = new Spell(name, region, damage, cardType);
                            spell.Id = cardID;
                            spell.IsUsed = true;
                            spell.SetOwnerID(owner_id);
                            openedCards.Add(spell);
                        }
                        // Weitere Karten-Typen können hinzugefügt werden

                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            // Aktualisiere den Status der Verwendung in der Datenbank
            UpdateCardUsageStatus(openedCards);

            return openedCards;
        }

        private void UpdateCardUsageStatus(List<Card> cards)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                // Aktualisiere den Status der Verwendung für jede Karte in der Liste
                foreach (Card card in cards)
                {
                    string query = "UPDATE mtcg_cards SET is_used = true WHERE card_id = @cardId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@cardId", card.Id);

                        command.ExecuteNonQuery();
                    }
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public void AddOwnerToCard(User user, List<Card> cards)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                foreach (Card card in cards)
                {
                    string query = "UPDATE mtcg_cards SET is_used = true, owner_id = @ownerid WHERE card_id = @cardId";

                    /*
                    Console.WriteLine("Card-ID:" + card.Id);
                    Console.WriteLine("Card-OwnerID:" + card.OwnerID);
                    Console.WriteLine("userid:" + user.Id);
                    Console.WriteLine("username:" + user.Username);
                    */

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@cardId", card.Id);
                        command.Parameters.AddWithValue("@ownerid", user.Id);

                        command.ExecuteNonQuery();
                    }
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public List<Card> GetCardsByUserId(int userId)
        {
            List<Card> userCards = new List<Card>();

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT * FROM mtcg_cards WHERE owner_id = @userId";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int cardID = Convert.ToInt32(reader["card_id"]);
                            string name = reader["name"].ToString();
                            ERegion region = (ERegion)Enum.Parse(typeof(ERegion), reader["region"].ToString());
                            int damage = Convert.ToInt32(reader["damage"]);
                            string cardType = reader["card_type"].ToString();
                            int owner_id = Convert.ToInt32(reader["owner_id"]);

                            // Erstelle ein Card-Objekt
                            Card card;

                            if (cardType.Equals("Champion"))
                            {
                                card = new Champion(name, region, damage, cardType);
                            }

                            else if (cardType.Equals("Spell"))
                            {
                                card = new Spell(name, region, damage, cardType);
                            }

                            else
                            {
                                throw new NotImplementedException();
                            }

                            // Setze die Karten-ID und füge sie zur Liste hinzu
                            card.Id = cardID;
                            card.OwnerID = owner_id;
                            card.IsUsed = true;
                            userCards.Add(card);
                        }
                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            return userCards;
        }

        public List<Card> GetDeck(string token)
        {
            User user = GetUserByToken(token);

            if (user == null)
            {
                return null;
            }

            // Wenn Karten schon als "is_in_Deck" deklariert sind, in userDeck speichern
            List<Card> userDeck = GetCardsByUserIdAndDeckStatus(user.Id, true);

            return userDeck;
        }

        public List<Card> GetCardsByUserIdAndDeckStatus(int userId, bool isInDeck)
        {
            List<Card> userCards = new List<Card>();

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT * FROM mtcg_cards WHERE owner_id = @userId AND is_in_deck = @isInDeck";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@isInDeck", isInDeck);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Lese die Daten für jede Karte aus der Datenbank
                            int cardID = Convert.ToInt32(reader["card_id"]);
                            string name = reader["name"].ToString();
                            ERegion region = (ERegion)Enum.Parse(typeof(ERegion), reader["region"].ToString());
                            int damage = Convert.ToInt32(reader["damage"]);
                            string cardType = reader["card_type"].ToString();
                            //bool isUsed = Convert.ToBoolean(reader["is_used"]);
                            //bool isInDeckValue = Convert.ToBoolean(reader["is_in_deck"]);
                            int owner_id = Convert.ToInt32(reader["owner_id"]);

                            // Erstelle ein Card-Objekt
                            Card card;

                            if (cardType.Equals("Champion"))
                            {
                                card = new Champion(name, region, damage, cardType);
                            }
                            else if (cardType.Equals("Spell"))
                            {
                                card = new Spell(name, region, damage, cardType);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }

                            // Setze die Karten-ID und füge sie zur Liste hinzu
                            card.Id = cardID;
                            card.OwnerID = owner_id;
                            card.IsUsed = true;
                            card.Is_In_Deck = true;

                            userCards.Add(card);
                        }
                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            return userCards;
        }

        public bool ConfigureDeck(string token, List<int> selectedCardIds)
        {
            // Überprüfe die Autorisierung und erhalte den Benutzer
            User user = GetUserByToken(token);

            if (user == null)
            {
                // Benutzer nicht gefunden
                Console.WriteLine("Benutzer nicht gefunden.");
                return false;
            }

            // Überprüfe, ob die Anzahl der ausgewählten Karten für das Deck korrekt ist (z.B., 4 Karten)
            if (selectedCardIds.Count != 4)
            {
                Console.WriteLine("Es müssen genau 4 Karten ausgewählt werden.");
                return false; // oder eine angemessene Fehlerbehandlung
            }

            // Überprüfe, ob alle ausgewählten Karten dem Benutzer gehören
            List<Card> userCards = GetCardsByUserIdAndDeckStatus(user.Id, false);

            foreach (int cardId in selectedCardIds)
            {
                if (userCards.All(card => card.Id != cardId))
                {
                    // Der Benutzer gehört nicht alle ausgewählten Karten
                    Console.WriteLine("Nicht alle ausgewählten Karten gehören dem Benutzer.");
                    return false;
                }
            }

            // Setze die ausgewählten Karten als Deck für den Benutzer
            if (!SetCardsInDeck(user.Id, selectedCardIds))
            {
                Console.WriteLine("Fehler beim Aktualisieren des Decks in der Datenbank.");
                return false;
            }

            Console.WriteLine("Deck erfolgreich konfiguriert.");
            return true;
        }

        private bool SetCardsInDeck(int userId, List<int> cardIds)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                // Setze die "is_in_deck"-Eigenschaft für die ausgewählten Karten auf true
                string query = $"UPDATE mtcg_cards SET is_in_deck = true WHERE owner_id = @userId AND card_id = ANY(@cardIds)";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@cardIds", cardIds.ToArray());

                    try
                    {
                        command.ExecuteNonQuery();
                        _dbHandler.CloseConnection(connection);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fehler beim Aktualisieren des Decks in der Datenbank: {ex.Message}");
                        _dbHandler.CloseConnection(connection);
                        return false;
                    }
                }
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT * FROM mtcg_users";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User();
                            user.Id = Convert.ToInt32(reader["id"]);
                            user.Username = reader["username"].ToString();
                            user.Password = reader["password"].ToString();
                            user.Bio = reader["bio"].ToString();
                            user.Image = reader["image"].ToString();
                            user.RitoPoints = Convert.ToInt32(reader["ritopoints"]);
                            user.EloPoints = Convert.ToInt32(reader["elopoints"]);
                            user.Victories = Convert.ToInt32(reader["victories"]);
                            user.Defeats = Convert.ToInt32(reader["defeats"]);
                            user.Draws = Convert.ToInt32(reader["draws"]);

                            users.Add(user);
                        }
                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            return users;
        }

    }
}
