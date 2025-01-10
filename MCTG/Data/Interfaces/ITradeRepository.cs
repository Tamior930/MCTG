using MCTG.Business.Models;

namespace MCTG.Data.Interfaces
{
    public interface ITradeRepository
    {
        List<Trade> GetAllTradingDeals();
        bool IsCardInTrade(int cardId);
        bool CreateTrade(Trade trade);
        Trade GetTradeById(int tradingId);
        bool DeleteTrade(int tradingId);
        bool ExecuteTrade(int tradingId, int offeredCardId, int newOwnerId);
    }
}
