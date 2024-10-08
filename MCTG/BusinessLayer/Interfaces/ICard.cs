using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.BusinessLayer.Interfaces
{
    internal interface ICard
    {
        string Name { get; }
        int Damage { get; }
        ElementType ElementType { get; }
        double CalculateDamage(ICard opponent);
    }
}
