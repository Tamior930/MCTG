using MCTG.BusinessLayer.Models;
using MCTG.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.PresentationLayer.Services
{
    public class UserService
    {
        private readonly IUserRepository _playerRepository;

        public UserService(IUserRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public User GetUserByToken(string token)
        {
            return _playerRepository.GetAllUsers().FirstOrDefault(u => u.AuthToken != null && u.AuthToken.Value == token && u.AuthToken.IsValid());
        }

        public bool PurchasePackage(User user, List<Card> package)
        {
            return user.PurchasePackage(package);
        }
    }
}
