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
                return false;
            }

            User newUser = new User(username, password);
            _userRepository.AddUser(newUser);
            return true;
        }

        public Token? Login(string username, string password)
        {
            User user = _userRepository.GetUserByUsername(username);
            if (user != null && user.Password == password)
            {
                string tokenValue = Guid.NewGuid().ToString();
                DateTime expiryTime = DateTime.Now.AddHours(1);
                var token = new Token(tokenValue, expiryTime);

                user.AssignToken(token);

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
                var expiredToken = new Token(authToken, DateTime.Now.AddSeconds(-1));
                return _userRepository.UpdateUserToken(user.Id, expiredToken);
            }
            return false;
        }
    }
}
