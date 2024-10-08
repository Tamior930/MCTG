using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.Dal
{
    public class InMemoryUserRepository : IUserRepository
    {
        private List<User> _users;

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
            return _users.FirstOrDefault(u => u.Username == username);
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
