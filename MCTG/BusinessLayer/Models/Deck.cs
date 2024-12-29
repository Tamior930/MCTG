namespace MCTG.BusinessLayer.Models
{
    public class Deck
    {
        private const int MAX_CARDS = 4;
        private readonly Random _random = new();
        public List<Card> Cards { get; private set; }

        public Deck()
        {
            Cards = new List<Card>();
        }

        public void SetDeck(List<Card> selectedCards)
        {
            if (selectedCards.Count != MAX_CARDS)
                throw new ArgumentException($"Deck must contain exactly {MAX_CARDS} cards!");

            Cards = new List<Card>(selectedCards);
        }

        public void AddCard(Card card)
        {
            if (Cards.Count >= MAX_CARDS)
                throw new InvalidOperationException($"Deck already has {MAX_CARDS} cards!");

            Cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            Cards.Remove(card);
        }

        public Card GetRandomCard()
        {
            if (Cards.Count == 0)
                return null;

            int index = _random.Next(Cards.Count);
            return Cards[index];
        }

        public bool IsValid()
        {
            return Cards.Count == MAX_CARDS;
        }
    }
}
