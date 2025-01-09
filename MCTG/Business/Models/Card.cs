using MCTG.Business.Interfaces;

namespace MCTG.Business.Models
{
    public abstract class Card : ICard
    {
        private static readonly Random _random = new Random();

        public int Id { get; set; }
        public string Name { get; set; }
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

        public static Card GenerateRandomCard()
        {
            bool isMonster = _random.Next(2) == 0;
            int damage = _random.Next(10, 101);
            ElementType elementType = (ElementType)_random.Next(3);

            string name;
            if (isMonster)
            {
                MonsterType monsterType = (MonsterType)_random.Next(Enum.GetValues<MonsterType>().Length);
                name = $"{elementType} {monsterType}";
                return new MonsterCard(0, name, damage, elementType, monsterType);
            }
            else
            {
                name = $"{elementType} Spell";
                return new SpellCard(0, name, damage, elementType);
            }
        }

        public virtual double CalculateDamage(ICard opponent)
        {
            return Damage;
        }

        protected double CalculateElementalDamage(ElementType opponentElement)
        {
            double damageMultiplier = 1.0;

            if ((ElementType == ElementType.Water && opponentElement == ElementType.Fire) ||
                (ElementType == ElementType.Fire && opponentElement == ElementType.Normal) ||
                (ElementType == ElementType.Normal && opponentElement == ElementType.Water))
            {
                damageMultiplier = 2.0;
            }
            else if ((ElementType == ElementType.Fire && opponentElement == ElementType.Water) ||
                    (ElementType == ElementType.Normal && opponentElement == ElementType.Fire) ||
                    (ElementType == ElementType.Water && opponentElement == ElementType.Normal))
            {
                damageMultiplier = 0.5;
            }

            return Damage * damageMultiplier;
        }
    }
}
