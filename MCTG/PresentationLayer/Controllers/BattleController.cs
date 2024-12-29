using MCTG.BusinessLayer.Controller;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class BattleController
    {
        private readonly BattleManager _battleManager;
        private readonly UserService _userService;

        public BattleController(BattleManager battleManager, UserService userService)
        {
            _battleManager = battleManager;
            _userService = userService;
        }

        public string HandleBattleRequest(string authToken, string opponentUsername)
        {
            // Get users from token and username
            var player1 = _userService.GetUserByToken(authToken);
            var player2 = _userService.GetUserByUsername(opponentUsername);

            if (player1 == null || player2 == null)
            {
                return "Error: One or both players not found.";
            }

            if (!player1.HasValidDeckSize() || !player2.HasValidDeckSize())
            {
                return "Error: Both players must have exactly 4 cards in their deck.";
            }

            return _battleManager.ExecuteBattle(player1, player2);
        }
    }
}
