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

        // Creates new user account
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

        // Authenticates user and generates access token
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
    }
}
