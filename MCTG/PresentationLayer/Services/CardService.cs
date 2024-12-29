using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
{
    public class CardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;
        private const int PACKAGE_SIZE = 5;
        private const int DECK_SIZE = 4;

        public CardService(IUserRepository userRepository, ICardRepository cardRepository, IDeckRepository deckRepository)
        {
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _deckRepository = deckRepository;
        }

        public List<Card> CreateRandomPackage()
        {
            return _cardRepository.GetRandomCardsForPackage(PACKAGE_SIZE);
        }

        public string PurchasePackage(string authToken)
        {
            // Get user from database
            var user = _userRepository.GetUserByToken(authToken);
            if (user == null)
                return "Error: User not found";

            if (!user.HasValidToken())
                return "Error: Invalid token";

            if (!user.CanAffordPackage())
                return "Error: Insufficient coins";

            // Get random cards for package
            var package = _cardRepository.GetRandomCardsForPackage(PACKAGE_SIZE);
            if (package.Count != PACKAGE_SIZE)
                return "Error: Failed to create package";

            // Use domain logic to attempt purchase
            if (user.PurchasePackage(package))
            {
                // If successful, persist changes to database
                _userRepository.UpdateUserCoins(user.Id, -5);
                foreach (var card in package)
                {
                    _cardRepository.AddCard(card, user.Id);
                }
                return "Package purchased successfully!";
            }

            return "Error: Failed to purchase package";
        }

        public List<Card> GetUserCards(string authToken)
        {
            var user = _userRepository.GetUserByToken(authToken);
            if (user == null || !user.HasValidToken())
                return new List<Card>();

            return _cardRepository.GetAllCardsForUser(user.Id);
        }

        public List<Card> GetUserDeck(string authToken)
        {
            var user = _userRepository.GetUserByToken(authToken);
            if (user == null || !user.HasValidToken())
                return new List<Card>();

            return _cardRepository.GetDeckCards(user.Id);
        }

        public string ConfigureDeck(string authToken, List<int> cardIds)
        {
            var user = _userRepository.GetUserByToken(authToken);
            if (user == null)
                return "Error: User not found";

            if (!user.HasValidToken())
                return "Error: Invalid token";

            if (cardIds.Count != DECK_SIZE)
                return "Error: Deck must contain exactly 4 cards";

            var cards = _cardRepository.GetCardsByIds(cardIds);
            if (cards.Count != DECK_SIZE)
                return "Error: One or more cards not found";

            // Verify all cards belong to user
            foreach (var card in cards)
            {
                if (!_cardRepository.ValidateCardOwnership(card.Id, user.Id))
                    return $"Error: Card {card.Name} is not in your stack";
            }

            try
            {
                _deckRepository.SaveDeck(user.Id, cards);
                return "Deck configured successfully!";
            }
            catch (Exception ex)
            {
                return $"Error configuring deck: {ex.Message}";
            }
        }

        public Card GetCardById(int cardId)
        {
            return _cardRepository.GetCardById(cardId);
        }
    }
}