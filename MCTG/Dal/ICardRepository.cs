using MCTG.BusinessLayer.Interfaces;
using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.Dal
{
    public interface ICardRepository
    {
        void AddCard(Card card);
        void RemoveCard(Card card);
        List<Card> GetAllCards();
        Card GetCardByName(string name);
    }
}
