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
    public class UserRepository
    {
        private readonly DbHandler _dbHandler;

        public UserRepository()
        {
            _dbHandler = new DbHandler();
            TruncateTable();
        }

        public void AddUser(User user)
        {
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

        public void TruncateTable()
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "TRUNCATE TABLE mtcg_users RESTART IDENTITY";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }
    }
}
