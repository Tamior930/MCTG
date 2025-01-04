using System.Collections.Concurrent;
using System.Text;
using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
{
    public class BattleService
    {
        private readonly IDeckRepository _deckRepository;
        private static readonly ConcurrentQueue<int> _waitingPlayers = new ConcurrentQueue<int>();
        private const int MAX_ROUNDS = 100;

        public BattleService(IDeckRepository deckRepository)
        {
            _deckRepository = deckRepository;
        }

        public string HandleBattle(int userId)
        {
            // 1. Check if there's already a waiting player
            if (_waitingPlayers.TryDequeue(out int opponentId))
            {
                if (opponentId == userId)
                {
                    _waitingPlayers.Enqueue(opponentId); // Put them back in queue
                    return "Error: Cannot battle against yourself";
                }
                return ExecuteBattle(userId, opponentId);
            }

            // 2. No opponent available, add user to waiting queue
            _waitingPlayers.Enqueue(userId);
            return "Error: No opponent available. Added to waiting queue.";
        }

        private string ExecuteBattle(int player1Id, int player2Id)
        {
            var player1Deck = new List<Card>(_deckRepository.GetDeckCards(player1Id));
            var player2Deck = new List<Card>(_deckRepository.GetDeckCards(player2Id));
            var battleLog = new StringBuilder();
            var random = new Random();

            battleLog.AppendLine("Battle started!");
            int roundCount = 0;

            while (player1Deck.Any() && player2Deck.Any() && roundCount < MAX_ROUNDS)
            {
                roundCount++;
                battleLog.AppendLine($"\nRound {roundCount}:");

                // Select random cards for the round
                var player1Card = player1Deck[random.Next(player1Deck.Count)];
                var player2Card = player2Deck[random.Next(player2Deck.Count)];

                // Calculate damages
                double damage1 = player1Card.CalculateDamage(player2Card);
                double damage2 = player2Card.CalculateDamage(player1Card);

                // Log the round
                battleLog.AppendLine($"Player 1: {player1Card.Name} ({damage1} damage) vs Player 2: {player2Card.Name} ({damage2} damage)");

                // Determine round winner and transfer cards
                if (damage1 > damage2)
                {
                    battleLog.AppendLine("Player 1 wins the round!");
                    TransferCard(player2Card, player2Deck, player1Deck);
                }
                else if (damage2 > damage1)
                {
                    battleLog.AppendLine("Player 2 wins the round!");
                    TransferCard(player1Card, player1Deck, player2Deck);
                }
                else
                {
                    battleLog.AppendLine("Round ended in a draw!");
                }
            }

            // Determine battle winner
            string result = DetermineBattleResult(player1Deck, player2Deck, roundCount, battleLog);

            // Save updated decks
            _deckRepository.SaveDeck(player1Id, player1Deck);
            _deckRepository.SaveDeck(player2Id, player2Deck);

            return result;
        }

        private void TransferCard(Card card, List<Card> fromDeck, List<Card> toDeck)
        {
            fromDeck.Remove(card);
            toDeck.Add(card);
        }

        private string DetermineBattleResult(List<Card> deck1, List<Card> deck2, int rounds, StringBuilder log)
        {
            if (rounds >= MAX_ROUNDS)
            {
                log.AppendLine("\nBattle ended in a draw (maximum rounds reached)");
                return log.ToString();
            }

            if (!deck1.Any())
            {
                log.AppendLine("\nPlayer 2 wins the battle!");
                return log.ToString();
            }

            if (!deck2.Any())
            {
                log.AppendLine("\nPlayer 1 wins the battle!");
                return log.ToString();
            }

            log.AppendLine("\nBattle ended in a draw");
            return log.ToString();
        }
    }
}