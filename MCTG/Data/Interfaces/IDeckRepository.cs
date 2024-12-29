using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface IDeckRepository
    {
        // Basic CRUD
        void SaveDeck(int userId, List<Card> cards);
        List<Card> GetDeckByUserId(int userId);
        void AddCardToDeck(int userId, Card card);
        void RemoveCardFromDeck(int userId, Card card);
        void ClearDeck(int userId);

        // Deck Management
        Card GetRandomCardFromDeck(int userId);
        bool TransferCardBetweenDecks(int cardId, int fromUserId, int toUserId);
        void SetCardInDeck(int cardId, bool inDeck);

        // Validation
        int GetDeckCount(int userId);
        bool IsValidDeck(int userId);
        bool IsCardInDeck(int cardId);
    }
}