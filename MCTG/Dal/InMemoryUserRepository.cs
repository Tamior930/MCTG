using MCTG.BusinessLayer.Models;

namespace MCTG.Dal
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public InMemoryUserRepository()
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
