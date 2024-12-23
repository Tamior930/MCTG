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
            Cards.Add(card);
        }

        public bool RemoveCard(Card card)
        {
            return Cards.Remove(card);
        }

        public bool Contains(Card card)
        {
            return Cards.Contains(card);
        }

        public int Count()
        {
            return Cards.Count;
        }

        public void AddRange(List<Card> cards)
        {
            Cards.AddRange(cards);
        }

        public bool IsCardAvailableForTrade(Card card, Deck deck)
        {
            return Contains(card) && !deck.Cards.Contains(card);
        }

        public IReadOnlyList<Card> GetCards()
        {
            return Cards.AsReadOnly();
        }
    }
}