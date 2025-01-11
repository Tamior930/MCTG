using MCTG.Business.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class TradeRepository : ITradeRepository
    {
        private readonly DatabaseHandler _databaseHandler;

        public TradeRepository()
        {
            _databaseHandler = new DatabaseHandler();
        }

        // Gets all active trading deals
        public List<Trade> GetAllTradingDeals()
        {
            var trades = new List<Trade>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM trades WHERE status = 'ACTIVE'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                trades.Add(MapTradeFromDatabase(reader));
            }
            return trades;
        }

        // Maps database row to Trade object
        private Trade MapTradeFromDatabase(NpgsqlDataReader reader)
        {
            var trade = new Trade(
                reader.GetInt32(reader.GetOrdinal("card_id")),
                reader.GetString(reader.GetOrdinal("required_type")),
                reader.IsDBNull(reader.GetOrdinal("minimum_damage")) ? null : reader.GetInt32(reader.GetOrdinal("minimum_damage"))
            );

            trade.Id = reader.GetInt32(reader.GetOrdinal("id"));
            trade.UserId = reader.GetInt32(reader.GetOrdinal("user_id"));
            trade.Status = reader.GetString(reader.GetOrdinal("status"));

            return trade;
        }

        // Creates a new trading deal
        public bool CreateTrade(Trade trade)
        {
            try
            {
                using var connection = _databaseHandler.GetConnection();
                connection.Open();
                using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO trades (card_id, user_id, required_type, minimum_damage, status) 
                    VALUES (@cardId, @userId, @requiredType, @minimumDamage, 'ACTIVE')
                    RETURNING id";

                command.Parameters.AddWithValue("@cardId", trade.CardId);
                command.Parameters.AddWithValue("@userId", trade.UserId);
                command.Parameters.AddWithValue("@requiredType", trade.RequiredType);
                command.Parameters.AddWithValue("@minimumDamage", trade.MinimumDamage);

                trade.Id = Convert.ToInt32(command.ExecuteScalar());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error in CreateTrade: {ex.Message}");
                return false;
            }
        }

        // Checks if a card is part of any active trade
        public bool IsCardInTrade(int cardId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT COUNT(*) FROM trades WHERE card_id = @cardId AND status = 'ACTIVE'";
            command.Parameters.AddWithValue("@cardId", cardId);

            try
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        // Retrieves a specific trade by ID
        public Trade GetTradeById(int tradingId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM trades WHERE id = @tradingId AND status = 'ACTIVE'";
            command.Parameters.AddWithValue("@tradingId", tradingId);

            using var reader = command.ExecuteReader();
            if (!reader.Read())  // Check if there's any data before reading
                return null;     // Return null if no trade found

            return MapTradeFromDatabase(reader);
        }

        // Marks a trade as deleted
        public bool DeleteTrade(int tradingId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE trades SET status = 'DELETED' WHERE id = @tradingId";
                command.Parameters.AddWithValue("@tradingId", tradingId);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        // Executes a trade between two users
        public bool ExecuteTrade(int tradingId, int offeredCardId, int newOwnerId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var trade = GetTradeById(tradingId);
                if (trade == null) 
                {
                    Console.WriteLine("Trade not found");
                    return false;
                }

                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                // First update: offered card ownership
                command.CommandText = "UPDATE cards SET user_id = @newOwnerId WHERE id = @offeredCardId";
                command.Parameters.AddWithValue("@newOwnerId", trade.UserId);  // Original owner gets the offered card
                command.Parameters.AddWithValue("@offeredCardId", offeredCardId);
                int rowsAffected1 = command.ExecuteNonQuery();

                // Second update: traded card ownership
                command.CommandText = "UPDATE cards SET user_id = @newOwnerId2 WHERE id = @tradeCardId";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@newOwnerId2", newOwnerId);  // New owner gets the traded card
                command.Parameters.AddWithValue("@tradeCardId", trade.CardId);
                int rowsAffected2 = command.ExecuteNonQuery();

                // Mark trade as completed
                command.CommandText = "UPDATE trades SET status = 'COMPLETED' WHERE id = @tradingId";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@tradingId", tradingId);
                int rowsAffected3 = command.ExecuteNonQuery();

                if (rowsAffected1 == 0 || rowsAffected2 == 0 || rowsAffected3 == 0)
                {
                    Console.WriteLine($"Trade failed: Cards updated: {rowsAffected1}, {rowsAffected2}, Trade status: {rowsAffected3}");
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing trade: {ex.Message}");
                transaction.Rollback();
                return false;
            }
        }
    }
}
