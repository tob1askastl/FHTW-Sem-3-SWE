using MTCG.Database;
using MTCG.Request;
using Newtonsoft.Json;
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

        // Füge einen User zur Datenbank mtcg_users hinzu
        public bool AddUser(User user, out string responseMessage)
        {
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

        public bool UserExists(string username)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = $"SELECT COUNT(*) FROM mtcg_users WHERE username = @Username";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    
                    // Anzahl der User mit jenem Usernamen
                    int userCount = Convert.ToInt32(command.ExecuteScalar());

                    return userCount > 0;
                }
            }
        }

        // Login
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

                    int userCount = Convert.ToInt32(command.ExecuteScalar());

                    return userCount > 0;
                }
            }
        }

        // Ist Token im Dictionary?
        public bool ValidateToken(string token)
        {
            return HttpServer.Server.userTokens.ContainsKey(token);
        }

        public User GetUserByToken(string token)
        {
            if (HttpServer.Server.userTokens.TryGetValue(token, out int userId))
            {
                using (NpgsqlConnection connection = _dbHandler.GetConnection())
                {
                    _dbHandler.OpenConnection(connection);

                    string query = "SELECT * FROM mtcg_users WHERE id = @userId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
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

                                return user;
                            }
                        }
                    }

                    _dbHandler.CloseConnection(connection);
                }
            }

            return null;
        }

        public User GetUserByUsernameAndPassword(string username, string password)
        {
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
                        if (reader.Read())
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

                            return user;
                        }
                    }
                }

                _dbHandler.CloseConnection(connection);
            }

            return null;
        }

        public bool HasEnoughMoneyForPackage(string token)
        {
            int packageCost = 5;

            User user = GetUserByToken(token);

            Console.WriteLine("Username: " + user.Username);
            Console.WriteLine("Amount of RP left: " + user.RitoPoints + "\n");

            return user != null && user.RitoPoints >= packageCost;
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

        // Scoreboard
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "SELECT * FROM mtcg_users ORDER BY elopoints DESC";

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

        public List<User> GetUsersWithDecks()
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                // Wähle einen User aus; basierend auf OwnerID und UserID & jene Karten, die sich in seinem Deck befinden
                string query = "SELECT u.* FROM mtcg_users u JOIN mtcg_cards c ON u.id = c.owner_id WHERE c.is_in_deck = true";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        List<User> usersWithDecks = new List<User>();

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

                            usersWithDecks.Add(user);
                        }

                        return usersWithDecks;
                    }
                }
            }
        }

        // Update Battle-Stats vom User
        public void EditStats(User user)
        {
            using (NpgsqlConnection connection = _dbHandler.GetConnection())
            {
                _dbHandler.OpenConnection(connection);

                string query = "UPDATE mtcg_users SET victories = @Victories, defeats = @Defeats, draws = @Draws, eloPoints = @EloPoints WHERE id = @UserId";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Victories", user.Victories);
                    command.Parameters.AddWithValue("@Defeats", user.Defeats);
                    command.Parameters.AddWithValue("@Draws", user.Draws);
                    command.Parameters.AddWithValue("@EloPoints", user.EloPoints);
                    command.Parameters.AddWithValue("@UserId", user.Id);

                    command.ExecuteNonQuery();
                }

                _dbHandler.CloseConnection(connection);
            }
        }
    }
}
