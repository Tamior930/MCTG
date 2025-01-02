namespace MCTG.BusinessLayer.Models
{
    public class User(string username, string password)
    {
        private const int STARTING_COINS = 20;
        private const int STARTING_ELO = 100;
        private const int PACKAGE_COST = 5;
        private const int ELO_WIN_BONUS = 3;
        private const int ELO_LOSS_PENALTY = 5;

        public int Id { get; private set; } = -1;
        public string Username { get; private set; } = username;
        public string Password { get; private set; } = password;
        public int Coins { get; private set; } = STARTING_COINS;
        public Stack Stack { get; private set; } = new();
        public Deck Deck { get; private set; } = new();
        public Token AuthToken { get; private set; }
        public int ELO { get; private set; } = STARTING_ELO;
        public int Wins { get; private set; } = 0;
        public int Losses { get; private set; } = 0;
        public UserProfile Profile { get; private set; } = new("", "");

        public void SetId(int id)
        {
            if (Id == -1)
                Id = id;
        }

        public void AssignToken(Token token)
        {
            AuthToken = token;
        }

        public bool HasValidToken()
        {
            return AuthToken != null && AuthToken.IsValid();
        }

        public bool PurchasePackage(List<Card> package)
        {
            if (package.Count != 5)
                throw new ArgumentException("Package must contain exactly 5 cards");

            if (Coins < PACKAGE_COST)
                return false;

            Coins -= PACKAGE_COST;
            Stack.AddRange(package);
            return true;
        }

        public bool ConfigureDeck(List<Card> selectedCards)
        {
            if (selectedCards.Count != 4)
                return false;

            if (!selectedCards.All(card => Stack.Contains(card)))
                return false;

            try
            {
                Deck.SetDeck(selectedCards);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateBattleStats(bool won)
        {
            if (won)
            {
                ELO += ELO_WIN_BONUS;
                Wins++;
            }
            else
            {
                ELO = Math.Max(0, ELO - ELO_LOSS_PENALTY);
                Losses++;
            }
        }

        public bool CanTrade(Card card)
        {
            return Stack.IsCardAvailableForTrade(card, Deck);
        }

        public void UpdateStats(int coins, int elo, int wins, int losses)
        {
            Coins = coins;
            ELO = elo;
            Wins = wins;
            Losses = losses;
        }

        public void InitializeFromDatabase(int coins, int elo, int wins, int losses)
        {
            Coins = coins;
            ELO = elo;
            Wins = wins;
            Losses = losses;
        }

        public bool HasValidDeckSize()
        {
            return Deck.IsValid();
        }

        public bool CanAffordPackage()
        {
            return Coins >= PACKAGE_COST;
        }

        public int GetStackSize()
        {
            return Stack.Count;
        }

        public bool HasCardInStack(Card card)
        {
            return Stack.Contains(card);
        }

        public void UpdateELO(bool won)
        {
            if (won)
            {
                ELO += ELO_WIN_BONUS;
                Wins++;
            }
            else
            {
                ELO = Math.Max(0, ELO - ELO_LOSS_PENALTY);
                Losses++;
            }
        }
    }
}
