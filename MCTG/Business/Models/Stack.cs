namespace MCTG.BusinessLayer.Models
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
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            Cards.Add(card);
        }

        // public bool RemoveCard(Card card)
        // {
        //     if (card == null)
        //         throw new ArgumentNullException(nameof(card));

        //     return Cards.Remove(card);
        // }

        // public bool Contains(Card card)
        // {
        //     return Cards.Contains(card);
        // }

        // public bool IsCardAvailableForTrade(Card card, Deck deck)
        // {
        //     if (card == null)
        //         throw new ArgumentNullException(nameof(card));
        //     if (deck == null)
        //         throw new ArgumentNullException(nameof(deck));

        //     return Contains(card) && !deck.Cards.Contains(card);
        // }

        // public int Count => Cards.Count;
    }
}