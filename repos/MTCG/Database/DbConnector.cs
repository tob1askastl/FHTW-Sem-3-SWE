using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data.SqlClient;

namespace MTCG.Database
{
    public class DbConnector
    {
        private const string HOST = "localhost";
        private const int PORT = 5432;
        private const string DBNAME = "MTCG_DB";
        private const string USERNAME = "postgres";
        private const string PASSWORD = "MTCG";
        
        // Neue Verbindung zur Postegre-Datenbank erstellen
        public NpgsqlConnection CreateConnection()
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = HOST,
                Port = PORT,
                Database = DBNAME,
                Username = USERNAME,
                Password = PASSWORD
            };

            NpgsqlConnection connection = new NpgsqlConnection(builder.ToString());

            return connection;
        }
    }
}
