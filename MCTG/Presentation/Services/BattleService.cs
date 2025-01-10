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

        public BattleService(IDeckRepository deckRepository, IUserRepository userRepository, ICardRepository cardRepository)
        {
            _deckRepository = deckRepository;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
        }

        public string HandleBattle(User user)
        {
            // Get user's deck and validate it has exactly 4 cards
            var userDeck = _deckRepository.GetDeckCards(user.Id);
            if (userDeck.Count != 4)
                return "Error: Invalid deck configuration";

            // Remove any expired battle requests
            CleanupExpiredRequests();

            // Try to find an opponent in the waiting queue
            if (_waitingPlayers.TryDequeue(out var opponent))
            {
                // Prevent self-battles
                if (opponent.UserId == user.Id)
                {
                    _waitingPlayers.Enqueue(opponent);
                    return "Error: Cannot battle against yourself";
                }
                return ExecuteBattle(
                    new BattleRequest(user.Id, user.Username, userDeck),
                    opponent
                );
            }

            // No opponent found, add user to waiting queue
            _waitingPlayers.Enqueue(new BattleRequest(user.Id, user.Username, userDeck));
            return "Waiting for opponent...";
        }

        private string ExecuteBattle(BattleRequest player1, BattleRequest player2)
        {
            var battleLog = new StringBuilder();
            var random = new Random();
            // Create copies of decks to modify during battle
            var player1Deck = new List<Card>(player1.Deck);
            var player2Deck = new List<Card>(player2.Deck);

            battleLog.AppendLine($"Battle: {player1.Username} vs {player2.Username}");
            int roundCount = 0;

            // Continue battle until a player runs out of cards or max rounds reached
            while (player1Deck.Any() && player2Deck.Any() && roundCount < MAX_ROUNDS)
            {
                roundCount++;
                battleLog.AppendLine($"\nRound {roundCount}:");

                // Randomly select cards for battle
                var card1 = player1Deck[random.Next(player1Deck.Count)];
                var card2 = player2Deck[random.Next(player2Deck.Count)];

                // Log the card matchup
                battleLog.AppendLine($"{player1.Username}'s {card1.Name} ({card1.Damage}) vs {player2.Username}'s {card2.Name} ({card2.Damage})");

                // Calculate effective damage considering special rules
                double damage1 = card1.CalculateDamage(card2);
                double damage2 = card2.CalculateDamage(card1);

                battleLog.AppendLine($"Calculated Damage: {damage1} vs {damage2}");

                // Determine round winner and handle card transfers
                if (damage1 > damage2)
                {
                    battleLog.AppendLine($"{player1.Username} wins round with {card1.Name}!");
                    HandleRoundWin(card1, card2, player1Deck, player2Deck, player1);
                }
                else if (damage2 > damage1)
                {
                    battleLog.AppendLine($"{player2.Username} wins round with {card2.Name}!");
                    HandleRoundWin(card2, card1, player2Deck, player1Deck, player2);
                }
                else
                {
                    battleLog.AppendLine("Round is a draw!");
                }

                // Log remaining cards for each player
                battleLog.AppendLine($"Cards remaining - {player1.Username}: {player1Deck.Count}, {player2.Username}: {player2Deck.Count}");
            }

            // Update player stats based on battle outcome
            UpdateBattleResults(player1, player2, player1Deck.Count, player2Deck.Count);

            // Determine and log final battle winner
            if (player1Deck.Count > player2Deck.Count)
            {
                battleLog.AppendLine($"\nBattle Winner: {player1.Username}!");
            }
            else if (player2Deck.Count > player1Deck.Count)
            {
                battleLog.AppendLine($"\nBattle Winner: {player2.Username}!");
            }
            else
            {
                battleLog.AppendLine("\nBattle ended in a draw!");
            }

            return battleLog.ToString();
        }

        private void HandleRoundWin(Card winningCard, Card losingCard, List<Card> winnerDeck, List<Card> loserDeck, BattleRequest winner)
        {
            try
            {
                // Transfer losing card to winner's deck and update database
                if (TransferCard(losingCard, loserDeck, winnerDeck, winner))
                {
                    // Refresh winning card position in deck
                    winnerDeck.Remove(winningCard);
                    winnerDeck.Add(winningCard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling round win: {ex.Message}");
            }
        }

        private bool TransferCard(Card card, List<Card> fromDeck, List<Card> toDeck, BattleRequest toPlayer)
        {
            try
            {
                // Remove card from loser's deck
                if (!fromDeck.Remove(card))
                    return false;

                // Update card ownership in database
                if (_cardRepository.UpdateCardOwnership(card, toPlayer.UserId))
                {
                    // Add card to winner's deck
                    toDeck.Add(card);
                    return true;
                }
                else
                {
                    // Revert changes if database update fails
                    fromDeck.Add(card);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error transferring card: {ex.Message}");
                fromDeck.Add(card);
                return false;
            }
        }

        private void UpdateBattleResults(BattleRequest player1, BattleRequest player2, int deck1Count, int deck2Count)
        {
            if (deck1Count == deck2Count) return;

            try
            {
                // Retrieve current user data
                var user1 = _userRepository.GetUserById(player1.UserId);
                var user2 = _userRepository.GetUserById(player2.UserId);

                if (user1 == null || user2 == null)
                {
                    Console.WriteLine("Error: Could not find users for battle results update");
                    return;
                }

                // Determine winner and update stats accordingly
                bool player1Won = deck1Count > deck2Count;

                // Update stats in database
                _userRepository.UpdateUserStats(user1.AuthToken.Value, player1Won);
                _userRepository.UpdateUserStats(user2.AuthToken.Value, !player1Won);

                // Update local user objects
                if (player1Won)
                {
                    user1.UpdateStats(true);
                    user2.UpdateStats(false);
                }
                else
                {
                    user1.UpdateStats(false);
                    user2.UpdateStats(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating battle results: {ex.Message}");
            }
        }

        private void CleanupExpiredRequests()
        {
            // Create a list to hold the non-expired requests
            var currentRequests = new List<BattleRequest>();

            // Dequeue all requests from the waiting queue
            while (_waitingPlayers.TryDequeue(out var request))
            {
                // Check if the request is not expired
                if (!request.IsExpired())
                    // Add the non-expired request to the list
                    currentRequests.Add(request);
            }

            // Re-enqueue all valid (non-expired) requests back into the waiting queue
            foreach (var request in currentRequests)
            {
                _waitingPlayers.Enqueue(request);
            }
        }
    }
}