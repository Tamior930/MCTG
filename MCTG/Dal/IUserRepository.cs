using MCTG.BusinessLayer.Interfaces;
using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
