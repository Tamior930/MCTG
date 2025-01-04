using MCTG.BusinessLayer.Interfaces;

namespace MCTG.BusinessLayer.Models
{
    public enum ElementType
    {
        Normal,
        Fire,
        Water
    }

    public class SpellCard : Card
    {
        public SpellCard(int id, string name, int damage, ElementType elementType)
            : base(id, name, damage, elementType, CardType.Spell)
        {
        }

        public override double CalculateDamage(ICard opponent)
        {
            // Against monsters
            if (opponent is MonsterCard monsterCard)
            {
                // Kraken is immune to spells
                if (monsterCard.MonsterType == MonsterType.Kraken)
                    return 0;

                // Knights drown instantly against water spells
                if (monsterCard.MonsterType == MonsterType.Knight && ElementType == ElementType.Water)
                    return double.MaxValue;
            }

            // Spell vs Spell or regular monster damage calculation
            return CalculateElementalDamage(opponent.ElementType);
        }
    }
}