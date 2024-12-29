using MCTG.BusinessLayer.Models;

namespace MCTG.Data.Interfaces
{
    public interface ITradeRepository
    {
        // Basic CRUD
        bool CreateTrade(int cardId, int userId, CardType requiredCardType,
                        ElementType? requiredElementType, MonsterType? requiredMonsterType,
                        int minimumDamage);
        Trade GetTradeById(int tradeId);
        bool CancelTrade(int tradeId);

        // Trade Management
        bool ExecuteTrade(int tradeId, int offeredCardId, int newOwnerId);
        List<Trade> GetAllActiveTrades();
        List<Trade> GetTradesByUser(int userId);

        // Validation
        bool IsCardInTrade(int cardId);
        bool ValidateTradeRequirements(int cardId, Trade trade);
    }
}
