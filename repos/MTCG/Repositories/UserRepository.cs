using MTCG.Database;
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

        public void AddUser(User user)
        {
            // Überprüfe, ob der Benutzer bereits existiert
            if (UserExists(user.Username))
            {
                Console.WriteLine($"Benutzer mit dem Namen '{user.Username}' existiert bereits.");
                return;
            }

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "INSERT INTO mtcg_users (username, password) VALUES (@Username, @Password)";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
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
