using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface IDeckRepository
    {
        void SaveDeck(int userId, List<Card> cards);
        List<Card> GetDeckByUserId(int userId);
        void AddCardToDeck(int userId, Card card);
        void RemoveCardFromDeck(int userId, Card card);
        int GetDeckCount(int userId);
    }
} 