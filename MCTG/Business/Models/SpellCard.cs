using MCTG.Business.Interfaces;

namespace MCTG.Business.Models
{
    public class SpellCard : Card
    {
        public SpellCard(int id, string name, int damage, ElementType elementType)
            : base(id, name, damage, elementType, CardType.Spell)
        {
        }
        
        public override double CalculateDamage(ICard opponent)
        {
            if (opponent is MonsterCard monsterCard)
            {
                if (monsterCard.MonsterType == MonsterType.Kraken)
                    return 0;

                if (monsterCard.MonsterType == MonsterType.Knight && ElementType == ElementType.Water)
                    return double.MaxValue;
            }

            return CalculateElementalDamage(opponent.ElementType);
        }
    }
}