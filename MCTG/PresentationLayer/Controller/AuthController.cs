using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var parameters = ParseBody(body);
            string username = parameters.ContainsKey("username") ? parameters["username"] : null;
            string password = parameters.ContainsKey("password") ? parameters["password"] : null;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return CreateResponse(400, "Invalid parameters");

            bool success = _authService.Register(username, password);
            if (success)
                return CreateResponse(201, "User registered successfully");
            else
                return CreateResponse(409, "User already exists");
        }

        public string Login(string body)
        {
            var parameters = ParseBody(body);
            string username = parameters.ContainsKey("username") ? parameters["username"] : null;
            string password = parameters.ContainsKey("password") ? parameters["password"] : null;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return CreateResponse(400, "Invalid parameters");

            Token token = _authService.Login(username, password);
            if (token != null)
                return CreateResponse(200, $"Login successful. Token: {token.Value}");
            else
                return CreateResponse(401, "Invalid credentials");
        }

        private Dictionary<string, string> ParseBody(string body)
        {
            var parameters = new Dictionary<string, string>();
            var pairs = body.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                    parameters[keyValue[0]] = keyValue[1];
            }
            return parameters;
        }

        private string CreateResponse(int statusCode, string message)
        {
            return $"{statusCode} {GetStatusDescription(statusCode)}\n{message}";
        }

        private string GetStatusDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                400 => "Bad Request",
                401 => "Unauthorized",
                409 => "Conflict",
                _ => "Error"
            };
        }
    }
}
