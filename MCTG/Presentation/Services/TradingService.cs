using MCTG.Business.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Presentation.Services
{
    // Manages card trading between players
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

        // Gets all active trading deals
        public List<Trade> GetTradingDeals()
        {
            return _tradeRepository.GetAllTradingDeals();
        }

        // Creates new trading deal
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

                if (!trade.RequiredType.Equals("spell", StringComparison.OrdinalIgnoreCase) &&
                    !trade.RequiredType.Equals("monster", StringComparison.OrdinalIgnoreCase))
                    return "Error: Required type must be either 'spell' or 'monster'";

                if (trade.MinimumDamage.HasValue && trade.MinimumDamage.Value <= 0)
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

        // Removes existing trading deal
        public string DeleteTradingDeal(int tradingId)
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

        // Gets specific trading deal by ID
        public Trade GetTradingDealById(int tradingId)
        {
            return _tradeRepository.GetTradeById(tradingId);
        }

        // Processes trade execution between users
        public string ExecuteTrade(int tradingId, int offeredCardId, int newOwnerId)
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
                // Get both cards before the trade
                var offeredCard = _cardRepository.GetCardById(offeredCardId);
                var tradedCard = _cardRepository.GetCardById(trade.CardId);
                
                if (offeredCard == null || tradedCard == null)
                    return "Error: One or both cards not found";

                bool success = _tradeRepository.ExecuteTrade(tradingId, offeredCardId, newOwnerId);
                if (!success)
                    return "Error: Failed to execute trade";

                // Update cards in memory after successful database update
                _cardRepository.UpdateCardOwnership(offeredCard, trade.UserId);  // Original owner gets the offered card
                _cardRepository.UpdateCardOwnership(tradedCard, newOwnerId);     // New owner gets the traded card

                return "Trading deal successfully executed";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Validates if offered card meets trade requirements
        private bool ValidateTradeRequirements(int offeredCardId, Trade trade)
        {
            var offeredCard = _cardRepository.GetCardById(offeredCardId);
            if (offeredCard == null) return false;

            // Check if the offered card type matches the required type
            bool isSpellRequired = trade.RequiredType.Equals("spell", StringComparison.OrdinalIgnoreCase);
            bool isSpellOffered = offeredCard.Type == CardType.Spell;
            if (isSpellRequired != isSpellOffered) return false;

            // Check minimum damage requirement if specified
            if (trade.MinimumDamage.HasValue && offeredCard.Damage < trade.MinimumDamage.Value)
                return false;

            return true;
        }
    }
}