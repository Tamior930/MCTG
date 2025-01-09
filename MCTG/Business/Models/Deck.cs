namespace MCTG.Business.Models
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
                throw new ArgumentException($"Deck must contain exactly {MAX_CARDS} cards!");

            Cards = new List<Card>(selectedCards);
        }
    }
}
