using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface IUserRepository
    {
        void AddUser(User user);
        User GetUserByUsername(string username);
        bool RemoveUserByUsername(string username);
        bool UserExists(string username);
        List<User> GetAllUsers();
    }
}
