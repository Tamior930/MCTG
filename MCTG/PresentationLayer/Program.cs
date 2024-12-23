using MCTG.Data;

namespace MCTG.PresentationLayer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //UserRepositoryTests.TestUserRetrieval();

            DatabaseHandler dbHandler = new DatabaseHandler();
            dbHandler.InitializeDatabase();

            HttpServer server = new HttpServer();
            server.Start();
        }
    }
}
