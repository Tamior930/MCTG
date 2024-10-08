using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Models
{
    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Coins { get; private set; }
        public List<Card> Stack { get; private set; }
        public Deck Deck { get; private set; }
        public Token AuthToken { get; private set; }
        public int ELO { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }

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

        public void AssignToken(Token token)
        {
            AuthToken = token;
        }

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
