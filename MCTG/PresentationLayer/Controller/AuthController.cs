using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;
using System.Text.Json;

namespace MCTG.PresentationLayer.Controller
{
    public class AuthController
    {
        private readonly AuthService _authService;


        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        public string Register(string body)
        {
            // Try to deserialize the JSON data into a dictionary
            Dictionary<string, string> parameters;
            try
            {
                parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            }
            catch (JsonException)
            {
                return CreateResponse(400, "Invalid JSON format");
            }

            // Get the username and password from the parameters
            string? username = null;
            string? password = null;

            if (parameters != null)
            {
                if (parameters.ContainsKey("Username"))
                {
                    username = parameters["Username"];
                }
                if (parameters.ContainsKey("Password"))
                {
                    password = parameters["Password"];
                }
            }

            // Check if username or password is missing
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return CreateResponse(400, "Username or password is missing");
            }

            // Try to register the user
            bool isRegistered = _authService.Register(username, password);
            if (isRegistered)
            {
                return CreateResponse(201, "Registration successful");
            }
            else
            {
                return CreateResponse(409, "User already exists");
            }
        }

        public string Login(string body)
        {
            // Try to deserialize the JSON data into a dictionary
            Dictionary<string, string> parameters;
            try
            {
                parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            }
            catch (JsonException)
            {
                return CreateResponse(400, "Invalid JSON format");
            }

            // Get the username and password from the parameters
            string? username = null;
            string? password = null;

            if (parameters != null)
            {
                if (parameters.ContainsKey("Username"))
                {
                    username = parameters["Username"];
                }
                if (parameters.ContainsKey("Password"))
                {
                    password = parameters["Password"];
                }
            }

            // Check if username or password is missing
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return CreateResponse(400, "Username or password is missing");
            }

            // Try to login the user
            Token token = _authService.Login(username, password);
            if (token != null)
            {
                return CreateResponse(200, $"Login successful. Token: {token.Value}");
            }
            else
            {
                return CreateResponse(401, "Login failed");
            }
        }

        // Helper method to create response messages
        private string CreateResponse(int statusCode, string message)
        {
            string statusDescription = GetStatusDescription(statusCode);
            return $"{statusCode} {statusDescription}\n{message}";
        }

        // Helper method to return the description for each status code
        private string GetStatusDescription(int statusCode)
        {
            switch (statusCode)
            {
                case 200:
                    return "OK";
                case 201:
                    return "Created";
                case 400:
                    return "Bad Request";
                case 401:
                    return "Unauthorized";
                case 409:
                    return "Conflict";
                default:
                    return "Unknown Error";
            }
        }
    }
}
