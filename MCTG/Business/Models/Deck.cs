namespace MCTG.Business.Models
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
            Cards = new List<Card>(selectedCards);
        }
    }
}
