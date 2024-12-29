using System.Text;
using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.BusinessLayer.Controller
{
    public class BattleManager
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IUserRepository _userRepository;
        private const int MAX_ROUNDS = 100;

        public BattleManager(IDeckRepository deckRepository, IUserRepository userRepository)
        {
            _deckRepository = deckRepository;
            _userRepository = userRepository;
        }

        public string ExecuteBattle(User player1, User player2)
        {
            var battleLog = new StringBuilder();
            battleLog.AppendLine($"Battle: {player1.Username} vs {player2.Username}\n");

            int roundCount = 0;
            bool battleEnded = false;

            while (!battleEnded && roundCount < MAX_ROUNDS)
            {
                roundCount++;
                battleLog.AppendLine($"\nRound {roundCount}:");

                // Get random cards for the round
                Card p1Card = _deckRepository.GetRandomCardFromDeck(player1.Id);
                Card p2Card = _deckRepository.GetRandomCardFromDeck(player2.Id);

                if (p1Card == null || p2Card == null)
                {
                    battleLog.AppendLine("Battle ended - One player has no cards left.");
                    battleEnded = true;
                    continue;
                }

                // Calculate damages
                double p1Damage = p1Card.CalculateDamage(p2Card);
                double p2Damage = p2Card.CalculateDamage(p1Card);

                battleLog.AppendLine($"{player1.Username}'s {p1Card.Name} ({p1Damage:F1} damage) vs " +
                                   $"{player2.Username}'s {p2Card.Name} ({p2Damage:F1} damage)");

                // Determine round winner and transfer card
                if (p1Damage > p2Damage)
                {
                    _deckRepository.TransferCardBetweenDecks(p2Card.Id, player2.Id, player1.Id);
                    battleLog.AppendLine($"{player1.Username} wins the round!");
                }
                else if (p2Damage > p1Damage)
                {
                    _deckRepository.TransferCardBetweenDecks(p1Card.Id, player1.Id, player2.Id);
                    battleLog.AppendLine($"{player2.Username} wins the round!");
                }
                else
                {
                    battleLog.AppendLine("Round ended in a draw!");
                }

                // Check if either player has no cards left
                if (!_deckRepository.IsValidDeck(player1.Id) || !_deckRepository.IsValidDeck(player2.Id))
                {
                    battleEnded = true;
                }
            }

            // Determine battle winner and update stats
            if (roundCount >= MAX_ROUNDS)
            {
                battleLog.AppendLine("\nBattle ended in a draw (max rounds reached)!");
                return battleLog.ToString();
            }

            bool player1Won = _deckRepository.IsValidDeck(player1.Id);
            User winner = player1Won ? player1 : player2;
            User loser = player1Won ? player2 : player1;

            _userRepository.UpdateUserStats(winner.Id, true);
            _userRepository.UpdateUserStats(loser.Id, false);

            battleLog.AppendLine($"\nBattle ended! {winner.Username} wins!");
            return battleLog.ToString();
        }
    }
}
