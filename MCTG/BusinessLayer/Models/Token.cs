using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Models
{
    public class Token
    {
        public string Value { get; private set; }
        public DateTime Expiration { get; private set; }

        public Token(string value, DateTime expiration)
        {
            Value = value;
            Expiration = expiration;
        }

        public bool IsValid()
        {
            return DateTime.Now < Expiration;
        }

        public static Token GenerateToken()
        {
            string tokenValue = Guid.NewGuid().ToString();
            DateTime expiration = DateTime.Now.AddHours(1); // Token valid for 1 hour
            return new Token(tokenValue, expiration);
        }

        public void Invalidate()
        {
            Expiration = DateTime.Now.AddSeconds(-1);
        }
    }
}
