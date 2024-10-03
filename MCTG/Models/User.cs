using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.Models
{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public int Coins { get; set; } = 20;
        // Deck
        public int ELO { get; set; } = 100;

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Token = null;
        }

    }
}
