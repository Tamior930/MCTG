using MCTG.BusinessLayer.Interfaces;

namespace MCTG.BusinessLayer.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string name, int damage, ElementType elementType)
            : base(name, damage, elementType)
        {
        }

        public override double CalculateDamage(ICard opponent)
        {
            // Check if the opponent iss a MonsterCard
            if (opponent is MonsterCard)
            {
                // Use a switch statement to determine damage based on element types
                switch (ElementType)
                {
                    case ElementType.Water:
                        switch (opponent.ElementType)
                        {
                            case ElementType.Fire:
                                return Damage * 2; // Water is effective against Fire
                            case ElementType.Normal:
                                return Damage * 0.5; // Water is not effective against Normal
                        }
                        break;

                    case ElementType.Fire:
                        switch (opponent.ElementType)
                        {
                            case ElementType.Normal:
                                return Damage * 2; // Fire is effective against Normal
                            case ElementType.Water:
                                return Damage * 0.5; // Fire is not effective against Water
                        }
                        break;

                    case ElementType.Normal:
                        switch (opponent.ElementType)
                        {
                            case ElementType.Water:
                                return Damage * 2; // Normal is effective against Water
                            case ElementType.Fire:
                                return Damage * 0.5; // Normal is not effective against Fire
                        }
                        break;
                }
            }

            // If none of the conditions are met, return the base damage
            return Damage; // No Effeft
        }

    }
}
