using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User GetUserByToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            return _userRepository.GetAllUsers().FirstOrDefault(u => u.AuthToken != null && u.AuthToken.Value == token && u.AuthToken.IsValid());
        }

        public User ValidateAndGetUser(string token, out string errorMessage)
        {
            var user = GetUserByToken(token);
            if (user == null)
            {
                errorMessage = "Error: User not found or invalid token.";
                return null;
            }
            errorMessage = null;
            return user;
        }

        //public bool PurchasePackage(User user, List<Card> package)
        //{
        //    return user.PurchasePackage(package);
        //}
    }
}
