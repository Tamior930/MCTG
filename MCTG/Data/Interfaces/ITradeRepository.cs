using MCTG.Business.Models;

namespace MCTG.Data.Interfaces
{
    public interface ITradeRepository
    {
        List<Trade> GetAllTradingDeals();
        bool IsCardInTrade(int cardId);
        bool CreateTrade(Trade trade);
        Trade GetTradeById(string tradingId);
        bool DeleteTrade(string tradingId);
        bool ExecuteTrade(string tradingId, int offeredCardId, int newOwnerId);
    }
}
