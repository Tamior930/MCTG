namespace MCTG.Business.Models
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Coins { get; private set; }
        public int ELO { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public Stack Stack { get; private set; }
        public Deck Deck { get; private set; }
        public UserProfile Profile { get; set; }
        public Token? AuthToken { get; private set; }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Coins = 20;
            ELO = 100;
            Stack = new Stack();
            Deck = new Deck();
            Profile = new UserProfile("", "");
        }

        public bool HasValidToken()
        {
            return AuthToken != null && AuthToken.IsValid();
        }

        public void SetId(int id)
        {
            Id = id;
        }

        public void InitializeFromDatabase(int coins, int elo, int wins, int losses)
        {
            Coins = coins;
            ELO = elo;
            Wins = wins;
            Losses = losses;
        }

        public void AssignToken(Token token)
        {
            AuthToken = token;
        }

        public void UpdateCoins(int amount)
        {
            Coins += amount;
        }

        public void UpdateStats(bool won)
        {
            if (won)
            {
                Wins++;
                ELO += 3;
            }
            else
            {
                Losses++;
                ELO -= 5;
            }
        }
    }
}
