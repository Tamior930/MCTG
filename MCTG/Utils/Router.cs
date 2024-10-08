using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCTG.Utils
{
    internal class Router
    {
        private readonly HttpParser _parser = new HttpParser();
        private readonly AuthService _authService;

        public Router(AuthService authService)
        {
            _authService = authService;
        }

        public void RouteRequest(TcpClient client)
        {
            using (var stream = client.GetStream())
            {
                var (method, endpoint, body) = _parser.ParseRequest(stream);

                if (method == "POST" && endpoint == "/register")
                {
                    var data = JsonSerializer.Deserialize<RegisterData>(body);
                    bool success = _authService.Register(data.Username, data.Password);
                    string response = success ? "User registered successfully" : "User already exists";
                    _parser.SendResponse(stream, response, success ? 200 : 400);
                }
                else if (method == "POST" && endpoint == "/login")
                {
                    var data = JsonSerializer.Deserialize<LoginData>(body);
                    string token = _authService.Login(data.Username, data.Password);
                    if (token != null)
                    {
                        _parser.SendResponse(stream, $"Token: {token}");
                    }
                    else
                    {
                        _parser.SendResponse(stream, "Invalid credentials", 401);
                    }
                }
                else
                {
                    _parser.SendResponse(stream, "Invalid endpoint", 404);
                }
            }
        }

        private class RegisterData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private class LoginData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
