using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface ICardRepository
    {
        void AddCard(Card card);
        void RemoveCard(int cardId);
        Card GetCardById(int cardId);
        List<Card> GetAllCardsForUser(int userId);
    }
}
