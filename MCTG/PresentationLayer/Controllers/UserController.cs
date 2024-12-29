using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class UserController
    {
        private readonly UserService _userService;
        private readonly CardService _cardService;
        private readonly AuthService _authService;

        public UserController(UserService userService, CardService cardService, AuthService authService)
        {
            _userService = userService;
            _cardService = cardService;
            _authService = authService;
        }

        public string Register(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return "Error: Username and password cannot be empty";

            if (_authService.Register(username, password))
                return "Registration successful";

            return "Error: Username already exists";
        }

        public string Login(string username, string password)
        {
            var token = _authService.Login(username, password);
            if (token != null)
                return $"Login successful. Token: {token.Value}";

            return "Error: Invalid username/password combination";
        }

        public string Logout(string authToken)
        {
            if (_authService.Logout(authToken))
                return "Logout successful";

            return "Error: Invalid token or user not found";
        }

        public string GetUserData(string authToken, string username)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            if (user.Username != username && !user.Username.Equals("admin"))
                return "Error: Access denied. You can only view your own data.";

            var targetUser = _userService.GetUserByUsername(username);
            if (targetUser == null)
                return "Error: User not found";

            return $"User Data for {targetUser.Username}:\n" +
                   $"Name: {targetUser.Profile.Name}\n" +
                   $"Bio: {targetUser.Profile.Bio}\n" +
                   $"Image: {targetUser.Profile.Image}";
        }

        public string UpdateUserProfile(string authToken, string username, UserProfile newProfile)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            if (user.Username != username && !user.Username.Equals("admin"))
                return "Error: Access denied. You can only update your own profile.";

            if (_userService.UpdateUserProfile(username, newProfile))
                return "Profile updated successfully";

            return "Error: Failed to update profile";
        }

        public string GetUserStats(string authToken)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            return $"User Stats for {user.Username}:\n" +
                   $"ELO: {user.ELO}\n" +
                   $"Wins: {user.Wins}\n" +
                   $"Losses: {user.Losses}\n" +
                   $"Win Rate: {user.GetWinRate():P2}\n" +
                   $"Coins: {user.Coins}\n" +
                   $"Cards in Stack: {user.GetStackSize()}";
        }

        public string GetScoreboard(string authToken)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            var scoreboard = _userService.GetScoreboard();
            if (!scoreboard.Any())
                return "No users found in scoreboard";

            var result = "=== SCOREBOARD ===\n";
            int rank = 1;
            foreach (var player in scoreboard)
            {
                result += $"{rank}. {player.Username} - ELO: {player.ELO}, W/L: {player.Wins}/{player.Losses}\n";
                rank++;
            }
            return result;
        }

        public string GetUserCards(string authToken)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            var cards = _cardService.GetUserCards(authToken);
            if (!cards.Any())
                return "No cards found in your stack";

            var result = "=== YOUR CARDS ===\n";
            foreach (var card in cards)
            {
                result += $"{card.Name} - Damage: {card.Damage}, Type: {card.Type}\n";
            }
            return result;
        }

        public string GetUserDeck(string authToken)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            var deck = _cardService.GetUserDeck(authToken);
            if (!deck.Any())
                return "No cards found in your deck";

            var result = "=== YOUR DECK ===\n";
            foreach (var card in deck)
            {
                result += $"{card.Name} - Damage: {card.Damage}, Type: {card.Type}\n";
            }
            return result;
        }
    }
}
