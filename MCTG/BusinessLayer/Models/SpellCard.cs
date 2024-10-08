using MCTG.BusinessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Models
{
    internal class SpellCard : Card
    {
        public SpellCard(string name, int damage, ElementType elementType)
            : base(name, damage, elementType)
        {
        }

        public override double CalculateDamage(ICard opponent)
        {
            if (opponent is MonsterCard)
            {
                switch (ElementType)
                {
                    case ElementType.Water when opponent.ElementType == ElementType.Fire:
                        return Damage * 2; // Effective

                    case ElementType.Fire when opponent.ElementType == ElementType.Normal:
                        return Damage * 2; // Effective

                    case ElementType.Normal when opponent.ElementType == ElementType.Water:
                        return Damage * 2; // Effective

                    case ElementType.Fire when opponent.ElementType == ElementType.Water:
                        return Damage * 0.5; // Not Effective

                    case ElementType.Normal when opponent.ElementType == ElementType.Fire:
                        return Damage * 0.5; // Not Effective

                    case ElementType.Water when opponent.ElementType == ElementType.Normal:
                        return Damage * 0.5; // Not Effective

                    default:
                        return Damage; // No Effect
                }
            }
            return Damage;
        }
    }
}
