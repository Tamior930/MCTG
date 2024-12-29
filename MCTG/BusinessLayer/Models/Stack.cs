namespace MCTG.BusinessLayer.Models
{
    public class Stack
    {
        private List<Card> Cards { get; set; }

        public Stack()
        {
            Cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            Cards.Add(card);
        }

        public bool RemoveCard(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            return Cards.Remove(card);
        }

        public bool Contains(Card card)
        {
            return Cards.Contains(card);
        }

        public bool IsCardAvailableForTrade(Card card, Deck deck)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (deck == null)
                throw new ArgumentNullException(nameof(deck));

            return Contains(card) && !deck.Cards.Contains(card);
        }

        public IReadOnlyList<Card> GetCards()
        {
            return Cards.AsReadOnly();
        }

        public List<Card> GetCardsByType(CardType type)
        {
            return Cards.Where(c => c.Type == type).ToList();
        }

        public List<Card> GetCardsByElement(ElementType elementType)
        {
            return Cards.Where(c => c.ElementType == elementType).ToList();
        }

        public int Count => Cards.Count;

        public void AddRange(List<Card> cards)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            Cards.AddRange(cards);
        }
    }
}