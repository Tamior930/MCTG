namespace MCTG.BusinessLayer.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int UserId { get; set; }
        public CardType RequiredCardType { get; set; }  // Monster or Spell
        public ElementType? RequiredElementType { get; set; }
        public MonsterType? RequiredMonsterType { get; set; }
        public int MinimumDamage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Trade(int cardId, int userId, CardType requiredCardType, ElementType? requiredElementType,
                    MonsterType? requiredMonsterType, int minimumDamage)
        {
            CardId = cardId;
            UserId = userId;
            RequiredCardType = requiredCardType;
            RequiredElementType = requiredElementType;
            RequiredMonsterType = requiredMonsterType;
            MinimumDamage = minimumDamage;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsValidTradeCard(Card offeredCard)
        {
            if (offeredCard.Damage < MinimumDamage)
                return false;

            if (offeredCard.Type != RequiredCardType)
                return false;

            if (RequiredElementType.HasValue && offeredCard.ElementType != RequiredElementType.Value)
                return false;

            if (RequiredMonsterType.HasValue && offeredCard is MonsterCard monsterCard)
                return monsterCard.MonsterType == RequiredMonsterType.Value;

            return true;
        }
    }
}