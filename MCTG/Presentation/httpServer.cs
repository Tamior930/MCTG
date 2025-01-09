using System.Net;
using System.Net.Sockets;
using System.Text;
using MCTG.Data.Repositories;
using MCTG.Presentation.Controllers;
using MCTG.Presentation.Routing;
using MCTG.Presentation.Services;
using MCTG.Presentation.Utils;

namespace MCTG.Presentation
{
    public class HttpServer
    {
        private readonly TcpListener _listener;
        private readonly HttpRequestParser _requestParser;
        private readonly HttpResponseParser _responseParser;
        private readonly Router _router;
        private readonly List<Thread> _clientThreads;
        private bool _isServerRunning;

        public HttpServer()
        {
            _listener = new TcpListener(IPAddress.Any, 10001);
            _requestParser = new HttpRequestParser();
            _responseParser = new HttpResponseParser();
            _clientThreads = new List<Thread>();
            _isServerRunning = false;

            // Initialize dependencies
            _router = InitializeDependencies();
        }

        private Router InitializeDependencies()
        {
            // Initialize repositories
            UserRepository userRepository = new UserRepository();
            CardRepository cardRepository = new CardRepository();
            DeckRepository deckRepository = new DeckRepository();
            TradeRepository tradeRepository = new TradeRepository();

            // Initialize services
            AuthService authService = new AuthService(userRepository);
            UserService userService = new UserService(userRepository);
            CardService cardService = new CardService(userRepository, cardRepository, deckRepository);
            BattleService battleService = new BattleService(deckRepository, userRepository, cardRepository);
            TradingService tradingService = new TradingService(tradeRepository, cardRepository, deckRepository);

            // Initialize controllers
            AuthController authController = new AuthController(authService, userService);
            UserController userController = new UserController(userService, authService, cardService);
            BattleController battleController = new BattleController(battleService, userService, cardService);
            CardController cardController = new CardController(cardService, userService);
            TradingController tradingController = new TradingController(tradingService, userService);

            return new Router(authController, userController, battleController, cardController, tradingController, userService);
        }

        public void Start()
        {
            _listener.Start();
            _isServerRunning = true;
            Console.WriteLine($"Server started on {_listener.LocalEndpoint}");

            while (_isServerRunning)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    Console.WriteLine("New client connected!");

                    var clientThread = new Thread(() => HandleClientConnection(client));
                    _clientThreads.Add(clientThread);
                    clientThread.Start();

                    CleanupCompletedThreads();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Server socket error: {ex.Message}");
                    if (!_isServerRunning) break;
                }
            }
        }

        private void HandleClientConnection(TcpClient client)
        {
            try
            {
                using (client) // This ensures the client is properly disposed
                using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                using (var reader = new StreamReader(client.GetStream()))
                {
                    // Set timeout to prevent hanging
                    client.ReceiveTimeout = 30000; // 30 seconds
                    client.SendTimeout = 30000;    // 30 seconds

                    // Process client requests
                    ProcessClientRequests(reader, writer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        private void ProcessClientRequests(StreamReader reader, StreamWriter writer)
        {
            while (_isServerRunning)
            {
                try
                {
                    // Read the incoming request
                    string rawRequest = ReadRequest(reader);
                    if (string.IsNullOrEmpty(rawRequest)) break;

                    Console.WriteLine($"\nReceived Request:\n{rawRequest}");

                    // Process the request
                    HttpRequest request = _requestParser.Parse(rawRequest);
                    HttpResponse response = _router.RouteRequest(request);

                    // Send the response
                    string rawResponse = _responseParser.CreateResponse(response);
                    Console.WriteLine($"\nSending Response:\n{rawResponse}");
                    writer.Write(rawResponse);
                }
                catch (IOException)
                {
                    // Client probably disconnected
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                    break;
                }
            }
        }

        private void CleanupCompletedThreads()
        {
            // Remove threads that have finished
            _clientThreads.RemoveAll(thread => !thread.IsAlive);
        }

        public void Stop()
        {
            // Stop accepting new connections
            _isServerRunning = false;
            _listener.Stop();

            Console.WriteLine("Shutting down server...");

            // Wait for all client threads to finish (with timeout)
            foreach (Thread thread in _clientThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Join(TimeSpan.FromSeconds(5));
                }
            }

            _clientThreads.Clear();
            Console.WriteLine("Server stopped.");
        }

        private string ReadRequest(StreamReader reader)
        {
            StringBuilder requestBuilder = new StringBuilder();

            // Set read timeout
            if (reader.BaseStream is NetworkStream networkStream)
            {
                networkStream.ReadTimeout = 30000; // 30 seconds timeout
            }

            try
            {
                string? line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    requestBuilder.AppendLine(line);
                }

                // Read body if present
                if (requestBuilder.ToString().Contains("Content-Length:"))
                {
                    int contentLength = GetContentLength(requestBuilder.ToString());
                    if (contentLength > 0)
                    {
                        char[] buffer = new char[contentLength];
                        reader.ReadBlock(buffer, 0, contentLength);
                        requestBuilder.Append(buffer);
                    }
                }

                return requestBuilder.ToString();
            }
            catch (IOException)
            {
                // Handle timeout or disconnection
                return string.Empty;
            }
        }

        private int GetContentLength(string request)
        {
            string[] lines = request.Split("\r\n");
            foreach (var line in lines)
            {
                if (line.StartsWith("Content-Length:"))
                {
                    string[] parts = line.Split(": ");

                    if (parts.Length == 2)
                    {
                        string lengthPart = parts[1];
                        int contentLength;

                        if (int.TryParse(lengthPart, out contentLength))
                        {
                            return contentLength;
                        }
                    }
                }
            }
            return 0;
        }
    }
}
