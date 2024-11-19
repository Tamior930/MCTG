using MCTG.BusinessLayer.Models;
using System.Text;

namespace MCTG.BusinessLayer.Controller
{
    public class BattleManager
    {
        private const int MAX_ROUNDS = 50;

        public string ExecuteBattle(User player1, User player2)
        {
            if (!player1.HasValidDeckSize() || !player2.HasValidDeckSize())
            {
                return "Battle cannot start. Both players must have exactly 4 cards in their deck.";
            }

            var battleLog = new StringBuilder();
            battleLog.AppendLine($"Battle: {player1.Username} vs {player2.Username}\n");

            int roundCount = 0;
            var player1Deck = new List<Card>(player1.Deck.Cards);
            var player2Deck = new List<Card>(player2.Deck.Cards);

            while (player1Deck.Any() && player2Deck.Any() && roundCount < MAX_ROUNDS)
            {
                roundCount++;
                battleLog.AppendLine($"Round {roundCount}:");

                // Randomly select cards
                var random = new Random();
                var player1Card = player1Deck[random.Next(player1Deck.Count)];
                var player2Card = player2Deck[random.Next(player2Deck.Count)];

                battleLog.AppendLine($"{player1.Username}: {player1Card.Name} ({player1Card.ElementType}, {player1Card.Damage} damage)");
                battleLog.AppendLine($"{player2.Username}: {player2Card.Name} ({player2Card.ElementType}, {player2Card.Damage} damage)");

                // Check special rules
                if (CheckSpecialRules(player1Card, player2Card, battleLog, out var winner))
                {
                    HandleRoundResult(winner, player1Card, player2Card, player1Deck, player2Deck, battleLog);
                    continue;
                }

                // Calculate damage
                double damage1 = player1Card.CalculateDamage(player2Card);
                double damage2 = player2Card.CalculateDamage(player1Card);

                battleLog.AppendLine($"Effective Damage - {player1.Username}: {damage1}, {player2.Username}: {damage2}");

                // Determine round winner
                if (damage1 > damage2)
                {
                    HandleRoundResult(1, player1Card, player2Card, player1Deck, player2Deck, battleLog);
                }
                else if (damage2 > damage1)
                {
                    HandleRoundResult(2, player1Card, player2Card, player1Deck, player2Deck, battleLog);
                }
                else
                {
                    battleLog.AppendLine("Round ended in a draw - no cards exchanged\n");
                }
            }

            // Determine battle winner and update stats
            UpdateBattleResults(player1, player2, player1Deck, player2Deck, battleLog);

            return battleLog.ToString();
        }

        private bool CheckSpecialRules(Card card1, Card card2, StringBuilder log, out int winner)
        {
            winner = 0;
            int tempWinner = 0;

            // Create helper method for checking card names
            bool CheckCardPair(string name1, string name2, Card c1, Card c2, string message, bool firstWins)
            {
                if ((c1.Name.Contains(name1) && c2.Name.Contains(name2)) ||
                    (c2.Name.Contains(name1) && c1.Name.Contains(name2)))
                {
                    log.AppendLine(message);
                    tempWinner = (c1.Name.Contains(name1) == firstWins) ? 1 : 2;
                    return true;
                }
                return false;
            }

            // Simplified special rules checks
            if (CheckCardPair("Goblin", "Dragon", card1, card2, 
                "Goblins are too afraid of Dragons to attack!", false))
            {
                winner = tempWinner;
                return true;
            }

            if (CheckCardPair("Wizard", "Ork", card1, card2,
                "Wizards control Orks, making them unable to attack!", true))
            {
                winner = tempWinner;
                return true;
            }

            if ((card1.Name.Contains("Knight") && card2 is SpellCard && card2.ElementType == ElementType.Water) ||
                (card2.Name.Contains("Knight") && card1 is SpellCard && card1.ElementType == ElementType.Water))
            {
                log.AppendLine("Knight's heavy armor causes them to drown in WaterSpells!");
                winner = card1.Name.Contains("Knight") ? 2 : 1;
                return true;
            }

            if (CheckCardPair("Kraken", "Spell", card1, card2,
                "Kraken is immune to spells!", true))
            {
                winner = tempWinner;
                return true;
            }

            if (CheckCardPair("FireElf", "Dragon", card1, card2,
                "FireElves evade Dragon attacks!", true))
            {
                winner = tempWinner;
                return true;
            }

            return false;
        }

        private void HandleRoundResult(int winner, Card card1, Card card2, List<Card> deck1, List<Card> deck2, StringBuilder log)
        {
            if (winner == 1)
            {
                deck2.Remove(card2);
                deck1.Add(card2);
                log.AppendLine($"Card {card2.Name} moves to deck 1\n");
            }
            else if (winner == 2)
            {
                deck1.Remove(card1);
                deck2.Add(card1);
                log.AppendLine($"Card {card1.Name} moves to deck 2\n");
            }
        }

        private void UpdateBattleResults(User player1, User player2, List<Card> finalDeck1, List<Card> finalDeck2, StringBuilder log)
        {
            if (finalDeck1.Count > finalDeck2.Count)
            {
                log.AppendLine($"\nWinner: {player1.Username}!");
                player1.UpdateELO(true);
                player2.UpdateELO(false);
            }
            else if (finalDeck2.Count > finalDeck1.Count)
            {
                log.AppendLine($"\nWinner: {player2.Username}!");
                player2.UpdateELO(true);
                player1.UpdateELO(false);
            }
            else
            {
                log.AppendLine("\nBattle ended in a draw!");
            }
        }
    }
}
