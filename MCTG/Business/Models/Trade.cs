namespace MCTG.Business.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int UserId { get; set; }
        public string? RequiredType { get; set; }  // "spell" or "monster"
        public int? MinimumDamage { get; set; }  // optional minimum damage
        public string Status { get; set; }

        public Trade(int cardId, string requiredType, int? minimumDamage = null)
        {
            CardId = cardId;
            RequiredType = requiredType.ToLower();
            MinimumDamage = minimumDamage;
            Status = "ACTIVE";
        }
    }
}