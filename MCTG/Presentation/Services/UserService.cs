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

        // Gets user by authentication token
        public User? GetUserByToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return null;
            }

            return _userRepository.GetUserByToken(authToken);
        }

        // Updates user profile information
        public bool UpdateUserProfile(int userId, UserProfile newProfile)
        {
            return _userRepository.UpdateUserProfile(userId, newProfile);
        }

        // Gets scoreboard sorted by ELO rating
        public List<User> GetScoreboard()
        {
            return _userRepository.GetScoreboard();
        }

        // Updates user statistics after battle
        public bool UpdateStats(string authToken, bool won)
        {
            return _userRepository.UpdateUserStats(authToken, won);
        }
    }
}
