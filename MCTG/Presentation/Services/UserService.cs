using MCTG.Business.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Presentation.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? GetUserByToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return null;
            }

            return _userRepository.GetUserByToken(authToken);
        }

        public bool UpdateUserProfile(int userId, UserProfile newProfile)
        {
            return _userRepository.UpdateUserProfile(userId, newProfile);
        }

        public List<User> GetScoreboard()
        {
            return _userRepository.GetScoreboard();
        }

        public bool UpdateStats(string authToken, bool won)
        {
            return _userRepository.UpdateUserStats(authToken, won);
        }
    }
}
