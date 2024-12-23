namespace MCTG.BusinessLayer.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int UserId { get; set; }
        public string RequiredType { get; set; }  // "monster" or "spell"
        public ElementType? RequiredElementType { get; set; }
        public int MinimumDamage { get; set; }
        public bool IsActive { get; set; }

        public Trade(int cardId, int userId, string requiredType, ElementType? requiredElementType, int minimumDamage)
        {
            CardId = cardId;
            UserId = userId;
            RequiredType = requiredType;
            RequiredElementType = requiredElementType;
            MinimumDamage = minimumDamage;
            IsActive = true;
        }
    }
} 