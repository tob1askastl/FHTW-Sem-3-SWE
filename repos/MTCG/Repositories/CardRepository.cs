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


    }
}
