using MCTG.BusinessLayer.Models;
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

        public List<Trade> GetAllTradingDeals()
        {
            var trades = new List<Trade>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM trades WHERE is_active = true";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                trades.Add(MapTradeFromDatabase(reader));
            }
            return trades;
        }

        private Trade MapTradeFromDatabase(NpgsqlDataReader reader)
        {
            return new Trade(
                reader.GetInt32(reader.GetOrdinal("card_id")),
                reader.GetInt32(reader.GetOrdinal("user_id")),
                reader.GetString(reader.GetOrdinal("required_type")),
                reader.GetString(reader.GetOrdinal("required_element_type")),
                reader.GetString(reader.GetOrdinal("required_monster_type")),
                reader.GetInt32(reader.GetOrdinal("minimum_damage"))
            )
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                IsActive = reader.GetString(reader.GetOrdinal("status")) == "ACTIVE"
            };
        }

        public bool CreateTrade(Trade trade)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO trades (card_id, user_id, required_type, required_element_type, 
                                  required_monster_type, minimum_damage, status) 
                VALUES (@cardId, @userId, @requiredType, @requiredElementType, 
                        @requiredMonsterType, @minimumDamage, true)
                RETURNING id";

            command.Parameters.AddWithValue("@cardId", trade.CardId);
            command.Parameters.AddWithValue("@userId", trade.UserId);
            command.Parameters.AddWithValue("@requiredType", trade.RequiredType);
            command.Parameters.AddWithValue("@requiredElementType", trade.RequiredElementType);
            command.Parameters.AddWithValue("@requiredMonsterType", trade.RequiredMonsterType);
            command.Parameters.AddWithValue("@minimumDamage", trade.MinimumDamage);

            try
            {
                var id = Convert.ToInt32(command.ExecuteScalar());
                trade.Id = id;
                return true;
            }
            catch
            {
                return false;
            }
        }
    
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
    }
}
