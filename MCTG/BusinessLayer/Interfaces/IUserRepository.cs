using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Interfaces
{
    internal interface IUserRepository
    {
        User GetUser(string username);
        void AddUser(User user);
        bool UserExists(string username);
    }
}
