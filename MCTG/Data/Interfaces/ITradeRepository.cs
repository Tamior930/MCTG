using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface ITradeRepository
    {
        List<Trade> GetAllTradingDeals();
        bool IsCardInTrade(int cardId);
        bool CreateTrade(Trade trade);
    }
}
