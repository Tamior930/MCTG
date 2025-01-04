using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface ICardRepository
    {
        // Basic CRUD
        void UpdateCardOwnership(Card card, int userId);
        bool ValidateCardOwnership(int cardId, int userId);
        // void RemoveCard(int cardId);
        Card GetCardById(int cardId);
        // bool UpdateCardOwner(int cardId, int newUserId);

        // // Card Retrieval
        List<Card> GetAllCardsForUser(int userId);
        List<Card> GetRandomCardsForPackage(int count);
        // List<Card> GetCardsByIds(List<int> cardIds);

        // // Filtered Retrieval
        // // List<Card> GetCardsByType(CardType cardType, int userId);
        // // List<Card> GetCardsByElement(ElementType elementType, int userId);


    }
}
