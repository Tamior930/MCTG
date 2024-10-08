namespace MCTG.PresentationLayer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            httpServer server = new httpServer();
            Console.WriteLine("Starting server...");
            server.Start();
        }
    }
}
