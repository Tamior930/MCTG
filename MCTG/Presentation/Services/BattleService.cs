using System.Collections.Concurrent;
using System.Text;
using MCTG.Business.Models;
using MCTG.Data.Interfaces;

namespace MCTG.Presentation.Services
{
    public class BattleService
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;
        private static readonly ConcurrentQueue<BattleRequest> _waitingPlayers = new ConcurrentQueue<BattleRequest>();
        private const int MAX_ROUNDS = 100;

        public class BattleRequest
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public List<Card> Deck { get; set; }
            public DateTime Timestamp { get; set; }

            public BattleRequest(int userId, string username, List<Card> deck)
            {
                UserId = userId;
                Username = username;
                Deck = deck;
                Timestamp = DateTime.Now;
            }

            public bool IsExpired() => (DateTime.Now - Timestamp).TotalMinutes > 5;
        }

        public BattleService(IDeckRepository deckRepository, IUserRepository userRepository, ICardRepository cardRepository)
        {
            _deckRepository = deckRepository;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
        }

        public string HandleBattle(User user)
        {
            // Validate deck
            var userDeck = _deckRepository.GetDeckCards(user.Id);
            if (userDeck.Count != 4)
                return "Error: Invalid deck configuration";

            // Clean expired requests
            CleanupExpiredRequests();

            // Try to find opponent
            if (_waitingPlayers.TryDequeue(out var opponent))
            {
                if (opponent.UserId == user.Id)
                {
                    _waitingPlayers.Enqueue(opponent); // Put back in queue
                    return "Error: Cannot battle against yourself";
                }
                return ExecuteBattle(
                    new BattleRequest(user.Id, user.Username, userDeck),
                    opponent
                );
            }

            // No opponent found, add to queue
            _waitingPlayers.Enqueue(new BattleRequest(user.Id, user.Username, userDeck));
            return "Waiting for opponent...";
        }

        private string ExecuteBattle(BattleRequest player1, BattleRequest player2)
        {
            var battleLog = new StringBuilder();
            var random = new Random();
            var player1Deck = new List<Card>(player1.Deck);
            var player2Deck = new List<Card>(player2.Deck);

            battleLog.AppendLine($"Battle: {player1.Username} vs {player2.Username}");
            int roundCount = 0;

            while (player1Deck.Any() && player2Deck.Any() && roundCount < MAX_ROUNDS)
            {
                roundCount++;
                battleLog.AppendLine($"\nRound {roundCount}:");

                // Select random cards
                var card1 = player1Deck[random.Next(player1Deck.Count)];
                var card2 = player2Deck[random.Next(player2Deck.Count)];

                // Log the cards being played
                battleLog.AppendLine($"{player1.Username}'s {card1.Name} ({card1.Damage}) vs {player2.Username}'s {card2.Name} ({card2.Damage})");

                // Calculate damage using the cards' built-in logic
                double damage1 = card1.CalculateDamage(card2);
                double damage2 = card2.CalculateDamage(card1);

                battleLog.AppendLine($"Calculated Damage: {damage1} vs {damage2}");

                // Determine round winner
                if (damage1 > damage2)
                {
                    battleLog.AppendLine($"{player1.Username} wins round with {card1.Name}!");
                    TransferCard(card2, player2Deck, player1Deck, player1, player2);
                }
                else if (damage2 > damage1)
                {
                    battleLog.AppendLine($"{player2.Username} wins round with {card2.Name}!");
                    TransferCard(card1, player1Deck, player2Deck, player2, player1);
                }
                else
                {
                    battleLog.AppendLine("Round is a draw!");
                }

                battleLog.AppendLine($"Cards remaining - {player1.Username}: {player1Deck.Count}, {player2.Username}: {player2Deck.Count}");
            }

            // Determine battle winner and update stats
            UpdateBattleResults(player1, player2, player1Deck.Count, player2Deck.Count);

            // Add final battle result to log
            if (player1Deck.Count > player2Deck.Count)
                battleLog.AppendLine($"\nBattle Winner: {player1.Username}!");
            else if (player2Deck.Count > player1Deck.Count)
                battleLog.AppendLine($"\nBattle Winner: {player2.Username}!");
            else
                battleLog.AppendLine("\nBattle ended in a draw!");

            return battleLog.ToString();
        }

        private bool TransferCard(Card card, List<Card> fromDeck, List<Card> toDeck, BattleRequest fromPlayer, BattleRequest toPlayer)
        {
            if (fromDeck.Remove(card))
            {
                if (_cardRepository.UpdateCardOwnership(card, toPlayer.UserId))
                {
                    toDeck.Add(card);
                    return true;
                }
            }
            return false;
        }

        private void UpdateBattleResults(BattleRequest player1, BattleRequest player2, int deck1Count, int deck2Count)
        {
            if (deck1Count == deck2Count) return; // Draw, no stats update

            bool player1Won = deck1Count > deck2Count;
            _userRepository.UpdateUserStats(player1.UserId.ToString(), player1Won);
            _userRepository.UpdateUserStats(player2.UserId.ToString(), !player1Won);
        }

        private void CleanupExpiredRequests()
        {
            var currentRequests = new List<BattleRequest>();
            while (_waitingPlayers.TryDequeue(out var request))
            {
                if (!request.IsExpired())
                    currentRequests.Add(request);
            }
            foreach (var request in currentRequests)
            {
                _waitingPlayers.Enqueue(request);
            }
        }
    }
}