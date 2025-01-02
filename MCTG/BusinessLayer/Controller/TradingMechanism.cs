using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.BusinessLayer.Controller
{
    public class TradingMechanism
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;

        public TradingMechanism(ITradeRepository tradeRepository, ICardRepository cardRepository, IDeckRepository deckRepository)
        {
            _tradeRepository = tradeRepository;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
        }

        public string CreateTradingDeal(int userId, int cardId, CardType requiredType,
            ElementType? requiredElementType, MonsterType? requiredMonsterType, int minimumDamage)
        {
            // Verify card ownership
            if (!_cardRepository.ValidateCardOwnership(cardId, userId))
                return "Error: You don't own this card";

            // Check if card is in deck
            if (_deckRepository.IsCardInDeck(cardId))
                return "Error: Cannot trade cards that are in your deck";

            // Check if card is already in a trade
            if (_tradeRepository.IsCardInTrade(cardId))
                return "Error: This card is already in a trading deal";

            try
            {
                bool success = _tradeRepository.CreateTrade(cardId, userId, requiredType,
                    requiredElementType, requiredMonsterType, minimumDamage);

                return success
                    ? "Trading deal created successfully"
                    : "Failed to create trading deal";
            }
            catch (Exception ex)
            {
                return $"Error creating trade: {ex.Message}";
            }
        }

        public string ExecuteTrade(int tradeId, int offeredCardId, int newOwnerId)
        {
            // Get trade details
            var trade = _tradeRepository.GetTradeById(tradeId);
            if (trade == null)
                return "Error: Trading deal not found";

            // Verify card ownership
            if (!_cardRepository.ValidateCardOwnership(offeredCardId, newOwnerId))
                return "Error: You don't own the offered card";

            // Check if offered card is in deck
            if (_deckRepository.IsCardInDeck(offeredCardId))
                return "Error: Cannot trade cards that are in your deck";

            // Validate trade requirements
            if (!_tradeRepository.ValidateTradeRequirements(offeredCardId, trade))
                return "Error: Offered card doesn't meet the trading requirements";

            try
            {
                bool success = _tradeRepository.ExecuteTrade(tradeId, offeredCardId, newOwnerId);
                return success
                    ? "Trade executed successfully"
                    : "Failed to execute trade";
            }
            catch (Exception ex)
            {
                return $"Error executing trade: {ex.Message}";
            }
        }

        public string CancelTrade(int tradeId, int userId)
        {
            var trade = _tradeRepository.GetTradeById(tradeId);
            if (trade == null)
                return "Error: Trading deal not found";

            if (trade.UserId != userId)
                return "Error: You can only cancel your own trading deals";

            try
            {
                bool success = _tradeRepository.CancelTrade(tradeId);
                return success
                    ? "Trading deal cancelled successfully"
                    : "Failed to cancel trading deal";
            }
            catch (Exception ex)
            {
                return $"Error cancelling trade: {ex.Message}";
            }
        }

        public List<Trade> GetActiveTrades()
        {
            return _tradeRepository.GetAllActiveTrades();
        }

        public List<Trade> GetUserTrades(int userId)
        {
            return _tradeRepository.GetTradesByUser(userId);
        }
    }
}
