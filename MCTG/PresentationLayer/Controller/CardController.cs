using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class CardPackageController
    {
        private readonly CardService _cardService;
        private readonly UserService _userService;

        public CardPackageController(CardService cardService, UserService userService)
        {
            _cardService = cardService;
            _userService = userService;
        }

        public string PurchasePackage(string authToken)
        {
            var user = _userService.GetUserByToken(authToken);
            if (user == null)
            {
                return "Error: User not found.";
            }

            if (!user.CanAffordPackage())
            {
                return "Error: Insufficient coins to purchase package.";
            }

            var package = _cardService.CreateRandomPackage();
            if (user.PurchasePackage(package))
            {
                return "Package purchased successfully!";
            }

            return "Error: Failed to purchase package.";
        }
    }
}
