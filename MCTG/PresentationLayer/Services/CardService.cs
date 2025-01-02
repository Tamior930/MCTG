using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
{
    public class CardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;
        private readonly ITradeRepository _tradeRepository;
        private const int PACKAGE_SIZE = 5;
        private const int DECK_SIZE = 4;
        private const int PACKAGE_COST = 5;

        public CardService(IUserRepository userRepository, ICardRepository cardRepository,
            IDeckRepository deckRepository, ITradeRepository tradeRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
        }

        // Package Management
        public string PurchasePackage(User user)
        {
            if (user == null)
                return "Error: User not found";

            if (user.Coins < PACKAGE_COST)
                return "Error: Insufficient coins";

            var package = _cardRepository.GetRandomCardsForPackage(PACKAGE_SIZE);
            if (package.Count != PACKAGE_SIZE)
                return "Error: Failed to create package";

            try
            {
                // Execute purchase transaction
                if (_userRepository.UpdateUserCoins(user.Id, -PACKAGE_COST))
                {
                    foreach (var card in package)
                    {
                        _cardRepository.AddCard(card, user.Id);
                    }
                    return $"Package purchased successfully! Added {PACKAGE_SIZE} cards to your stack.";
                }
                return "Error: Failed to process payment";
            }
            catch (Exception ex)
            {
                return $"Error: Transaction failed - {ex.Message}";
            }
        }

        // Card Management
        public List<Card> GetUserCards(int userId)
        {
            try
            {
                return _cardRepository.GetAllCardsForUser(userId);
            }
            catch
            {
                return new List<Card>();
            }
        }

        public List<Card> GetUserDeck(int userId)
        {
            try
            {
                return _cardRepository.GetDeckCards(userId);
            }
            catch
            {
                return new List<Card>();
            }
        }

        public string ConfigureDeck(int userId, List<int> cardIds)
        {
            if (cardIds == null || cardIds.Count != DECK_SIZE)
                return "Error: Deck must contain exactly 4 cards";

            var cards = _cardRepository.GetCardsByIds(cardIds);
            if (cards.Count != DECK_SIZE)
                return "Error: One or more cards not found";

            // Verify card ownership
            foreach (var card in cards)
            {
                if (!_cardRepository.ValidateCardOwnership(card.Id, userId))
                    return $"Error: Card {card.Name} is not in your stack";
            }

            try
            {
                _deckRepository.SaveDeck(userId, cards);
                return "Deck configured successfully!";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to configure deck - {ex.Message}";
            }
        }

        // public List<Trade> GetTradingDeals()
        // {
        //     return _tradeRepository.GetAllTrades();
        // }

        // public string CreateTradingDeal(int userId, TradingDeal tradingDeal)
        // {
        //     if (!_cardRepository.ValidateCardOwnership(tradingDeal.CardToTrade, userId))
        //         return "Error: Card not in user's stack";

        //     if (_cardRepository.IsCardInDeck(tradingDeal.CardToTrade))
        //         return "Error: Card is in deck";

        //     var trade = new Trade
        //     {
        //         CardId = tradingDeal.CardToTrade,
        //         UserId = userId,
        //         MinimumDamage = tradingDeal.MinimumDamage,
        //         RequiredType = tradingDeal.Type
        //     };

        //     return _tradeRepository.CreateTrade(trade)
        //         ? "Trading deal created successfully"
        //         : "Error: Failed to create trading deal";
        // }
    }
}