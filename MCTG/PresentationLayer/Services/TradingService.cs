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
            return _tradeRepository.GetAllActiveTrades();
        }

        public string CreateTradingDeal(int userId, TradingDealRequest request)
        {
            // Verify card ownership
            if (!_cardRepository.ValidateCardOwnership(request.CardToTrade, userId))
                return "Error: You don't own this card";

            // Check if card is in deck
            if (_deckRepository.IsCardInDeck(request.CardToTrade))
                return "Error: Cannot trade cards that are in your deck";

            try
            {
                _tradeRepository.CreateTrade(
                    request.CardToTrade,
                    userId,
                    request.RequiredCardType,
                    request.RequiredElementType,
                    request.RequiredMonsterType,
                    request.MinimumDamage
                );

                return "Trading deal created successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public string DeleteTradingDeal(int userId, string tradingId)
        {
            if (!int.TryParse(tradingId, out int tradeId))
                return "Error: Invalid trading deal ID";

            var trade = _tradeRepository.GetTradeById(tradeId);
            if (trade == null)
                return "Error: Trading deal not found";

            if (trade.UserId != userId)
                return "Error: You can only delete your own trading deals";

            if (_tradeRepository.CancelTrade(tradeId))
                return "Trading deal deleted successfully";

            return "Error: Failed to delete trading deal";
        }

        public string ExecuteTrade(int userId, string tradingId, int offeredCardId)
        {
            if (!int.TryParse(tradingId, out int tradeId))
                return "Error: Invalid trading deal ID";

            // Get trade details
            var trade = _tradeRepository.GetTradeById(tradeId);
            if (trade == null)
                return "Error: Trading deal not found";

            // Verify card ownership
            if (!_cardRepository.ValidateCardOwnership(offeredCardId, userId))
                return "Error: You don't own the offered card";

            // Get offered card details
            var offeredCard = _cardRepository.GetCardById(offeredCardId);
            if (offeredCard == null)
                return "Error: Offered card not found";

            // Validate trade requirements
            if (!trade.IsValidTradeCard(offeredCard))
                return "Error: Offered card does not meet trade requirements";

            // Check if card is in deck
            if (_deckRepository.IsCardInDeck(offeredCardId))
                return "Error: Cannot trade cards that are in your deck";

            try
            {
                // Execute the trade
                if (_tradeRepository.ExecuteTrade(tradeId, offeredCardId, userId))
                {
                    // Update card ownerships
                    _cardRepository.UpdateCardOwner(trade.CardId, userId);
                    _cardRepository.UpdateCardOwner(offeredCardId, trade.UserId);
                    return "Trade executed successfully";
                }

                return "Error: Failed to execute trade";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}