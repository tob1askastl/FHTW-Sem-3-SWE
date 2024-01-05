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

        // Weitere Methoden zum Arbeiten mit Karten (Champions und Spells)

        public void AddCard(Card card)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "INSERT INTO mtcg_cards (name, region, damage, card_type) " +
                               "VALUES (@name, @region, @damage, @cardType)";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", card.Name);

                    command.Parameters.AddWithValue("@region", (int)card.Region);

                    command.Parameters.AddWithValue("@damage", card.Damage);

                    command.Parameters.AddWithValue("@cardType", GetCardType(card));

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }

        private string GetCardType(Card card)
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


        public void UpdateCard(Card card)
        {
            // Implementiere die Logik zum Aktualisieren einer Karte in der Datenbank
        }

        public void DeleteCard(int cardId)
        {
            // Implementiere die Logik zum Löschen einer Karte aus der Datenbank
        }

        public Card GetCardById(int cardId)
        {
            // Implementiere die Logik zum Abrufen einer Karte aus der Datenbank anhand der ID
            // Beachte, dass dies eine allgemeine Methode für Karten ist und die spezifische Klasse (Champion oder Spell) im Rückgabetyp stehen könnte.
            return null;
        }

        public static void TruncateTable()
        {
            DbHandler dbHandler = new DbHandler();
            using (NpgsqlConnection connection = dbHandler.GetConnection())
            {
                dbHandler.OpenConnection(connection);

                string query = "TRUNCATE TABLE mtcg_cards RESTART IDENTITY";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                dbHandler.CloseConnection(connection);
            }
        }
    }
}
