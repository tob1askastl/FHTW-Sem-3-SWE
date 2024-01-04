using MTCG.Database;
using MTCG.Request;
using Npgsql;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Hier wird der HTTP-Server gestartet
            HttpServer server = HttpServer.Server;
            server.StartServer();

            // Endlosschleife, um das Programm aktiv zu halten
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            // Hier wird der HTTP-Server gestoppt, wenn eine Taste gedrückt wird
            server.StopServer();
        }
    }
}

            /*
            string[] champlines = File.ReadAllLines("D:/FH/FHTW-Sem-3-SWE/repos/MTCG/bin/Debug/net7.0/champions.txt");
            string[] spelllines = File.ReadAllLines("D:/FH/FHTW-Sem-3-SWE/repos/MTCG/bin/Debug/net7.0/spells.txt");

            var dbConnector = new DbConnector();
            using (var connection = dbConnector.CreateConnection())
            {
                connection.Open();
                // Die Verbindung ist jetzt geöffnet und Sie können Datenbankoperationen durchführen.

                // Daten in die Datenbank einfügen
                foreach (string line in champlines)
                {
                    string[] values = line.Split(';');

                    string name = values[0];
                    string description = values[1];
                    int region = int.Parse(values[2]);
                    int hP = int.Parse(values[3]);

                    try
                    {
                        using (var command = new NpgsqlCommand("INSERT INTO mtcg_champions (name, description, region, healthpoints) VALUES (@Name, @Description, @Region, @HP)", connection))
                        {
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@Region", region);
                            command.Parameters.AddWithValue("@HP", hP);

                            command.ExecuteNonQuery();
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fehler beim Einfügen von Daten in die Tabelle mtcg_champions: {ex.Message}");
                    }
                }

                foreach (string line in spelllines)
                {
                    string[] values = line.Split(';');

                    string name = values[0];
                    string description = values[1];
                    int region = int.Parse(values[2]);
                    int manaCost = int.Parse(values[3]);

                    try
                    {
                        using (var command = new NpgsqlCommand("INSERT INTO mtcg_spells (name, description, region, manacost) VALUES (@Name, @Description, @Region, @ManaCost)", connection))
                        {
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@Region", region);
                            command.Parameters.AddWithValue("@ManaCost", manaCost);

                            command.ExecuteNonQuery();
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fehler beim Einfügen von Daten in die Tabelle mtcg_spells: {ex.Message}");
                    }
                }

                // Zufälligen Champion auswählen
                Champion randomChampion = SelectRandomChampion(connection);
                Console.WriteLine("champ:" + randomChampion + "\n");

                // Zufälligen Spell auswählen
                Spell randomSpell = SelectRandomSpell(connection);
                Console.WriteLine("spell:" + randomSpell);
            }            
        }

            static Champion SelectRandomChampion(NpgsqlConnection connection)
        {
            using (var command = new NpgsqlCommand("SELECT * FROM mtcg_champions ORDER BY RANDOM() LIMIT 1", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return CreateChampionFromReader(reader);
                    }
                }
            }

            return null;
        }

        static Spell SelectRandomSpell(NpgsqlConnection connection)
        {
            using (var command = new NpgsqlCommand("SELECT * FROM mtcg_spells ORDER BY RANDOM() LIMIT 1", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return CreateSpellFromReader(reader);
                    }
                }
            }

            return null;
        }

        static Champion CreateChampionFromReader(NpgsqlDataReader reader)
        {
            return new Champion(
                reader["name"].ToString(),
                reader["description"].ToString(),
                (ERegion)Enum.Parse(typeof(ERegion), reader["region"].ToString()),
                Convert.ToInt32(reader["healthpoints"])
            );
        }

        static Spell CreateSpellFromReader(NpgsqlDataReader reader)
        {
            return new Spell(
                reader["name"].ToString(),
                reader["description"].ToString(),
                (ERegion)Enum.Parse(typeof(ERegion), reader["region"].ToString()), 
                Convert.ToInt32(reader["manacost"])
            );
        }
    }
}
            */