using MCTG.Business.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Presentation.Services
{
    public class CardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;
        private const int PACKAGE_SIZE = 5;
        private const int DECK_SIZE = 4;
        private const int PACKAGE_COST = 5;

        public CardService(IUserRepository userRepository, ICardRepository cardRepository,
            IDeckRepository deckRepository)
        {
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
        }

        public string PurchasePackage(User user)
        {
            if (user.Coins < PACKAGE_COST)
                return "Error: Insufficient coins";

            try
            {
                var package = _cardRepository.GetRandomCardsForPackage(PACKAGE_SIZE);
                if (package.Count != PACKAGE_SIZE)
                    return "Error: Failed to create package";

                if (_userRepository.UpdateUserCoins(user.Id, -PACKAGE_COST))
                {
                    user.UpdateCoins(-PACKAGE_COST);
                    foreach (var card in package)
                    {
                        user.Stack.AddCard(card);
                        _cardRepository.UpdateCardOwnership(card, user.Id);
                    }
                    return $"Package purchased successfully!";
                }
                return "Error: Failed to process payment";
            }
            catch (Exception ex)
            {
                return $"Error: Transaction failed - {ex.Message}";
            }
        }

        public List<Card> GetUserCards(int userId)
        {
            return _cardRepository.GetAllCardsForUser(userId);
        }

        public List<Card> GetUserDeck(int userId)
        {
            return _deckRepository.GetDeckCards(userId);
        }

        public int GetUserStackSize(int userId)
        {
            return _cardRepository.GetAllCardsForUser(userId).Count;
        }

        public string ConfigureDeck(User user, List<int> cardIds)
        {
            if (cardIds == null || cardIds.Count != DECK_SIZE)
            {
                return "Error: Deck must contain exactly 4 cards";
            }

            var selectedCards = new List<Card>();
            foreach (var cardId in cardIds)
            {
                var card = _cardRepository.GetCardById(cardId);
                if (card == null)
                    return "Error: One or more cards not found";

                if (!_cardRepository.ValidateCardOwnership(cardId, user.Id))
                    return "Error: You don't own one or more of these cards";

                selectedCards.Add(card);
            }

            try
            {
                if (!_deckRepository.SaveDeck(user.Id, selectedCards))
                {
                    return "Error: Failed to save deck";
                }
                user.Deck.SetDeck(selectedCards);

                return "Deck configured successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}