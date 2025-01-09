using MCTG.Business.Models;
using MCTG.Presentation.Services;

namespace MCTG.Presentation.Controllers
{
    public class UserController : BaseController
    {
        private readonly AuthService _authService;
        private readonly CardService _cardService;
        public UserController(UserService userService, AuthService authService, CardService cardService) : base(userService)
        {
            _authService = authService;
            _cardService = cardService;
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
                return CreateResponse(400, "Username and password are required");
            }

            return _authService.Register(username, password)
                ? CreateResponse(201, "Registration successful")
                : CreateResponse(409, "Username already exists");
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
                return CreateResponse(400, "Username and password are required");
            }

            var token = _authService.Login(username, password);
            return token != null
                ? CreateResponse(200, $"Login successful. Token: {token.Value}")
                : CreateResponse(401, "Invalid username/password combination");
        }

        public string Logout(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            return _authService.Logout(authToken)
                ? CreateResponse(200, "Logout successful")
                : CreateResponse(400, "Logout failed");
        }

        public string GetUserData(string authToken, string username)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            if (user.Username != username && !user.Username.Equals("admin"))
                return CreateResponse(403, "Access denied. You can only view your own data.");

            var targetUser = _userService.GetUserByToken(authToken);
            if (targetUser == null)
                return CreateResponse(404, "User not found");

            var userData = new
            {
                targetUser.Username,
                Profile = new
                {
                    targetUser.Profile.Bio,
                    targetUser.Profile.Image
                },
                targetUser.ELO,
                targetUser.Wins,
                targetUser.Losses,
                targetUser.Coins,
                StackSize = _cardService.GetUserStackSize(targetUser.Id),
                Deck = _cardService.GetUserDeck(targetUser.Id)
            };

            return CreateResponse(200, SerializeResponse(userData));
        }

        public string GetUserStats(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var stats = new
            {
                user.Username,
                user.ELO,
                user.Wins,
                user.Losses,
                user.Coins,
                StackSize = _cardService.GetUserStackSize(user.Id)
            };

            return CreateResponse(200, SerializeResponse(stats));
        }

        public string GetScoreboard(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var scoreboard = _userService.GetScoreboard()
                .Select((player, index) => new
                {
                    Rank = index + 1,
                    player.Username,
                    player.ELO,
                    WinLoss = $"{player.Wins}/{player.Losses}",
                })
                .ToList();

            return CreateResponse(200, SerializeResponse(scoreboard));
        }

        public string UpdateUserData(string authToken, string username, string requestBody)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
            {
                return error;
            }

            if (user.Username != username && !user.Username.Equals("admin"))
                return CreateResponse(403, "Access denied. You can only update your own profile.");

            try
            {
                var profile = DeserializeBody<UserProfile>(requestBody, out error);
                if (profile == null)
                    return CreateResponse(400, error);

                return _userService.UpdateUserProfile(user.Id, profile)
                    ? CreateResponse(200, "User data updated successfully")
                    : CreateResponse(404, "User not found");
            }
            catch (Exception ex)
            {
                return CreateResponse(400, ex.Message);
            }
        }
    }
}
