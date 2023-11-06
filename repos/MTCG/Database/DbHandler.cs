using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG.Database
{
    public class DbHandler
    {
        private readonly string _connectionString;

        public DbHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public void OpenConnection(NpgsqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        public void CloseConnection(NpgsqlConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public NpgsqlDataReader ExecuteQuery(string query)
        {
            using (var connection = GetConnection())
            {
                OpenConnection(connection);
                using (var command = new NpgsqlCommand(query, connection))
                {
                    return command.ExecuteReader();
                }
            }
        }
    }
}
