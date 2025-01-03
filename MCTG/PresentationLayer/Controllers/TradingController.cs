using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class TradingController : BaseController
    {
        private readonly TradingService _tradingService;

        public TradingController(TradingService tradingService, UserService userService) : base(userService)
        {
            _tradingService = tradingService;
        }
        
        public string GetTradingDeals(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var deals = _tradingService.GetTradingDeals();
            return deals.Any() 
                ? CreateResponse(200, SerializeResponse(deals))
                : CreateResponse(204, "No trading deals available");
        }

        public string CreateTradingDeal(string authToken, string body)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var trade = DeserializeBody<Trade>(body, out error);
            if (trade == null)
                return CreateResponse(400, error);

            return _tradingService.CreateTradingDeal(user, trade);
        }
    }
}