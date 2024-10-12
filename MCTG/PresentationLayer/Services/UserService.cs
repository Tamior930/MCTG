using MCTG.Dal;

namespace MCTG.PresentationLayer.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //public User GetUserByToken(string token)
        //{
        //    return _userRepository.GetAllUsers().FirstOrDefault(u => u.AuthToken != null && u.AuthToken.Value == token && u.AuthToken.IsValid());
        //}

        //public bool PurchasePackage(User user, List<Card> package)
        //{
        //    return user.PurchasePackage(package);
        //}
    }
}
