using MCTG.Dal;
using MCTG.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG.PresentationLayer.Utils
{
    public class TokenUtils
    {
        public static bool ValidateToken(string tokenValue, IUserRepository playerRepository)
        {
            foreach (var user in playerRepository.GetAllUsers())
            {
                if (user.AuthToken != null && user.AuthToken.Value == tokenValue && user.AuthToken.IsValid())
                    return true;
            }
            return false;
        }

        public static string ExtractToken(string authHeader)
        {
            if (authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }
    }
}
