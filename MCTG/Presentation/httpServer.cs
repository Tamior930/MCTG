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
            // Initialize the TCP listener to listen on any IP address and port 10001
            _listener = new TcpListener(IPAddress.Any, 10001);

            // Initialize the request and response parsers
            _requestParser = new HttpRequestParser();
            _responseParser = new HttpResponseParser();

            // Initialize the list to keep track of client threads
            _clientThreads = new List<Thread>();

            // Set the server running flag to false initially
            _isServerRunning = false;

            // Initialize all dependencies and set up the router
            _router = InitializeDependencies();
        }

        // Initializes all dependencies (repositories, services, controllers)
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
            BattleController battleController = new BattleController(battleService, userService);
            CardController cardController = new CardController(cardService, userService);
            TradingController tradingController = new TradingController(tradingService, userService);

            // Return a new Router instance with all the initialized controllers and services
            return new Router(authController, userController, battleController, cardController, tradingController, userService);
        }

        // Starts the server and begins accepting client connections
        public void Start()
        {
            // Start the TCP listener
            _listener.Start();

            // Set the server running flag to true
            _isServerRunning = true;
            Console.WriteLine($"Server started on {_listener.LocalEndpoint}");

            // Continuously accept new client connections while the server is running
            while (_isServerRunning)
            {
                try
                {
                    // Accept a new client connection
                    var client = _listener.AcceptTcpClient();
                    Console.WriteLine("New client connected!");

                    // Create and start a new thread to handle the client connection
                    var clientThread = new Thread(() => HandleClientConnection(client));
                    _clientThreads.Add(clientThread);
                    clientThread.Start();

                    // Remove references to completed threads
                    CleanupCompletedThreads();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Server socket error: {ex.Message}");
                    if (!_isServerRunning) break;
                }
            }
        }

        // Handles individual client connection and communication
        private void HandleClientConnection(TcpClient client)
        {
            try
            {
                using (client)
                using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                using (var reader = new StreamReader(client.GetStream()))
                {
                    // Set timeouts to prevent hanging connections
                    client.ReceiveTimeout = 30000;
                    client.SendTimeout = 30000;

                    // Process client requests
                    ProcessClientRequests(reader, writer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        // Processes requests from connected client
        private void ProcessClientRequests(StreamReader reader, StreamWriter writer)
        {
            while (_isServerRunning)
            {
                try
                {
                    // Read and validate the request from the client
                    string rawRequest = ReadRequest(reader);
                    if (string.IsNullOrEmpty(rawRequest)) break;

                    Console.WriteLine($"\nReceived Request:\n{rawRequest}");

                    // Parse the request and route it to the appropriate handler
                    HttpRequest request = _requestParser.Parse(rawRequest);
                    HttpResponse response = _router.RouteRequest(request);

                    // Send the response back to the client
                    string rawResponse = _responseParser.CreateResponse(response);
                    Console.WriteLine($"\nSending Response:\n{rawResponse}");
                    writer.Write(rawResponse);
                }
                catch (IOException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                    break;
                }
            }
        }

        // Removes completed client threads from tracking list
        private void CleanupCompletedThreads()
        {
            // Remove all threads that are no longer alive from the list
            _clientThreads.RemoveAll(thread => !thread.IsAlive);
        }

        // Stops the server and cleans up resources
        public void Stop()
        {
            // Set the server running flag to false
            _isServerRunning = false;

            // Stop the TCP listener
            _listener.Stop();

            Console.WriteLine("Shutting down server...");

            // Wait for all client threads to complete
            foreach (Thread thread in _clientThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Join(TimeSpan.FromSeconds(5));
                }
            }

            // Clear the list of client threads
            _clientThreads.Clear();
            Console.WriteLine("Server stopped.");
        }

        // Reads complete HTTP request from client
        private string ReadRequest(StreamReader reader)
        {
            StringBuilder requestBuilder = new StringBuilder();

            // Set read timeout for network stream
            if (reader.BaseStream is NetworkStream networkStream)
            {
                networkStream.ReadTimeout = 30000;
            }

            try
            {
                // Read headers
                string? line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    requestBuilder.AppendLine(line);
                }

                // Read body if Content-Length is present
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
                return string.Empty;
            }
        }

        // Extracts Content-Length value from request headers
        private int GetContentLength(string request)
        {
            // Split the request into lines
            string[] lines = request.Split("\r\n");

            // Iterate through each line to find the Content-Length header
            foreach (var line in lines)
            {
                if (line.StartsWith("Content-Length:"))
                {
                    string[] parts = line.Split(": ");

                    if (parts.Length == 2)
                    {
                        string lengthPart = parts[1];
                        int contentLength;

                        // Try to parse the Content-Length value
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
