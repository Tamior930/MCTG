using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class BattleController : BaseController
    {
        private readonly BattleService _battleService;
        private readonly CardService _cardService;

        public BattleController(BattleService battleService, UserService userService, CardService cardService)
            : base(userService)
        {
            _battleService = battleService;
            _cardService = cardService;
        }

        public string HandleBattleRequest(string authToken)
        {
            // 1. Authenticate user
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            // 2. Check if user has a valid deck
            var userDeck = _cardService.GetUserDeck(user.Id);
            if (userDeck == null || !userDeck.Any())
                return CreateResponse(400, "Error: Please configure your deck before battling");

            try
            {
                // 3. Find or wait for opponent
                var result = _battleService.HandleBattle(user.Id);

                // 4. Return battle results
                return result.StartsWith("Error")
                    ? CreateResponse(400, result)
                    : CreateResponse(200, result);
            }
            catch (Exception ex)
            {
                return CreateResponse(500, $"Error: {ex.Message}");
            }
        }
    }
}
