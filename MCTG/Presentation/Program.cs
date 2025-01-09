using MCTG.Data;

namespace MCTG.Presentation
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Initialize database
            DatabaseHandler dbHandler = new DatabaseHandler();
            dbHandler.InitializeDatabase();

            // Create and start the server
            HttpServer server = new HttpServer();

            // Setup graceful shutdown on Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                server.Stop();
            };

            // Start the server (this will run until Stop is called)
            Console.WriteLine("Starting server... Press Ctrl+C to stop.");
            server.Start();
        }
    }
}
