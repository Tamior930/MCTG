using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Models
{
    class Card
    {
        public string CardName { get; set; }
        public int Damage { get; set; }

        public Card(string cardName, int damage)
        {
            CardName = cardName;
            Damage = damage;
        }
    }
}
