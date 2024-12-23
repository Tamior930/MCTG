using MCTG.BusinessLayer.Interfaces;

namespace MCTG.BusinessLayer.Models
{
    public enum ElementType
    {
        Fire,
        Water,
        Normal
    }

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
        public ElementType ElementType { get; private set; }
        public CardType Type { get; private set; }

        protected Card(string name, int damage, ElementType elementType, CardType type)
        {
            Name = name;
            Damage = damage;
            ElementType = elementType;
            Type = type;
        }

        public virtual double CalculateDamage(ICard opponent)
        {
            return Damage;
        }
    }
}
