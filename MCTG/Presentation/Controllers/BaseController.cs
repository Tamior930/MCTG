using System.Text.Json;
using MCTG.Business.Models;
using MCTG.Presentation.Services;

namespace MCTG.Presentation.Controllers
{
    public abstract class BaseController
    {
        protected readonly UserService _userService;

        protected BaseController(UserService userService)
        {
            _userService = userService;
        }

        // Creates formatted HTTP response with status code and message
        protected string CreateResponse(int statusCode, string message)
        {
            string statusDescription = GetStatusDescription(statusCode);
            return $"{statusCode} {statusDescription}\n{message}";
        }

        // Maps HTTP status codes to descriptions
        protected string GetStatusDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Not Found",
                409 => "Conflict",
                _ => "Unknown Error"
            };
        }

        // Safely deserializes JSON request body
        protected T? DeserializeBody<T>(string body, out string error)
        {
            error = string.Empty;
            try
            {
                return JsonSerializer.Deserialize<T>(body);
            }
            catch (JsonException)
            {
                error = "Invalid JSON format";
                return default;
            }
        }

        // Serializes response data to JSON
        protected string SerializeResponse<T>(T data, bool indent = true)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = indent });
        }

        // Validates user authentication token
        protected (User? user, string response) AuthenticateUser(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return (null, CreateResponse(401, "Error: No authentication token provided."));
            }

            var user = _userService.GetUserByToken(authToken);
            if (user == null)
            {
                return (null, CreateResponse(401, "Error: User not found or invalid token."));
            }

            return (user, string.Empty);
        }
    }
}