using System.Collections.Concurrent;
using System.Text;
using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
{
    public class BattleService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDeckRepository _deckRepository;
        private readonly ConcurrentQueue<User> _battleQueue;
        private const int MAX_ROUNDS = 100;

        public BattleService(IUserRepository userRepository, IDeckRepository deckRepository)
        {
            _userRepository = userRepository;
            _deckRepository = deckRepository;
            _battleQueue = new ConcurrentQueue<User>();
        }

        public User FindOpponent(int userId)
        {
            if (_battleQueue.TryDequeue(out User opponent))
            {
                if (opponent.Id != userId) // Don't match with self
                    return opponent;

                // If we dequeued ourselves, add back to queue
                _battleQueue.Enqueue(opponent);
            }
            return null;
        }

        public void AddUserToQueue(User user)
        {
            _battleQueue.Enqueue(user);
        }

        public BattleResult ExecuteBattle(User player1, User player2)
        {
            var battleLog = new StringBuilder();
            battleLog.AppendLine($"Battle: {player1.Username} vs {player2.Username}\n");

            var p1Deck = _deckRepository.GetDeckByUserId(player1.Id);
            var p2Deck = _deckRepository.GetDeckByUserId(player2.Id);
            int roundCount = 0;

            while (roundCount < MAX_ROUNDS && p1Deck.Any() && p2Deck.Any())
            {
                roundCount++;
                battleLog.AppendLine($"\nRound {roundCount}:");

                // Get random cards for battle
                var p1Card = GetRandomCard(p1Deck);
                var p2Card = GetRandomCard(p2Deck);

                // Execute round
                var roundResult = ExecuteRound(p1Card, p2Card);
                battleLog.AppendLine(roundResult.Log);

                // Handle card transfer based on round result
                if (roundResult.Winner != null)
                {
                    TransferCard(roundResult.Winner == p1Card ? p1Deck : p2Deck,
                               roundResult.Winner == p1Card ? p2Deck : p1Deck,
                               roundResult.Loser);
                }

                // Check for early win condition
                if (!p1Deck.Any() || !p2Deck.Any())
                    break;
            }

            // Determine battle winner
            var winner = DetermineBattleWinner(p1Deck, p2Deck);
            battleLog.AppendLine($"\nBattle ended after {roundCount} rounds.");
            battleLog.AppendLine(GetBattleResultMessage(winner, player1, player2));

            return new BattleResult
            {
                Winner = winner,
                BattleLog = battleLog.ToString()
            };
        }

        private RoundResult ExecuteRound(Card card1, Card card2)
        {
            // Apply special rules first
            if (ApplySpecialRules(card1, card2, out RoundResult specialResult))
                return specialResult;

            // Calculate damage
            double damage1 = card1.CalculateDamage(card2);
            double damage2 = card2.CalculateDamage(card1);

            // Determine round winner
            if (damage1 > damage2)
                return new RoundResult(card1, card2, $"{card1.Name} defeats {card2.Name}");
            else if (damage2 > damage1)
                return new RoundResult(card2, card1, $"{card2.Name} defeats {card1.Name}");
            else
                return new RoundResult(null, null, "Round ends in a draw");
        }

        private bool ApplySpecialRules(Card card1, Card card2, out RoundResult result)
        {
            // Implement special rules from the specification
            if (card1 is MonsterCard monster1 && card2 is MonsterCard monster2)
            {
                // Goblins are too afraid of Dragons
                if (monster1.MonsterType == MonsterType.Goblin && monster2.MonsterType == MonsterType.Dragon)
                {
                    result = new RoundResult(card2, card1, "Goblin is too afraid of Dragon");
                    return true;
                }
                // ... implement other special rules
            }

            result = null;
            return false;
        }

        private Card GetRandomCard(List<Card> deck)
        {
            var random = new Random();
            int index = random.Next(deck.Count);
            return deck[index];
        }

        private void TransferCard(List<Card> winnerDeck, List<Card> loserDeck, Card cardToTransfer)
        {
            loserDeck.Remove(cardToTransfer);
            winnerDeck.Add(cardToTransfer);
        }

        private BattleWinner DetermineBattleWinner(List<Card> deck1, List<Card> deck2)
        {
            if (!deck1.Any() && !deck2.Any()) return BattleWinner.Draw;
            if (!deck2.Any()) return BattleWinner.Player1;
            if (!deck1.Any()) return BattleWinner.Player2;
            return BattleWinner.Draw; // If max rounds reached
        }

        private string GetBattleResultMessage(BattleWinner winner, User player1, User player2)
        {
            return winner switch
            {
                BattleWinner.Player1 => $"{player1.Username} wins the battle!",
                BattleWinner.Player2 => $"{player2.Username} wins the battle!",
                _ => "Battle ends in a draw!"
            };
        }
    }

    public class RoundResult
    {
        public Card Winner { get; }
        public Card Loser { get; }
        public string Log { get; }

        public RoundResult(Card winner, Card loser, string log)
        {
            Winner = winner;
            Loser = loser;
            Log = log;
        }
    }

    public class BattleResult
    {
        public BattleWinner Winner { get; set; }
        public string BattleLog { get; set; } = string.Empty;
    }

    public enum BattleWinner
    {
        Player1,
        Player2,
        Draw
    }
}
