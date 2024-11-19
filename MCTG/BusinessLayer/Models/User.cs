namespace MCTG.BusinessLayer.Models
{
    public class User
    {
        private const int STARTING_COINS = 20;
        private const int STARTING_ELO = 100;
        private const int PACKAGE_COST = 5;
        private const int ELO_WIN_BONUS = 3;
        private const int ELO_LOSS_PENALTY = 5;

        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Coins { get; private set; }
        public Stack Stack { get; private set; }
        public Deck Deck { get; private set; }
        public Token AuthToken { get; private set; }
        public int ELO { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }

        public User(int id, string username, string password, int coins, int elo, int wins, int losses)
        {
            Id = id;
            Username = username;
            Password = password;
            Coins = coins;
            Stack = new Stack();
            Deck = new Deck();
            ELO = elo;
            Wins = wins;
            Losses = losses;
        }

        public User(string username, string password)
            : this(-1, username, password, STARTING_COINS, STARTING_ELO, 0, 0)
        {
        }

        public User(string username, string password, int coins, int elo, int wins, int losses)
            : this(-1, username, password, coins, elo, wins, losses)
        {
        }

        public void SetId(int id)
        {
            if (Id == -1)
            {
                Id = id;
            }
        }

        public void AssignToken(Token token)
        {
            AuthToken = token;
        }

        public bool HasValidToken()
        {
            return AuthToken != null && AuthToken.IsValid();
        }

        // Package Purchase Methods
        public bool CanAffordPackage()
        {
            return Coins >= PACKAGE_COST;
        }

        public bool PurchasePackage(List<Card> package)
        {
            if (!CanAffordPackage())
            {
                return false;
            }

            DeductCoins(PACKAGE_COST);
            Stack.AddRange(package);
            return true;
        }

        private void DeductCoins(int amount)
        {
            Coins -= amount;
        }

        // Battle Statistics Methods
        public void UpdateELO(bool won)
        {
            if (won)
            {
                ELO += ELO_WIN_BONUS;
                Wins++;
            }
            else
            {
                ELO = Math.Max(0, ELO - ELO_LOSS_PENALTY); // Prevent negative ELO
                Losses++;
            }
        }

        public int GetTotalGamesPlayed()
        {
            return Wins + Losses;
        }

        public double GetWinRate()
        {
            int totalGames = GetTotalGamesPlayed();
            return totalGames > 0 ? (double)Wins / totalGames : 0;
        }

        // Deck Management Methods
        public bool TryAddCardToDeck(Card card)
        {
            if (!Stack.Contains(card))
            {
                return false;
            }

            try
            {
                Deck.AddCard(card);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryRemoveCardFromDeck(Card card)
        {
            Deck.RemoveCard(card);
            return true;
        }

        public bool HasValidDeckSize()
        {
            return Deck.Cards.Count == 4;
        }

        // Add this new method
        public bool IsCardAvailableForTrade(Card card)
        {
            return Stack.IsCardAvailableForTrade(card, Deck);
        }
    }
}
