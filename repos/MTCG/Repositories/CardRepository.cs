using MTCG.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    public class CardRepository
    {
        private readonly DbHandler _dbHandler;

        public CardRepository()
        {
            _dbHandler = new DbHandler();
        }

        public void AddCard(Card card)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "INSERT INTO mtcg_cards (name, region, damage, card_type, is_used, owner_id) VALUES (@name, @region, @damage, @cardType, false, @ownerID) RETURNING card_id";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", card.Name);
                    command.Parameters.AddWithValue("@region", (int)card.Region);
                    command.Parameters.AddWithValue("@damage", card.Damage);
                    command.Parameters.AddWithValue("@cardType", GetCardType(card));
                    command.Parameters.AddWithValue("@ownerID", DBNull.Value);

                    // ExecuteScalar gibt die generierte ID zurück
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int generatedId))
                    {
                        card.Id = generatedId;
                    }
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        public string GetCardType(Card card)
        {
            if (card is Champion)
            {
                return "Champion";
            }

            else if (card is Spell)
            {
                return "Spell";
            }

            else
            {
                return "Unknown";
            }
        }

        public List<Card> GetCardsInDeckForUser(int userId)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT * FROM mtcg_cards WHERE owner_id = @userId AND is_in_deck = true";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        List<Card> cardsInDeck = new List<Card>();

                        while (reader.Read())
                        {
                            string cardType = reader["card_type"].ToString();

                            if (cardType.Equals("Champion", StringComparison.OrdinalIgnoreCase))
                            {
                                Champion champion = new Champion();
                                champion.Id = Convert.ToInt32(reader["card_id"]);
                                champion.Name = reader["name"].ToString();
                                champion.Region = (ERegion)Convert.ToInt32(reader["region"]);
                                champion.Damage = Convert.ToInt32(reader["damage"]);
                                champion.IsUsed = Convert.ToBoolean(reader["is_used"]);
                                champion.OwnerID = Convert.ToInt32(reader["owner_id"]);
                                champion.Is_In_Deck = Convert.ToBoolean(reader["is_in_deck"]);

                                cardsInDeck.Add(champion);
                            }

                            else if (cardType.Equals("Spell", StringComparison.OrdinalIgnoreCase))
                            {
                                Spell spell = new Spell();
                                spell.Id = Convert.ToInt32(reader["card_id"]);
                                spell.Name = reader["name"].ToString();
                                spell.Region = (ERegion)Convert.ToInt32(reader["region"]);
                                spell.Damage = Convert.ToInt32(reader["damage"]);
                                spell.IsUsed = Convert.ToBoolean(reader["is_used"]);
                                spell.OwnerID = Convert.ToInt32(reader["owner_id"]);
                                spell.Is_In_Deck = Convert.ToBoolean(reader["is_in_deck"]);

                                cardsInDeck.Add(spell);
                            }
                        }

                        return cardsInDeck;
                    }
                }
            }
        }

        public bool AreEnoughCardsAvailable()
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

        public List<Card> BuyAndOpenPackage(string token)
        {
            // Überprüfe, ob genügend Karten verfügbar sind
            if (!AreEnoughCardsAvailable())
            {
                // Keine verfügbaren Karten mehr
                Console.WriteLine("Keine Karten mehr verfuegbar");
                return null; // Oder eine leere Liste oder eine andere Kennzeichnung für das Fehlschlagen
            }

            UserRepository userRepository = new UserRepository();

            // Hole den Benutzer aus der Datenbank
            User user = userRepository.GetUserByToken(token);

            // Annahme: packagePrice ist der Preis eines Kartenpakets
            int packagePrice = 5;

            // Ziehe den Preis des Kartenpakets ab
            user.DecreaseRitoPoints(packagePrice);

            // Aktualisiere den Münzstand des Benutzers in der Datenbank
            userRepository.UpdateUserCoins(user.Username, user.RitoPoints);

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
                            Champion champion = new Champion();
                            champion.Name = name;
                            champion.Region = region;
                            champion.Damage = damage;
                            champion.CardType = cardType;
                            champion.Id = cardID;
                            champion.IsUsed = true;
                            champion.SetOwnerID(owner_id);
                            openedCards.Add(champion);
                        }

                        else if (cardType.Equals("Spell"))
                        {
                            Spell spell = new Spell();
                            spell.Name = name;
                            spell.Region = region;
                            spell.Damage = damage;
                            spell.CardType = cardType;
                            spell.Id = cardID;
                            spell.IsUsed = true;
                            spell.SetOwnerID(owner_id);

                            openedCards.Add(spell);
                        }
                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            // Aktualisiere den Status der Verwendung in der Datenbank
            UpdateCardUsageStatus(openedCards);

            return openedCards;
        }

        public void UpdateCardUsageStatus(List<Card> cards)
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
                                card = new Champion();
                            }

                            else if (cardType.Equals("Spell"))
                            {
                                card = new Spell();
                            }

                            else
                            {
                                throw new NotImplementedException();
                            }

                            card.Name = name;
                            card.Region = region;
                            card.Damage = damage;
                            card.CardType = cardType;
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
            UserRepository userRepository = new UserRepository();

            User user = userRepository.GetUserByToken(token);

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
                                card = new Champion();
                                card.Name = name;
                                card.Region = region;
                                card.Damage = damage;
                                card.CardType = cardType;
                            }

                            else if (cardType.Equals("Spell"))
                            {
                                card = new Spell();
                                card.Name = name;
                                card.Region = region;
                                card.Damage = damage;
                                card.CardType = cardType;
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
            UserRepository userRepository = new UserRepository();

            // Überprüfe die Autorisierung und erhalte den Benutzer
            User user = userRepository.GetUserByToken(token);

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

        public bool SetCardsInDeck(int userId, List<int> cardIds)
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
    }
}
