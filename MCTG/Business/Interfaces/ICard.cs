using MCTG.Business.Models;

namespace MCTG.Business.Interfaces
{
    public interface ICard
    {
        int Id { get; set; }
        string Name { get; set; }
        int Damage { get; }
        CardType Type { get; }
        ElementType ElementType { get; }
        double CalculateDamage(ICard opponent);
    }
}
