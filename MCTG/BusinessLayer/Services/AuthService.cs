using MCTG.BusinessLayer.Interfaces;
using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Services
{
    internal class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool Register(string username, string password)
        {
            if (_userRepository.UserExists(username)) return false;
            var user = new User { Username = username, Password = password };
            _userRepository.AddUser(user);
            return true;
        }

        public string Login(string username, string password)
        {
            var user = _userRepository.GetUser(username);
            if (user != null && user.Password == password)
            {
                user.Token = GenerateToken(username);
                return user.Token;
            }
            return null;
        }

        private string GenerateToken(string username)
        {
            return $"token-{username}-{System.Guid.NewGuid()}";
        }
    }
}
