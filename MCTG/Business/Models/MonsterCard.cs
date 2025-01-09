using MCTG.Business.Interfaces;

namespace MCTG.Business.Models
{
    public class MonsterCard : Card
    {
        public MonsterType MonsterType { get; private set; }

        public MonsterCard(int id, string name, int damage, ElementType elementType, MonsterType monsterType)
            : base(id, name, damage, elementType, CardType.Monster)
        {
            MonsterType = monsterType;
        }

        public override double CalculateDamage(ICard opponent)
        {
            if (opponent is MonsterCard opponentMonster)
            {
                if (MonsterType == MonsterType.Goblin && opponentMonster.MonsterType == MonsterType.Dragon)
                    return 0;

                if (MonsterType == MonsterType.Ork && opponentMonster.MonsterType == MonsterType.Wizard)
                    return 0;

                if (MonsterType == MonsterType.FireElf && opponentMonster.MonsterType == MonsterType.Dragon)
                    return Damage;

                return Damage;
            }

            if (opponent is SpellCard)
            {
                if (MonsterType == MonsterType.Kraken)
                    return Damage;

                if (MonsterType == MonsterType.Knight && opponent.ElementType == ElementType.Water)
                    return 0;

                return CalculateElementalDamage(opponent.ElementType);
            }

            return Damage;
        }
    }
}
