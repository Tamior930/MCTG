using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.Dal
{
    internal class UserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users = new();

        public User GetUser(string username)
        {
            _users.TryGetValue(username, out User user);
            return user;
        }

        public void AddUser(User user)
        {
            _users[user.Username] = user;
        }

        public bool UserExists(string username)
        {
            return _users.ContainsKey(username);
        }
    }
}
