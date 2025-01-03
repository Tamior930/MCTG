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
            trade.IsActive = true;

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
    }
}