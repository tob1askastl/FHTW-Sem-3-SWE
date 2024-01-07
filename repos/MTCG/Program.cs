using MTCG.Database;
using MTCG.Repositories;
using MTCG.Request;
using Npgsql;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpServer server = HttpServer.Server;
            server.StartServer();

            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.StopServer();
        }
    }
}