using MCTG.Business.Models;

namespace MCTG.Data.Interfaces
{
    public interface ICardRepository
    {
        bool UpdateCardOwnership(Card card, int newUserId);
        bool ValidateCardOwnership(int cardId, int userId);
        Card? GetCardById(int cardId);
        List<Card> GetAllCardsForUser(int userId);
        List<Card> GetRandomCardsForPackage(int count);
    }
}
