namespace MCTG.BusinessLayer.Models
{
    public class Deck
    {
        private const int MAX_CARDS = 4;
        public List<Card> Cards { get; private set; }

        public Deck()
        {
            Cards = new List<Card>();
        }

        public void SetDeck(List<Card> selectedCards)
        {
            if (selectedCards.Count != MAX_CARDS)
            {
                throw new ArgumentException("Deck must contain exactly 4 cards!");
            }
            Cards = selectedCards;
        }

        public void AddCard(Card card)
        {
            if (Cards.Count >= MAX_CARDS)
            {
                throw new InvalidOperationException("Deck already has 4 cards!");
            }
            Cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            Cards.Remove(card);
        }
    }
}
