using MCTG.BusinessLayer.Models;
using MCTG.PresentationLayer.Services;

namespace MCTG.PresentationLayer.Controller
{
    public class BattleController : BaseController
    {
        private readonly BattleService _battleService;

        public BattleController(BattleService battleService, UserService userService)
            : base(userService)
        {
            _battleService = battleService;
        }

        public string HandleBattleRequest(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            try
            {
                if (!user.HasValidDeckSize())
                    return CreateResponse(400, "Your deck must contain exactly 4 cards.");

                var opponent = _battleService.FindOpponent(user.Id);
                if (opponent == null)
                {
                    _battleService.AddUserToQueue(user);
                    return CreateResponse(200, "Waiting for opponent...");
                }

                var battleResult = _battleService.ExecuteBattle(user, opponent);
                UpdateBattleStats(user, opponent, battleResult);

                return CreateResponse(200, battleResult.BattleLog);
            }
            catch (Exception ex)
            {
                return CreateResponse(500, $"Battle error: {ex.Message}");
            }
        }

        private void UpdateBattleStats(User player1, User player2, BattleResult result)
        {
            switch (result.Winner)
            {
                case BattleWinner.Player1:
                    _userService.UpdateStats(player1.Id, true);  // Win
                    _userService.UpdateStats(player2.Id, false); // Loss
                    break;
                case BattleWinner.Player2:
                    _userService.UpdateStats(player1.Id, false); // Loss
                    _userService.UpdateStats(player2.Id, true);  // Win
                    break;
                case BattleWinner.Draw:
                    // No ELO changes on draw
                    break;
            }
        }
    }
}
