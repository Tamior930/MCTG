using MCTG.BusinessLayer.Models;

namespace MCTG.Dal
{
    public interface IUserRepository
    {
        void AddUser(User user);
        User GetUserByUsername(string username);
        bool UserExists(string username);
        List<User> GetAllUsers();
    }
}
