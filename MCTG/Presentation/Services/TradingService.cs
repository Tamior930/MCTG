using MCTG.Business.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Presentation.Services
{
    public class TradingService
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;

        public TradingService(ITradeRepository tradeRepository,
            ICardRepository cardRepository,
            IDeckRepository deckRepository)
        {
            _tradeRepository = tradeRepository;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
        }

        public List<Trade> GetTradingDeals()
        {
            return _tradeRepository.GetAllTradingDeals();
        }

        public string CreateTradingDeal(User user, Trade trade)
        {
            if (trade == null || trade.CardId <= 0)
                return "Error: Invalid trade data";

            try
            {
                if (!_cardRepository.ValidateCardOwnership(trade.CardId, user.Id))
                    return "Error: You don't own this card";

                if (_deckRepository.IsCardInDeck(trade.CardId))
                    return "Error: Cannot trade cards that are in your deck";

                if (_tradeRepository.IsCardInTrade(trade.CardId))
                    return "Error: This card is already in a trading deal";

                if (string.IsNullOrEmpty(trade.RequiredType))
                    return "Error: Required type must be specified";

                if (trade.MinimumDamage <= 0)
                    return "Error: Minimum damage must be greater than 0";

                trade.UserId = user.Id;
                bool success = _tradeRepository.CreateTrade(trade);

                return success
                    ? "Trading deal created successfully"
                    : "Error: Failed to create trading deal";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Trading error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public string DeleteTradingDeal(string tradingId)
        {
            try
            {
                bool success = _tradeRepository.DeleteTrade(tradingId);
                return success
                    ? "Trading deal successfully deleted"
                    : "Error: Failed to delete trading deal";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public Trade GetTradingDealById(string tradingId)
        {
            return _tradeRepository.GetTradeById(tradingId);
        }

        public string ExecuteTrade(string tradingId, int offeredCardId, int newOwnerId)
        {
            if (!_cardRepository.ValidateCardOwnership(offeredCardId, newOwnerId))
                return "Error: You don't own this card";

            if (_deckRepository.IsCardInDeck(offeredCardId))
                return "Error: Cannot trade cards that are in your deck";

            var trade = _tradeRepository.GetTradeById(tradingId);
            if (trade == null)
                return "Error: Trading deal not found";

            if (!ValidateTradeRequirements(offeredCardId, trade))
                return "Error: Offered card doesn't meet the trading requirements";

            try
            {
                bool success = _tradeRepository.ExecuteTrade(tradingId, offeredCardId, newOwnerId);
                return success
                    ? "Trading deal successfully executed"
                    : "Error: Failed to execute trade";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private bool ValidateTradeRequirements(int offeredCardId, Trade trade)
        {
            var offeredCard = _cardRepository.GetCardById(offeredCardId);
            if (offeredCard == null) return false;

            if (!offeredCard.Type.ToString().Equals(trade.RequiredType, StringComparison.OrdinalIgnoreCase))
                return false;

            if (offeredCard.Damage < trade.MinimumDamage)
                return false;

            return true;
        }
    }
}