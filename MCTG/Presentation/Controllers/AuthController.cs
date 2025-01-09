using MCTG.Presentation.Services;

namespace MCTG.Presentation.Controllers
{
    public class AuthController : BaseController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService, UserService userService)
            : base(userService)
        {
            _authService = authService;
        }

        public string Register(string body)
        {
            var credentials = DeserializeBody<Dictionary<string, string>>(body, out var error);
            if (credentials == null)
                return CreateResponse(400, error);

            if (!credentials.TryGetValue("Username", out var username) ||
                !credentials.TryGetValue("Password", out var password) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                return CreateResponse(400, "Username or password is missing");
            }

            return _authService.Register(username, password)
                ? CreateResponse(201, "Registration successful")
                : CreateResponse(409, "User already exists");
        }

        public string Login(string body)
        {
            var credentials = DeserializeBody<Dictionary<string, string>>(body, out var error);
            if (credentials == null)
                return CreateResponse(400, error);

            if (!credentials.TryGetValue("Username", out var username) ||
                !credentials.TryGetValue("Password", out var password) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                return CreateResponse(400, "Username or password is missing");
            }

            var token = _authService.Login(username, password);
            return token != null
                ? CreateResponse(200, $"Login successful. Token: {token.Value}")
                : CreateResponse(401, "Login failed");
        }
    }
}
