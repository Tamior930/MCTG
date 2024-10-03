using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Models
{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Token authToken { get; set; }

        //public int Coins { get; set; } = 20;
        // Deck
        //public int ELO { get; set; } = 100;

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            authToken = null;
        }

        public void addCard(Card card)
        {
            //Cards.Add(card);

        }

        public void removeCard(Card card)
        {
            //Cards.Remove(card);
        }
    }
}
