using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class TradeRepository : ITradeRepository
    {
        private readonly DatabaseHandler _dbHandler;
        private readonly ICardRepository _cardRepository;

        public TradeRepository(ICardRepository cardRepository)
        {
            _dbHandler = new DatabaseHandler();
            _cardRepository = cardRepository;
        }

        public bool CreateTrade(int cardId, int userId, string type, ElementType? requiredElementType, int minimumDamage)
        {
            // Check if card is in deck
            if (_cardRepository.IsCardInDeck(cardId))
            {
                return false;
            }

            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO trades (card_id, user_id, required_type, required_element_type, minimum_damage, is_active)
                VALUES (@cardId, @userId, @type, @elementType, @minDamage, true)";

            cmd.Parameters.AddWithValue("@cardId", cardId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@elementType", requiredElementType?.ToString());
            cmd.Parameters.AddWithValue("@minDamage", minimumDamage);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Trade> GetAllTrades()
        {
            var trades = new List<Trade>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM trades WHERE is_active = true";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                trades.Add(ReadTrade(reader));
            }

            return trades;
        }

        public Trade GetTradeById(int tradeId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM trades WHERE id = @tradeId AND is_active = true";
            cmd.Parameters.AddWithValue("@tradeId", tradeId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return ReadTrade(reader);
            }
            return null;
        }

        public bool DeleteTrade(int tradeId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE trades SET is_active = false WHERE id = @tradeId";
            cmd.Parameters.AddWithValue("@tradeId", tradeId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ExecuteTrade(int tradeId, int offeredCardId, int newOwnerId)
        {
            var trade = GetTradeById(tradeId);
            if (trade == null || !trade.IsActive)
            {
                return false;
            }

            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Update ownership of both cards
                _cardRepository.UpdateCardOwner(trade.CardId, newOwnerId);
                _cardRepository.UpdateCardOwner(offeredCardId, trade.UserId);

                // Mark trade as completed
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE trades SET is_active = false WHERE id = @tradeId";
                cmd.Parameters.AddWithValue("@tradeId", tradeId);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<Trade> GetTradesByUser(int userId)
        {
            var trades = new List<Trade>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM trades WHERE user_id = @userId AND is_active = true";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                trades.Add(ReadTrade(reader));
            }

            return trades;
        }

        public bool IsCardInTrade(int cardId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM trades WHERE card_id = @cardId AND is_active = true";
            cmd.Parameters.AddWithValue("@cardId", cardId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private Trade ReadTrade(NpgsqlDataReader reader)
        {
            return new Trade(
                cardId: reader.GetInt32(reader.GetOrdinal("card_id")),
                userId: reader.GetInt32(reader.GetOrdinal("user_id")),
                requiredType: reader.GetString(reader.GetOrdinal("required_type")),
                requiredElementType: reader.IsDBNull(reader.GetOrdinal("required_element_type")) 
                    ? null 
                    : Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("required_element_type"))),
                minimumDamage: reader.GetInt32(reader.GetOrdinal("minimum_damage")))
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            };
        }
    }
}
