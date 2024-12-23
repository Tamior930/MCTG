using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface ICardRepository
    {
        void AddCard(Card card, int userId);
        void RemoveCard(int cardId);
        Card GetCardById(int cardId);
        List<Card> GetAllCardsForUser(int userId);
        List<Card> GetRandomCardsForPackage(int count);
        bool UpdateCardOwner(int cardId, int newUserId);
        bool IsCardInDeck(int cardId);
        void SetCardInDeck(int cardId, bool inDeck);
        bool IsCardInTrade(int cardId);
        List<Card> GetCardsByType(string cardType, int userId);
        List<Card> GetCardsByElement(ElementType elementType, int userId);
    }
}
