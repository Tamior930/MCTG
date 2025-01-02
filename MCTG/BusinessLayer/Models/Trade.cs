namespace MCTG.BusinessLayer.Models
{
    public class TradingDealRequest
    {
        public int CardToTrade { get; set; }
        public CardType RequiredCardType { get; set; }
        public ElementType? RequiredElementType { get; set; }
        public MonsterType? RequiredMonsterType { get; set; }
        public int MinimumDamage { get; set; }
    }

    public class TradeOffer
    {
        public int CardId { get; set; }
    }

    public class Trade
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public int UserId { get; set; }
        public string RequiredType { get; set; }
        public int MinimumDamage { get; set; }
        public bool IsActive { get; set; }

        public Trade(string cardId, int userId, string requiredType, int minimumDamage)
        {
            CardId = cardId;
            UserId = userId;
            RequiredType = requiredType;
            MinimumDamage = minimumDamage;
            IsActive = true;
        }
    }
}