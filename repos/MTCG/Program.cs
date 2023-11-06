using MTCG.Database;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            Shop shop = new Shop();

            Champion c1 = shop.SelectRandomChampion();
            Spell s1 = shop.SelectRandomSpell();

            Console.WriteLine(c1 + "\n");
            Console.WriteLine(s1);
            */

            var dbConnector = new DbConnector();
            using (var connection = dbConnector.CreateConnection())
            {
                connection.Open();
                // Die Verbindung ist jetzt geöffnet und Sie können Datenbankoperationen durchführen.
            }

        }
    }
}