using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class TradingController : BaseController
    {
        private readonly TradingService _tradingService;

        public TradingController(TradingService tradingService, UserService userService)
            : base(userService)
        {
            _tradingService = tradingService;
        }

        public string GetTradingDeals(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var trades = _tradingService.GetTradingDeals();
            return CreateResponse(200, FormatTrades(trades));
        }

        public string CreateTradingDeal(string authToken, string body)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var dealRequest = DeserializeBody<TradingDealRequest>(body, out error);
            if (dealRequest == null)
                return CreateResponse(400, error);

            var result = _tradingService.CreateTradingDeal(user.Id, dealRequest);
            return result.StartsWith("Error")
                ? CreateResponse(400, result)
                : CreateResponse(201, result);
        }

        public string DeleteTradingDeal(string authToken, string tradingId)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var result = _tradingService.DeleteTradingDeal(user.Id, tradingId);
            return result.StartsWith("Error")
                ? CreateResponse(400, result)
                : CreateResponse(200, result);
        }

        public string ExecuteTrade(string authToken, string tradingId, string body)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var cardToTrade = DeserializeBody<TradeOffer>(body, out error);
            if (cardToTrade == null)
                return CreateResponse(400, error);

            var result = _tradingService.ExecuteTrade(user.Id, tradingId, cardToTrade.CardId);
            return result.StartsWith("Error")
                ? CreateResponse(400, result)
                : CreateResponse(200, result);
        }

        private string FormatTrades(List<Trade> trades)
        {
            if (!trades.Any())
                return "No trading deals available";

            return SerializeResponse(trades);
        }
    }
}