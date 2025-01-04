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

        public string DeleteTradingDeal(string authToken, string tradingId)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            try
            {
                var trade = _tradingService.GetTradingDealById(tradingId);
                if (trade == null)
                    return CreateResponse(404, "Trading deal not found");

                if (trade.UserId != user.Id)
                    return CreateResponse(403, "You can only delete your own trading deals");

                var result = _tradingService.DeleteTradingDeal(tradingId);

                return result.StartsWith("Error")
                    ? CreateResponse(400, result)
                    : CreateResponse(200, "Trading deal successfully deleted");
            }
            catch (Exception ex)
            {
                return CreateResponse(500, $"Error: {ex.Message}");
            }
        }

        public string ExecuteTrade(string authToken, string tradingId, string body)
        {
            // 1. Authenticate user
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            // 2. Parse the offered card from request body
            var card = DeserializeBody<Card>(body, out error);
            if (card == null)
                return CreateResponse(400, error);

            try
            {
                // 3. Get the trade
                var trade = _tradingService.GetTradingDealById(tradingId);
                if (trade == null)
                    return CreateResponse(404, "Trading deal not found");

                // 4. Verify user is not trading with themselves
                if (trade.UserId == user.Id)
                    return CreateResponse(403, "You cannot trade with yourself");

                // 5. Execute the trade
                var result = _tradingService.ExecuteTrade(tradingId, card.Id, user.Id);

                return result.StartsWith("Error")
                    ? CreateResponse(400, result)
                    : CreateResponse(200, "Trading deal successfully executed");
            }
            catch (Exception ex)
            {
                return CreateResponse(500, $"Error: {ex.Message}");
            }
        }
    }
}