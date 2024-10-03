using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Models
{
    internal class Token
    {
        public string Value { get; set; }

        public Token(string token)
        {
            Value = token;
        }
    }
}
