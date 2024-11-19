using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class UserController
    {
        private readonly UserService _userService;
        private readonly CardService _cardService;

        public UserController(UserService userService, CardService cardService)
        {
            _userService = userService;
            _cardService = cardService;
        }

        public string GetUserStats(string authToken)
        {
            var user = _userService.ValidateAndGetUser(authToken, out var error);
            if (user == null)
                return error;

            return $"User Stats for {user.Username}:\n" +
                   $"ELO: {user.ELO}\n" +
                   $"Wins: {user.Wins}\n" +
                   $"Losses: {user.Losses}\n" +
                   $"Win Rate: {user.GetWinRate():P2}\n" +
                   $"Coins: {user.Coins}\n" +
                   $"Cards in Stack: {user.GetStackSize()}";
        }

        public string ConfigureDeck(string authToken, List<string> cardIds)
        {
            var user = _userService.GetUserByToken(authToken);
            if (user == null)
            {
                return "Error: User not found.";
            }

            if (cardIds.Count != 4)
            {
                return "Error: Deck must contain exactly 4 cards.";
            }

            var cards = _cardService.GetCardsByIds(cardIds);
            if (cards.Count != 4)
            {
                return "Error: One or more cards not found.";
            }

            foreach (var card in cards)
            {
                if (!user.HasCardInStack(card))
                {
                    return $"Error: Card {card.Name} is not in your stack.";
                }
            }

            user.Deck.SetDeck(cards);
            return "Deck configured successfully!";
        }
    }
}
