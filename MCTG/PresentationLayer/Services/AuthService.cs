using MCTG.BusinessLayer.Models;
using MCTG.Dal;

namespace MCTG.PresentationLayer.Services
{
    public class AuthService
    {
        private readonly IUserRepository _playerRepository;

        public AuthService(IUserRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public bool Register(string username, string password)
        {
            if (_playerRepository.UserExists(username))
                return false;

            User newUser = new User(username, password);
            _playerRepository.AddUser(newUser);
            return true;
        }

        public Token Login(string username, string password)
        {
            User user = _playerRepository.GetUserByUsername(username);
            if (user != null && user.Password == password)
            {
                Token token = Token.GenerateToken();
                user.AssignToken(token);
                return token;
            }
            return null;
        }

        public bool ValidateToken(string tokenValue)
        {
            foreach (var user in _playerRepository.GetAllUsers())
            {
                if (user.AuthToken != null && user.AuthToken.Value == tokenValue && user.AuthToken.IsValid())
                    return true;
            }
            return false;
        }
    }
}
