using MTCG.Database;
using MTCG.Request;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    /*
    docker exec -it MTCG psql -U postgres
    \c MTCG_DB
    \dt
    \d mtcg_users
     */

    public class UserRepository
    {
        private readonly DbHandler _dbHandler;
        private readonly Dictionary<string, string> userTokens = new Dictionary<string, string>();

        public UserRepository()
        {
            _dbHandler = new DbHandler();
        }

        public bool AddUser(User user, out string responseMessage)
        {
            // Überprüfe, ob der Benutzer bereits existiert
            if (UserExists(user.Username))
            {
                Console.WriteLine($"Benutzer mit dem Namen '{user.Username}' existiert bereits.");
                responseMessage = "User existiert bereits.";
                return false;
            }

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "INSERT INTO mtcg_users (username, password, ritopoints) VALUES (@Username, @Password, 20)";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);

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

        public void AddTokenToUser(string username, string token)
        {
            // Füge den Benutzernamen und das Token zum Dictionary hinzu
            userTokens[username] = token;
        }

        public bool ValidateToken(string token)
        {
            // Überprüfe, ob das Token in der Liste der gültigen Tokens vorhanden ist
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
                                User user = new User(reader["username"].ToString(), reader["password"].ToString());
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


        public bool HasEnoughMoneyForPackage(string token)
        {
            int initialRP = 20;
            int packageCost = 5;

            // Hole den Benutzer aus der Datenbank
            User user = GetUserByToken(token);

            // Überprüfe, ob der Benutzer genug Geld für das Paket hat
            return user != null && user.RitoPoints >= (initialRP - packageCost);
        }

        /*
        public List<Card> BuyAndOpenPackage(string token)
        {
            // Hole den Benutzer aus der Datenbank
            User user = GetUserByToken(token);

            // Annahme: packagePrice ist der Preis eines Kartenpakets
            int packagePrice = 5;
            
            // Ziehe den Preis des Kartenpakets ab
            user.RitoPoints -= packagePrice;

            // Aktualisiere den Münzstand des Benutzers in der Datenbank
            userRepository.UpdateUserCoins(username, user.Coins);

            // Erstelle und öffne das Kartenpaket
            List<Card> openedCards = OpenCardPackage();

            // Füge die geöffneten Karten dem Benutzer hinzu (z. B. in die Datenbank)
            userRepository.AddUserCards(username, openedCards);

            return openedCards;
        }
        */

        public static void TruncateTable()
        {
            DbHandler dbHandler = new DbHandler();
            using (NpgsqlConnection connection = dbHandler.GetConnection())
            {
                dbHandler.OpenConnection(connection);

                string query = "TRUNCATE TABLE mtcg_users RESTART IDENTITY";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                dbHandler.CloseConnection(connection);
            }
        }
    }
}
