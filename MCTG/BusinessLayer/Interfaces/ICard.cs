using MCTG.BusinessLayer.Models;

namespace MCTG.BusinessLayer.Interfaces
{
    public interface ICard
    {
        int Id { get; set; }
        string Name { get; }
        int Damage { get; }
        CardType Type { get; }
        ElementType ElementType { get; }
        double CalculateDamage(ICard opponent);
    }
}
