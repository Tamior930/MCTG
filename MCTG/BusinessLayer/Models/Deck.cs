namespace MCTG.BusinessLayer.Models
{
    public class Deck
    {
        public List<Card> Cards { get; private set; }

        public Deck()
        {
            Cards = new List<Card>();
        }

        public void SetDeck(List<Card> selectedCards)
        {
            if (selectedCards.Count != 4)
                throw new System.Exception("A deck must consist of exactly 4 cards.");

            Cards = selectedCards;
        }

        public void AddCard(Card card)
        {
            if (Cards.Count >= 4)
                throw new System.Exception("Deck already has 4 cards.");

            Cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            Cards.Remove(card);
        }
    }
}
