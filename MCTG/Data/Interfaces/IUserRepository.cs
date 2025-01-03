using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface IUserRepository
    {
        // Basic CRUD
        void AddUser(User user);
        User GetUserByToken(string authToken);
        User GetUserByUsername(string username);
        bool UpdateUserProfile(string authToken, UserProfile profile);
        // User MapUserFromDatabase(NpgsqlDataReader reader);

        // User Stats and Currency
        bool UpdateUserStats(string authToken, bool won);
        bool UpdateUserCoins(int userId, int amount);
        //int GetUserCoins(string authToken);

        // User Lists and Rankings
        List<User> GetScoreboard();

        // Validation
        bool UserExists(string username);
        bool UpdateUserToken(int userId, Token token);
    }
}
