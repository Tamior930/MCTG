using System.Net;
using System.Net.Sockets;
using System.Text;
using MCTG.Data.Interfaces;
using MCTG.Data.Repositories;
using MCTG.PresentationLayer.Controller;
using MCTG.PresentationLayer.Services;
using MCTG.PresentationLayer.Utils;

namespace MCTG.PresentationLayer
{
    internal class HttpServer
    {
        // Basic server components
        private readonly TcpListener _listener;
        private readonly HttpRequestParser _requestParser;
        private readonly HttpResponseParser _responseParser;

        // Controllers for handling different types of requests
        private readonly AuthController _authController;
        private readonly UserController _userController;
        private readonly BattleController _battleController;
        private readonly CardController _cardController;
        private readonly UserService _userService;
        private readonly TradingController _tradingController;

        // Thread management
        private readonly List<Thread> _clientThreads;
        private bool _isServerRunning;

        public HttpServer()
        {
            // Initialize basic components
            _listener = new TcpListener(IPAddress.Any, 10001);
            _requestParser = new HttpRequestParser();
            _responseParser = new HttpResponseParser();

            // Initialize thread management
            _clientThreads = new List<Thread>();
            _isServerRunning = false;

            // Initialize repositories
            IUserRepository userRepository = new UserRepository();
            ICardRepository cardRepository = new CardRepository();
            IDeckRepository deckRepository = new DeckRepository();
            ITradeRepository tradeRepository = new TradeRepository();

            // Initialize services
            AuthService authService = new AuthService(userRepository);
            UserService userService = new UserService(userRepository);
            CardService cardService = new CardService(userRepository, cardRepository, deckRepository);
            BattleService battleService = new BattleService(deckRepository);
            TradingService tradingService = new TradingService(tradeRepository, cardRepository, deckRepository);

            // Initialize controllers
            _authController = new AuthController(authService, userService);
            _userController = new UserController(userService, authService, cardService);
            _battleController = new BattleController(battleService, userService, cardService);
            _cardController = new CardController(cardService, userService);
            _tradingController = new TradingController(tradingService, userService);


            // Store UserService instance
            _userService = userService;
        }

        public void Start()
        {
            // Start the server
            _listener.Start();
            _isServerRunning = true;
            Console.WriteLine($"Server started on {_listener.LocalEndpoint}");

            // Main server loop
            while (_isServerRunning)
            {
                try
                {
                    // Wait for a client to connect
                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("New client connected!");

                    // Create a new thread to handle this client
                    Thread clientThread = new Thread(() => HandleClientConnection(client));
                    _clientThreads.Add(clientThread);
                    clientThread.Start();

                    // Clean up completed threads
                    CleanupCompletedThreads();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Server socket error: {ex.Message}");
                    if (!_isServerRunning) break; // Exit if server is stopping
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
                    HttpResponse response = RouteRequest(request);

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

        private HttpResponse RouteRequest(HttpRequest request)
        {
            try
            {
                // Extract auth token for all requests except login and register
                string authToken = TokenUtils.ExtractAuthToken(request);

                // Public endpoints that don't require authentication
                if ((request.Path == "/sessions" && request.Method == "POST") ||  // Login
                    (request.Path == "/users" && request.Method == "POST"))       // Register
                {
                    switch ((request.Path, request.Method))
                    {
                        case ("/users", "POST"):
                            return CreateHttpResponse(_authController.Register(request.Body));
                        case ("/sessions", "POST"):
                            return CreateHttpResponse(_authController.Login(request.Body));
                        default:
                            throw new Exception("Invalid public endpoint");
                    }
                }

                // All other endpoints require authentication
                if (string.IsNullOrEmpty(authToken))
                {
                    return new HttpResponse
                    {
                        StatusCode = 401,
                        StatusDescription = "Unauthorized",
                        Body = "Access token is missing"
                    };
                }

                // Get user and validate token
                var user = _userService.GetUserByToken(authToken);
                if (user == null || !user.HasValidToken())
                {
                    return new HttpResponse
                    {
                        StatusCode = 401,
                        StatusDescription = "Unauthorized",
                        Body = "Invalid or expired token"
                    };
                }

                // Continue with the rest of your routing logic...
                switch ((request.Path, request.Method))
                {
                    // User Management & Authentication
                    case ("/users/{username}", "GET"):
                        return CreateHttpResponse(_userController.GetUserData(authToken, GetUsernameFromPath(request.Path)));
                    case ("/users/{username}", "PUT"):
                        return CreateHttpResponse(_userController.UpdateUserData(authToken, GetUsernameFromPath(request.Path), request.Body));

                    // Package & Card Management
                    case ("/transactions/packages", "POST"):
                        return CreateHttpResponse(_cardController.AcquirePackage(authToken));
                    case ("/cards", "GET"):
                        return CreateHttpResponse(_cardController.GetUserCards(authToken));

                    // Deck Management
                    case ("/deck", "GET"):
                        return CreateHttpResponse(_cardController.GetUserDeck(authToken));
                    case ("/deck", "PUT"):
                        return CreateHttpResponse(_cardController.ConfigureDeck(authToken, request.Body));

                    // Trading
                    case ("/tradings", "GET"):
                        return CreateHttpResponse(_tradingController.GetTradingDeals(authToken));
                    case ("/tradings", "POST"):
                        return CreateHttpResponse(_tradingController.CreateTradingDeal(authToken, request.Body));
                    case ("/tradings/{tradingId}", "DELETE"):
                        return CreateHttpResponse(_tradingController.DeleteTradingDeal(authToken, GetTradingIdFromPath(request.Path)));
                    case ("/tradings/{tradingId}", "POST"):
                        return CreateHttpResponse(_tradingController.ExecuteTrade(authToken, GetTradingIdFromPath(request.Path), request.Body));

                    // Battle
                    case ("/battles", "POST"):
                        return CreateHttpResponse(_battleController.HandleBattleRequest(authToken));

                    // // Stats & Scoreboard
                    // case ("/stats", "GET"):
                    //     return CreateHttpResponse(_userController.GetUserStats(authToken));
                    // case ("/score", "GET"):
                    //     return CreateHttpResponse(_userController.GetScoreboard(authToken));

                    default:
                        return new HttpResponse
                        {
                            StatusCode = 404,
                            StatusDescription = "Not Found",
                            Body = "Endpoint not found"
                        };
                }
            }
            catch (UnauthorizedAccessException)
            {
                return new HttpResponse
                {
                    StatusCode = 401,
                    StatusDescription = "Unauthorized",
                    Body = "Access token is missing or invalid"
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = 500,
                    StatusDescription = "Internal Server Error",
                    Body = $"An error occurred: {ex.Message}"
                };
            }
        }

        private HttpResponse CreateHttpResponse(string responseString)
        {
            try
            {
                string[] parts = responseString.Split('\n', 2);
                int statusCode = int.Parse(parts[0].Split(' ')[0]);
                string statusDescription = parts[0].Split(' ')[1];
                string body = parts.Length > 1 ? parts[1] : "";

                return new HttpResponse
                {
                    StatusCode = statusCode,
                    StatusDescription = statusDescription,
                    Body = body
                };
            }
            catch (Exception)
            {
                // If parsing fails, treat it as a 500 error
                return new HttpResponse
                {
                    StatusCode = 500,
                    StatusDescription = "Internal Server Error",
                    Body = "Error processing response"
                };
            }
        }

        // Helper methods for path parameter extraction
        private string GetUsernameFromPath(string path)
        {
            var parts = path.Split('/');
            return parts.Length >= 3 ? parts[2] : "";
        }

        private string GetTradingIdFromPath(string path)
        {
            var parts = path.Split('/');
            return parts.Length >= 3 ? parts[2] : "";
        }

        private string GetQueryParam(string path, string paramName)
        {
            if (!path.Contains('?')) return "";

            var queryString = path.Split('?')[1];
            var parameters = queryString.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(p => p[0], p => p[1]);

            return parameters.TryGetValue(paramName, out var value) ? value : "";
        }
    }
}
