using MCTG.BusinessLayer.Interfaces;

namespace MCTG.BusinessLayer.Models
{
    public enum ElementType
    {
        Fire,
        Water,
        Normal
    }
    public abstract class Card : ICard
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public ElementType ElementType { get; private set; }

        protected Card(string name, int damage, ElementType elementType)
        {
            Name = name;
            Damage = damage;
            ElementType = elementType;
        }

        public virtual double CalculateDamage(ICard opponent)
        {
            return Damage;
        }
    }
}
