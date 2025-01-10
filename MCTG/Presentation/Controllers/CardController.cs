using MCTG.Presentation.Services;

namespace MCTG.Presentation.Controllers
{
    public class CardController : BaseController
    {
        private readonly CardService _cardService;

        public CardController(CardService cardService, UserService userService)
            : base(userService)
        {
            _cardService = cardService;
        }

        // Processes card package purchases
        public string AcquirePackage(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var result = _cardService.PurchasePackage(user);
            return result.StartsWith("Error")
                ? CreateResponse(400, result)
                : CreateResponse(200, result);
        }

        // Retrieves user's card collection
        public string GetUserCards(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var cards = _cardService.GetUserCards(user.Id);

            if (!cards.Any())
                return CreateResponse(200, "No cards found");

            return CreateResponse(200, SerializeResponse(cards));
        }

        // Gets user's current deck configuration
        public string GetUserDeck(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var deck = _cardService.GetUserDeck(user.Id);
            if (!deck.Any())
                return CreateResponse(200, "No cards in deck");

            return CreateResponse(200, SerializeResponse(deck));
        }

        // Updates user's deck configuration
        public string ConfigureDeck(string authToken, string body)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            var cardIds = DeserializeBody<List<int>>(body, out error);
            if (cardIds == null)
                return CreateResponse(400, error);

            var result = _cardService.ConfigureDeck(user, cardIds);
            return result.StartsWith("Error")
                ? CreateResponse(400, result)
                : CreateResponse(200, result);
        }
    }
}
