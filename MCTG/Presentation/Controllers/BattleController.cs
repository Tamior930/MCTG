using MCTG.Presentation.Services;

namespace MCTG.Presentation.Controllers
{
    public class BattleController : BaseController
    {
        private readonly BattleService _battleService;

        public BattleController(BattleService battleService, UserService userService)
            : base(userService)
        {
            _battleService = battleService;
        }

        // Handles battle requests and matchmaking
        public string HandleBattleRequest(string authToken)
        {
            var (user, error) = AuthenticateUser(authToken);
            if (user == null)
                return error;

            try
            {
                var result = _battleService.HandleBattle(user);

                if (result.StartsWith("Error"))
                    return CreateResponse(400, result);
                if (result == "Waiting for opponent...")
                    return CreateResponse(202, result);

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
