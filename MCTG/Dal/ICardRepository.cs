using MCTG.BusinessLayer.Models;

namespace MCTG.Dal
{
    public interface ICardRepository
    {
        void AddCard(Card card);
        void RemoveCard(Card card);
        List<Card> GetAllCards();
        Card GetCardByName(string name);
    }
}
