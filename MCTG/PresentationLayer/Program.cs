namespace MCTG.PresentationLayer
{
    internal class Program
    {
        static void main()
        {
            httpServer server = new httpServer();
            Console.WriteLine("Starting server...");
            server.Start();
        }
    }
}
