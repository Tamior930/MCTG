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
        public int Coins { get; set; }
        public List<Card> Stack { get; set; }
        public Deck Deck { get; set; }

        public string Token { get; set; }

        public int ELO { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Coins = 20;
            Stack = new List<Card>();
            Deck = new Deck();
            ELO = 100;
            Wins = 0;
            Losses = 0;
        }

        //public void AssignToken(Token token)
        //{
        //    AuthToken = token;
        //}

        public void AddCard(Card card)
        {
            Stack.Add(card);
        }

        public bool RemoveCard(Card card)
        {
            return Stack.Remove(card);
        }

        public bool PurchasePackage(List<Card> package)
        {
            if (Coins >= 5)
            {
                Coins -= 5;
                Stack.AddRange(package);
                return true;
            }
            return false;
        }

        public void UpdateELO(bool won)
        {
            if (won)
            {
                ELO += 3;
                Wins++;
            }
            else
            {
                ELO -= 5;
                Losses++;
            }
        }
    }
}
