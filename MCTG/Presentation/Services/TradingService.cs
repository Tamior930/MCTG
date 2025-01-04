using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
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
            // 1. Verify card ownership
            if (!_cardRepository.ValidateCardOwnership(trade.CardId, user.Id))
                return "Error: You don't own this card";

            // 2. Check if card is in deck (roadmap: MUST NOT BE IN THE DECK)
            if (_deckRepository.IsCardInDeck(trade.CardId))
                return "Error: Cannot trade cards that are in your deck";

            // 3. Check if card is already in a trade
            if (_tradeRepository.IsCardInTrade(trade.CardId))
                return "Error: This card is already in a trading deal";

            // 4. Set trade properties
            trade.UserId = user.Id;
            trade.Status = true;

            // 5. Create the trade
            try
            {
                bool success = _tradeRepository.CreateTrade(trade);
                return success
                    ? "Trading deal created successfully"
                    : "Error: Failed to create trading deal";
            }
            catch (Exception ex)
            {
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
            // 1. Verify card ownership
            if (!_cardRepository.ValidateCardOwnership(offeredCardId, newOwnerId))
                return "Error: You don't own this card";

            // 2. Check if offered card is in deck
            if (_deckRepository.IsCardInDeck(offeredCardId))
                return "Error: Cannot trade cards that are in your deck";

            // 3. Get trade details
            var trade = _tradeRepository.GetTradeById(tradingId);
            if (trade == null)
                return "Error: Trading deal not found";

            // 4. Validate trade requirements
            if (!ValidateTradeRequirements(offeredCardId, trade))
                return "Error: Offered card doesn't meet the trading requirements";

            // 5. Execute the trade
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

            // Check if it's the required card type (Spell or Monster)
            if (!offeredCard.Type.ToString().Equals(trade.RequiredType, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check minimum damage requirement
            if (offeredCard.Damage < trade.MinimumDamage)
                return false;

            return true;
        }
    }
}