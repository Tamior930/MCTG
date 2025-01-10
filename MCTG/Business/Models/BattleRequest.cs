namespace MCTG.Business.Models
{
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
}