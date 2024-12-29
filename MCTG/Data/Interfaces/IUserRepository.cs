using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface IUserRepository
    {
        // Basic CRUD
        void AddUser(User user);
        User GetUserById(int userId);
        User GetUserByToken(string authToken);
        User GetUserByUsername(string username);
        bool UpdateUserProfile(int userId, UserProfile profile);

        // User Stats and Currency
        bool UpdateUserStats(int userId, bool won);
        bool UpdateUserCoins(int userId, int amount);
        int GetUserCoins(int userId);

        // User Lists and Rankings
        List<User> GetAllUsers();
        List<User> GetScoreboard();

        // Validation
        bool UserExists(string username);
        bool HasEnoughCoins(int userId, int requiredAmount);
        bool UpdateUserToken(int userId, Token token);
    }
}
