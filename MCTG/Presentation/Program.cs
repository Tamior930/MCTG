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

            // Attach an event handler to handle the Ctrl+C (SIGINT) signal for graceful shutdown
            Console.CancelKeyPress += (sender, e) =>
            {
                // Prevent the process from terminating immediately
                e.Cancel = true;

                // Stop the server gracefully
                server.Stop();
            };

            Console.WriteLine("Starting server... Press Ctrl+C to stop.");
            server.Start();
        }
    }
}
