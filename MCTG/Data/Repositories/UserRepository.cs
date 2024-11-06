using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public UserRepository()
        {
            _users = new List<User>();
        }

        public void AddUser(User user)
        {
            _users.Add(user);
        }

        public User GetUserByUsername(string username)
        {
            return _users.Find(u => u.Username == username);
        }

        public bool RemoveUserByUsername(string username)
        {
            var user = _users.Find(u => u.Username == username);

            if (user != null)
            {
                _users.Remove(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UserExists(string username)
        {
            return _users.Any(u => u.Username == username);
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }
    }
}
