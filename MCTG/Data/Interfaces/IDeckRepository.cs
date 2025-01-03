using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface IDeckRepository
    {
        List<Card> GetDeckCards(int userId);
        bool SaveDeck(int userId, List<Card> cards);
    }
}