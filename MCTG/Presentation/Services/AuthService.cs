using MCTG.Business.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Presentation.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool Register(string username, string password)
        {
            if (_userRepository.UserExists(username))
            {
                return false; // Username already exists
            }

            User newUser = new User(username, password);
            _userRepository.AddUser(newUser);
            return true; // Registration successful
        }

        public Token? Login(string username, string password)
        {
            User user = _userRepository.GetUserByUsername(username);
            if (user != null && user.Password == password)
            {
                // Generate new token
                string tokenValue = Guid.NewGuid().ToString();
                DateTime expiryTime = DateTime.Now.AddHours(1);
                var token = new Token(tokenValue, expiryTime);

                // Assign token to user
                user.AssignToken(token);

                // Persist token to database
                if (_userRepository.UpdateUserToken(user.Id, token))
                {
                    return token;
                }
            }
            return null;
        }

        public bool Logout(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return false;
            }

            var user = _userRepository.GetUserByToken(authToken);
            if (user != null)
            {
                // Create expired token
                var expiredToken = new Token(authToken, DateTime.Now.AddSeconds(-1));
                return _userRepository.UpdateUserToken(user.Id, expiredToken);
            }
            return false;
        }
    }
}
