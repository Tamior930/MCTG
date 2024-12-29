using MCTG.BusinessLayer.Interfaces;

namespace MCTG.BusinessLayer.Models
{
    public enum MonsterType
    {
        Goblin,
        Dragon,
        Wizard,
        Ork,
        Knight,
        Kraken,
        FireElf
    }

    public class MonsterCard : Card
    {
        public MonsterType MonsterType { get; private set; }

        public MonsterCard(int id, string name, int damage, ElementType elementType, MonsterType monsterType)
            : base(id, name, damage, elementType, CardType.Monster)
        {
        }

        public override double CalculateDamage(ICard opponent)
        {
            // Special monster rules
            if (opponent is MonsterCard opponentMonster)
            {
                // Goblins are too afraid of Dragons
                if (MonsterType == MonsterType.Goblin && opponentMonster.MonsterType == MonsterType.Dragon)
                    return 0;

                // Wizards can control Orks
                if (MonsterType == MonsterType.Ork && opponentMonster.MonsterType == MonsterType.Wizard)
                    return 0;

                // FireElves can evade Dragon attacks
                if (MonsterType == MonsterType.FireElf && opponentMonster.MonsterType == MonsterType.Dragon)
                    return Damage;

                // Pure monster fights are not affected by element type
                return Damage;
            }

            // Against SpellCard
            if (opponent is SpellCard)
            {
                // Kraken is immune against spells
                if (MonsterType == MonsterType.Kraken)
                    return Damage;

                // Knights drown instantly against water spells
                if (MonsterType == MonsterType.Knight && opponent.ElementType == ElementType.Water)
                    return 0;

                // Calculate elemental effectiveness for other cases
                return CalculateElementalDamage(opponent.ElementType);
            }

            return Damage;
        }
    }
}
