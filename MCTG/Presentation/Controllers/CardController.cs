using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class CardController : BaseController
    {
        private readonly CardService _cardService;

        public CardController(CardService cardService, UserService userService)
            : base(userService)
        {
            _cardService = cardService;
        }

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

        // public string GetTradingDeals(string? authToken)
        // {
        //     if (string.IsNullOrEmpty(authToken))
        //         return "401 Unauthorized\nAccess token is missing or invalid";

        //     try
        //     {
        //         var trades = _cardService.GetTradingDeals();
        //         return $"200 OK\n{JsonSerializer.Serialize(trades)}";
        //     }
        //     catch (UnauthorizedAccessException)
        //     {
        //         return "401 Unauthorized\nAccess token is missing or invalid";
        //     }
        //     catch (Exception ex)
        //     {
        //         return $"400 Bad Request\n{ex.Message}";
        //     }
        // }

        /// <summary>
        /// Erstellt ein neues Handelsangebot
        /// </summary>
        /// <param name="authToken">Der Auth-Token des Users</param>
        /// <param name="body">JSON mit Trading-Deal Informationen</param>
        // public string CreateTradingDeal(string? authToken, string body)
        // {
        //     // 1. Überprüfe ob der User eingeloggt ist
        //     var (user, error) = AuthenticateUser(authToken);
        //     if (user == null)
        //         return error;

        //     try
        //     {
        //         // 2. Wandle den JSON-Body in ein TradingDeal-Objekt um
        //         var tradingDeal = DeserializeBody<TradingDeal>(body, out error);
        //         if (tradingDeal == null)
        //             return CreateResponse(400, error);

        //         // 3. Erstelle das Trading-Deal über den CardService
        //         var result = _cardService.CreateTradingDeal(user.Id, tradingDeal);

        //         // 4. Gib entsprechende Erfolgsmeldung oder Fehlermeldung zurück
        //         return result.StartsWith("Error")
        //             ? CreateResponse(400, result)
        //             : CreateResponse(201, "Trading deal successfully created");
        //     }
        //     catch (Exception ex)
        //     {
        //         return CreateResponse(400, ex.Message);
        //     }
        // }

        /// <summary>
        /// Löscht ein bestehendes Handelsangebot
        /// </summary>
        /// <param name="authToken">Der Auth-Token des Users</param>
        /// <param name="tradingId">ID des zu löschenden Handelsangebots</param>
        // public string DeleteTradingDeal(string? authToken, string tradingId)
        // {
        //     // 1. Überprüfe ob der User eingeloggt ist
        //     var (user, error) = AuthenticateUser(authToken);
        //     if (user == null)
        //         return error;

        //     try
        //     {
        //         // 2. Versuche das Trading-Deal zu löschen
        //         var result = _cardService.DeleteTradingDeal(user.Id, tradingId);

        //         // 3. Gib entsprechende Erfolgsmeldung oder Fehlermeldung zurück
        //         return result.StartsWith("Error")
        //             ? CreateResponse(400, result)
        //             : CreateResponse(200, "Trading deal successfully deleted");
        //     }
        //     catch (Exception ex)
        //     {
        //         return CreateResponse(400, ex.Message);
        //     }
        // }

        /// <summary>
        /// Führt einen Handel aus (akzeptiert ein Handelsangebot)
        /// </summary>
        /// <param name="authToken">Der Auth-Token des Users</param>
        /// <param name="tradingId">ID des Handelsangebots</param>
        /// <param name="body">JSON mit der CardId die getauscht werden soll</param>
        // public string ExecuteTrade(string? authToken, string tradingId, string body)
        // {
        //     // 1. Überprüfe ob der User eingeloggt ist
        //     var (user, error) = AuthenticateUser(authToken);
        //     if (user == null)
        //         return error;

        //     try
        //     {
        //         // 2. Hole die CardId aus dem Body
        //         var tradeRequest = DeserializeBody<TradeRequest>(body, out error);
        //         if (tradeRequest == null)
        //             return CreateResponse(400, error);

        //         // 3. Führe den Trade aus
        //         var result = _cardService.ExecuteTrade(user.Id, tradingId, tradeRequest.CardToTrade);

        //         // 4. Gib entsprechende Erfolgsmeldung oder Fehlermeldung zurück
        //         return result.StartsWith("Error")
        //             ? CreateResponse(400, result)
        //             : CreateResponse(200, "Trading deal successfully executed");
        //     }
        //     catch (Exception ex)
        //     {
        //         return CreateResponse(400, ex.Message);
        //     }
        // }
    }
}
