using MCTG.BusinessLayer.Models;

namespace MCTG.BusinessLayer.Interfaces
{
    public interface ICard
    {
        string Name { get; }
        int Damage { get; }
        ElementType ElementType { get; }
        double CalculateDamage(ICard opponent);
    }
}
