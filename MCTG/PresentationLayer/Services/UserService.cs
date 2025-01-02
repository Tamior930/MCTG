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

        public User GetUserByToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return null;
            }

            return _userRepository.GetUserByToken(authToken);
        }

        public bool UpdateUserProfile(string authToken, UserProfile newProfile)
        {
            var user = _userRepository.GetUserByToken(authToken);
            if (user == null)
                return false;

            return _userRepository.UpdateUserProfile(authToken, newProfile);
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
