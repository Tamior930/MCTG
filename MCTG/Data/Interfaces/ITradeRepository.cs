using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface ITradeRepository
    {
        bool CreateTrade(int cardId, int userId, string type, ElementType? requiredElementType, int minimumDamage);
        List<Trade> GetAllTrades();
        Trade GetTradeById(int tradeId);
        bool DeleteTrade(int tradeId);
        bool ExecuteTrade(int tradeId, int cardId, int newOwnerId);
        List<Trade> GetTradesByUser(int userId);
        bool IsCardInTrade(int cardId);
    }
}
