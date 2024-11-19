using MCTG.BusinessLayer.Models;
using MCTG.DataLayer.Repositories;

namespace MCTG.PresentationLayer.Services
{
    public class CardService
    {
        private readonly ICardRepository _cardRepository;

        public CardService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        public List<Card> CreateRandomPackage()
        {
            // Implementation for creating a random package of 5 cards
            throw new NotImplementedException();
        }

        public List<Card> GetCardsByIds(List<string> cardIds)
        {
            // Implementation for retrieving cards by their IDs
            throw new NotImplementedException();
        }
    }
}
