using MCTG.Business.Models;

namespace MCTG.Data.Interfaces
{
    public interface IUserRepository
    {
        void AddUser(User user);
        User GetUserByToken(string authToken);
        User GetUserByUsername(string username);
        User GetUserById(int userId);
        bool UpdateUserProfile(int userId, UserProfile profile);
        bool UpdateUserStats(string authToken, bool won);
        bool UpdateUserCoins(int userId, int amount);
        List<User> GetScoreboard();
        bool UserExists(string username);
        bool UpdateUserToken(int userId, Token token);
    }
}
