using MCTG.Presentation.Controllers;
using MCTG.Presentation.Services;
using MCTG.Presentation.Utils;

namespace MCTG.Presentation.Routing
{
    public class Router
    {
        private readonly AuthController _authController;
        private readonly UserController _userController;
        private readonly BattleController _battleController;
        private readonly CardController _cardController;
        private readonly UserService _userService;
        private readonly TradingController _tradingController;

        public Router(AuthController authController,
                     UserController userController,
                     BattleController battleController,
                     CardController cardController,
                     TradingController tradingController,
                     UserService userService)
        {
            _authController = authController;
            _userController = userController;
            _battleController = battleController;
            _cardController = cardController;
            _tradingController = tradingController;
            _userService = userService;
        }

        // Main routing logic for incoming requests
        public HttpResponse RouteRequest(HttpRequest request)
        {
            try
            {
                // Check public endpoints (login/register)
                if (IsPublicEndpoint(request))
                {
                    return HandlePublicEndpoints(request);
                }

                // Verify authentication for protected endpoints
                string authToken = TokenUtils.ExtractAuthToken(request);
                if (!IsValidAuthToken(authToken, out HttpResponse? authResponse))
                {
                    return authResponse!;
                }

                // Route authenticated request
                return HandleAuthenticatedRequest(request, authToken);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }

        // Checks if endpoint requires authentication
        private bool IsPublicEndpoint(HttpRequest request)
        {
            return (request.Path == "/sessions" && request.Method == "POST") ||  // Login
                   (request.Path == "/users" && request.Method == "POST");       // Register
        }

        // Validates user authentication token
        private bool IsValidAuthToken(string authToken, out HttpResponse? response)
        {
            // Check if token exists
            if (string.IsNullOrEmpty(authToken))
            {
                response = CreateHttpResponse(401, "Unauthorized", "Access token is missing");
                return false;
            }

            // Check if token is valid
            var user = _userService.GetUserByToken(authToken);
            if (user == null || !user.HasValidToken())
            {
                response = CreateHttpResponse(401, "Unauthorized", "Invalid or expired token");
                return false;
            }

            response = null;
            return true;
        }

        // Handles requests to public endpoints
        private HttpResponse HandlePublicEndpoints(HttpRequest request)
        {
            if (request.Path == "/users" && request.Method == "POST")
            {
                return CreateHttpResponse(_authController.Register(request.Body));
            }

            if (request.Path == "/sessions" && request.Method == "POST")
            {
                return CreateHttpResponse(_authController.Login(request.Body));
            }

            return CreateHttpResponse(404, "Not Found", "Endpoint not found");
        }

        // Routes authenticated requests to appropriate controllers
        private HttpResponse HandleAuthenticatedRequest(HttpRequest request, string authToken)
        {
            // User Management
            if (request.Path.StartsWith("/users/"))
            {
                return HandleUserRequests(request, authToken);
            }

            // Card & Deck Management
            if (request.Path.StartsWith("/cards") || request.Path.StartsWith("/deck"))
            {
                return HandleCardRequests(request, authToken);
            }

            // Trading
            if (request.Path.StartsWith("/tradings"))
            {
                return HandleTradingRequests(request, authToken);
            }

            // Other endpoints
            switch (request.Path)
            {
                case "/battles" when request.Method == "POST":
                    return CreateHttpResponse(_battleController.HandleBattleRequest(authToken));

                case "/stats" when request.Method == "GET":
                    return CreateHttpResponse(_userController.GetUserStats(authToken));

                case "/score" when request.Method == "GET":
                    return CreateHttpResponse(_userController.GetScoreboard(authToken));

                default:
                    return CreateHttpResponse(404, "Not Found", "Endpoint not found");
            }
        }

        // Processes user-related requests
        private HttpResponse HandleUserRequests(HttpRequest request, string authToken)
        {
            string username = GetPathParameter(request.Path, "/users/");

            return request.Method switch
            {
                "GET" => CreateHttpResponse(_userController.GetUserData(authToken, username)),
                "PUT" => CreateHttpResponse(_userController.UpdateUserData(authToken, username, request.Body)),
                _ => CreateHttpResponse(405, "Method Not Allowed", "Invalid method for user endpoint")
            };
        }

        // Processes card and deck-related requests
        private HttpResponse HandleCardRequests(HttpRequest request, string authToken)
        {
            switch (request.Path)
            {
                case "/cards" when request.Method == "GET":
                    return CreateHttpResponse(_cardController.GetUserCards(authToken));

                case "/cards/packages" when request.Method == "POST":
                    return CreateHttpResponse(_cardController.AcquirePackage(authToken));

                case "/deck" when request.Method == "GET":
                    return CreateHttpResponse(_cardController.GetUserDeck(authToken));

                case "/deck" when request.Method == "PUT":
                    return CreateHttpResponse(_cardController.ConfigureDeck(authToken, request.Body));

                default:
                    return CreateHttpResponse(404, "Not Found", "Card endpoint not found");
            }
        }

        // Processes trading-related requests
        private HttpResponse HandleTradingRequests(HttpRequest request, string authToken)
        {
            if (request.Path == "/tradings")
            {
                return request.Method switch
                {
                    "GET" => CreateHttpResponse(_tradingController.GetTradingDeals(authToken)),
                    "POST" => CreateHttpResponse(_tradingController.CreateTradingDeal(authToken, request.Body)),
                    _ => CreateHttpResponse(405, "Method Not Allowed", "Invalid method for trading endpoint")
                };
            }

            int tradingId = int.Parse(GetPathParameter(request.Path, "/tradings/"));
            return request.Method switch
            {
                "DELETE" => CreateHttpResponse(_tradingController.DeleteTradingDeal(authToken, tradingId)),
                "POST" => CreateHttpResponse(_tradingController.ExecuteTrade(authToken, tradingId, request.Body)),
                _ => CreateHttpResponse(405, "Method Not Allowed", "Invalid method for trading endpoint")
            };
        }

        // Extracts path parameter from request URL
        private string GetPathParameter(string path, string prefix)
        {
            return path.Substring(prefix.Length);
        }

        // Creates HTTP response from response string
        private HttpResponse CreateHttpResponse(string responseString)
        {
            try
            {
                string[] parts = responseString.Split('\n', 2);
                string[] statusParts = parts[0].Split(' ', 2);

                return new HttpResponse
                {
                    StatusCode = int.Parse(statusParts[0]),
                    StatusDescription = statusParts[1],
                    Body = parts.Length > 1 ? parts[1] : ""
                };
            }
            catch
            {
                return CreateHttpResponse(500, "Internal Server Error", "Error processing response");
            }
        }

        // Creates HTTP response with status and message
        private HttpResponse CreateHttpResponse(int statusCode, string statusDescription, string body)
        {
            return new HttpResponse
            {
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                Body = body
            };
        }

        // Creates error response from exception
        private HttpResponse CreateErrorResponse(Exception ex)
        {
            if (ex is UnauthorizedAccessException)
            {
                return CreateHttpResponse(401, "Unauthorized", "Access token is missing or invalid");
            }

            return CreateHttpResponse(500, "Internal Server Error", $"An error occurred: {ex.Message}");
        }
    }
}