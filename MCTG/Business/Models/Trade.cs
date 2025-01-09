namespace MCTG.Business.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int UserId { get; set; }
        public string RequiredType { get; set; }
        public string? RequiredElementType { get; set; }
        public string? RequiredMonsterType { get; set; }
        public int MinimumDamage { get; set; }
        public string Status { get; set; } = "ACTIVE";

        public Trade(int id, int cardId, int userId, string requiredType, string? requiredElementType,
            string? requiredMonsterType, int minimumDamage, bool isActive)
        {
            Id = id;
            CardId = cardId;
            UserId = userId;
            RequiredType = requiredType;
            RequiredElementType = requiredElementType;
            RequiredMonsterType = requiredMonsterType;
            MinimumDamage = minimumDamage;
            Status = isActive ? "ACTIVE" : "COMPLETED";
        }
    }
}