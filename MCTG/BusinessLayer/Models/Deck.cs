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
            {
                // Err: Card can only be 4!
            }
            Cards = selectedCards;
        }

        public void AddCard(Card card)
        {
            if (Cards.Count >= 4)
            {
                // Err: Deck already has 4 cards!
            }
            Cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            Cards.Remove(card);
        }
    }
}
