namespace MCTG.BusinessLayer.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public int CardId { get; }
        public int UserId { get; set; }
        
        // Mandatory requirement
        public string RequiredType { get; }  // "Spell" or "Monster"
        
        // Optional requirements (at least one must be set)
        public string RequiredElementType { get; }   // "Fire", "Water", "Normal"
        public string RequiredMonsterType { get; }   // Specific monster type
        public int MinimumDamage { get; }            // Minimum damage requirement
        
        public bool IsActive { get; set; }

        public Trade(int id, int cardId, int userId, string requiredType, string requiredElementType, string requiredMonsterType, int minimumDamage)
        {
            Id = id;
            CardId = cardId;
            UserId = userId;
            RequiredType = requiredType;
            RequiredElementType = requiredElementType;
            RequiredMonsterType = requiredMonsterType;
            MinimumDamage = minimumDamage;
            IsActive = true;
        }
    }
}