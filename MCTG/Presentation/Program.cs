using MCTG.Data;

namespace MCTG.Presentation
{
    public class Program
    {
        static void Main(string[] args)
        {
            DatabaseHandler dbHandler = new DatabaseHandler();
            dbHandler.InitializeDatabase();

            HttpServer server = new HttpServer();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                server.Stop();
            };

            Console.WriteLine("Starting server... Press Ctrl+C to stop.");
            server.Start();
        }
    }
}
