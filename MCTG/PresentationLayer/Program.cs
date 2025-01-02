using MCTG.Data;

namespace MCTG.PresentationLayer
{
    internal class Program
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
                Console.WriteLine("\nShutting down server...");
                server.Stop();
            };

            // Start the server (this will run until Stop is called)
            Console.WriteLine("Starting server... Press Ctrl+C to stop.");
            server.Start();
        }
    }
}
