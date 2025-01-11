using MCTG.Business.Models;
using MCTG.Presentation.Services;

namespace MCTG.Presentation.Controllers
{
    public class TradingController : BaseController
    {
        private readonly TradingService _tradingService;

        public TradingController(TradingService tradingService, UserService userService) : base(userService)
        {
            _tradingService = tradingService;
        }

        // Lists all available trading deals
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

        // Creates new trading deal
        public string CreateTradingDeal(string authToken, string body)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var trade = DeserializeBody<Trade>(body, out error);
            if (trade == null)
                return CreateResponse(400, error);

            // Set the user ID for the trade
            trade.UserId = user.Id;

            var result = _tradingService.CreateTradingDeal(user, trade);

            return result.StartsWith("Error")
                ? CreateResponse(400, result)
                : CreateResponse(201, result);
        }

        // Removes existing trading deal
        public string DeleteTradingDeal(string authToken, int tradingId)
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

        // Processes trade execution between users
        public string ExecuteTrade(string authToken, int tradingId, string body)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var request = DeserializeBody<Dictionary<string, int>>(body, out error);
            if (request == null || !request.ContainsKey("CardId"))
                return CreateResponse(400, "Invalid request format");

            int offeredCardId = request["CardId"];

            try
            {
                var trade = _tradingService.GetTradingDealById(tradingId);
                if (trade == null)
                    return CreateResponse(404, "Trading deal not found");

                if (trade.UserId == user.Id)
                    return CreateResponse(403, "You cannot trade with yourself");

                var result = _tradingService.ExecuteTrade(tradingId, offeredCardId, user.Id);

                return result.StartsWith("Error")
                    ? CreateResponse(400, result)
                    : CreateResponse(200, result);
            }
            catch (Exception ex)
            {
                return CreateResponse(500, $"Error: {ex.Message}");
            }
        }
    }
}