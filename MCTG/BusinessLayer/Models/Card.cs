using MCTG.BusinessLayer.Interfaces;

namespace MCTG.BusinessLayer.Models
{
    public enum CardType
    {
        Monster,
        Spell
    }

    public abstract class Card : ICard
    {
        public int Id { get; set; }
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public CardType Type { get; private set; }
        public ElementType ElementType { get; private set; }

        protected Card(int id, string name, int damage, ElementType elementType, CardType type)
        {
            Id = id;
            Name = name;
            Damage = damage;
            ElementType = elementType;
            Type = type;
        }

        public virtual double CalculateDamage(ICard opponent)
        {
            return Damage;
        }

        protected double CalculateElementalDamage(ElementType opponentElement)
        {
            // Multiplier for damage calculation
            double damageMultiplier = 1.0;

            // Check for effective combinations (2x damage)
            if (ElementType == ElementType.Water && opponentElement == ElementType.Fire)
            {
                damageMultiplier = 2.0; // Water is effective against Fire
            }
            else if (ElementType == ElementType.Fire && opponentElement == ElementType.Normal)
            {
                damageMultiplier = 2.0; // Fire is effective against Normal
            }
            else if (ElementType == ElementType.Normal && opponentElement == ElementType.Water)
            {
                damageMultiplier = 2.0; // Normal is effective against Water
            }
            // Check for not effective combinations (0.5x damage)
            else if (ElementType == ElementType.Fire && opponentElement == ElementType.Water)
            {
                damageMultiplier = 0.5; // Fire is not effective against Water
            }
            else if (ElementType == ElementType.Normal && opponentElement == ElementType.Fire)
            {
                damageMultiplier = 0.5; // Normal is not effective against Fire
            }
            else if (ElementType == ElementType.Water && opponentElement == ElementType.Normal)
            {
                damageMultiplier = 0.5; // Water is not effective against Normal
            }

            // Calculate final damage using the multiplier
            return Damage * damageMultiplier;
        }
    }
}
