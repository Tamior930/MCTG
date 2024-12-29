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

            return _userRepository.GetUserByToken(token);
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

        public User GetUserByUsername(string username)
        {
            return _userRepository.GetUserByUsername(username);
        }

        // public bool PurchasePackage(User user, List<Card> package)
        // {
        //    return user.PurchasePackage(package);
        // }

        public bool UpdateUserProfile(string username, UserProfile newProfile)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user == null)
                return false;

            return _userRepository.UpdateUserProfile(user.Id, newProfile);
        }

        public List<User> GetScoreboard()
        {
            return _userRepository.GetScoreboard();
        }

        public bool UpdateStats(int userId, bool won)
        {
            return _userRepository.UpdateUserStats(userId, won);
        }

        public bool UpdateCoins(int userId, int amount)
        {
            return _userRepository.UpdateUserCoins(userId, amount);
        }

        public int GetCoins(int userId)
        {
            return _userRepository.GetUserCoins(userId);
        }
    }
}
