namespace MCTG.Business.Models
{
    public class Stack
    {
        public List<Card> Cards { get; private set; }

        public Stack()
        {
            Cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }
    }
}