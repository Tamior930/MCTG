using MCTG.BusinessLayer.Models;
using MCTG.Dal;

namespace MCTG.PresentationLayer.Services
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

        // Checks Login if OK and then assigns the token to the User
        public Token Login(string username, string password)
        {
            User user = _userRepository.GetUserByUsername(username);
            if (user != null && user.Password == password)
            {
                Token token = Token.GenerateToken();
                user.AssignToken(token);
                return token;
            }
            return null;
        }
    }
}
