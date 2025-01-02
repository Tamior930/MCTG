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

        // Basic CRUD Operations

        /// <summary>
        /// Creates a new trade offer
        /// </summary>
        public bool CreateTrade(int cardId, int userId, CardType requiredCardType,
                              ElementType? requiredElementType, MonsterType? requiredMonsterType,
                              int minimumDamage)
        {
            // Validate that card is not in deck
            if (_deckRepository.IsCardInDeck(cardId))
            {
                throw new InvalidOperationException("Cannot trade a card that is in a deck");
            }

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO trades (card_id, user_id, required_type, required_element_type, 
                                      required_monster_type, minimum_damage, is_active)
                    VALUES (@cardId, @userId, @type, @elementType, @monsterType, @minDamage, true)";

                SetTradeParameters(command, cardId, userId, requiredCardType,
                                 requiredElementType, requiredMonsterType, minimumDamage);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to create trade for card {cardId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a specific trade by its ID
        /// </summary>
        public Trade GetTradeById(int tradeId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM trades WHERE id = @tradeId AND is_active = true";
            command.Parameters.AddWithValue("@tradeId", tradeId);

            try
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return CreateTradeFromDatabaseRow(reader);
                }
                return null!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get trade {tradeId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels (soft deletes) a trade
        /// </summary>
        public bool CancelTrade(int tradeId)
        {
            return DeleteTrade(tradeId);
        }

        // Trade Management Operations

        /// <summary>
        /// Executes a trade between two users
        /// </summary>
        public bool ExecuteTrade(int tradeId, int offeredCardId, int newOwnerId)
        {
            // Validate trade exists and is active
            var trade = GetTradeById(tradeId);
            if (trade == null || !trade.IsActive)
            {
                return false;
            }

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Step 1: Transfer the traded card to new owner
                bool cardOneTransferred = _cardRepository.UpdateCardOwner(trade.CardId, newOwnerId);
                if (!cardOneTransferred) throw new Exception("Failed to transfer first card");

                // Step 2: Transfer the offered card to original owner
                bool cardTwoTransferred = _cardRepository.UpdateCardOwner(offeredCardId, trade.UserId);
                if (!cardTwoTransferred) throw new Exception("Failed to transfer second card");

                // Step 3: Mark trade as completed
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE trades SET is_active = false WHERE id = @tradeId";
                command.Parameters.AddWithValue("@tradeId", tradeId);
                command.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to execute trade {tradeId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active trades in the system
        /// </summary>
        public List<Trade> GetAllActiveTrades()
        {
            var tradesList = new List<Trade>();

            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM trades WHERE is_active = true";

            try
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Trade trade = CreateTradeFromDatabaseRow(reader);
                    tradesList.Add(trade);
                }
                return tradesList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get all trades: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active trades for a specific user
        /// </summary>
        public List<Trade> GetTradesByUser(int userId)
        {
            var userTrades = new List<Trade>();

            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM trades WHERE user_id = @userId AND is_active = true";
            command.Parameters.AddWithValue("@userId", userId);

            try
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Trade trade = CreateTradeFromDatabaseRow(reader);
                    userTrades.Add(trade);
                }
                return userTrades;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get trades for user {userId}: {ex.Message}");
            }
        }

        // Validation Operations

        /// <summary>
        /// Checks if a card is currently in any active trade
        /// </summary>
        public bool IsCardInTrade(int cardId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM trades WHERE card_id = @cardId AND is_active = true";
            command.Parameters.AddWithValue("@cardId", cardId);

            try
            {
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check if card {cardId} is in trade: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if an offered card meets the trade requirements
        /// </summary>
        public bool ValidateTradeRequirements(int cardId, Trade trade)
        {
            try
            {
                var offeredCard = _cardRepository.GetCardById(cardId);
                if (offeredCard == null) return false;

                // Check basic requirements
                if (offeredCard.Damage < trade.MinimumDamage) return false;
                if (offeredCard.Type != trade.RequiredCardType) return false;

                // Check element type if specified
                if (trade.RequiredElementType.HasValue &&
                    offeredCard.ElementType != trade.RequiredElementType.Value)
                    return false;

                // Check monster type if specified and if card is a monster card
                if (trade.RequiredMonsterType.HasValue &&
                    offeredCard is MonsterCard monsterCard &&
                    monsterCard.MonsterType != trade.RequiredMonsterType.Value)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to validate trade requirements for card {cardId}: {ex.Message}");
            }
        }

        // Helper Methods

        /// <summary>
        /// Sets parameters for a trade command
        /// </summary>
        private void SetTradeParameters(NpgsqlCommand command, int cardId, int userId, CardType requiredCardType, ElementType? requiredElementType, MonsterType? requiredMonsterType, int minimumDamage)
        {
            command.Parameters.AddWithValue("@cardId", cardId);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@type", requiredCardType.ToString());
            command.Parameters.AddWithValue("@elementType", requiredElementType.HasValue ? requiredElementType.Value.ToString() : DBNull.Value);
            command.Parameters.AddWithValue("@monsterType", requiredMonsterType.HasValue ? requiredMonsterType.Value.ToString() : DBNull.Value);
            command.Parameters.AddWithValue("@minDamage", minimumDamage);
        }

        /// <summary>
        /// Creates a trade object from database row data
        /// </summary>
        private Trade CreateTradeFromDatabaseRow(NpgsqlDataReader reader)
        {
            int cardId = reader.GetInt32(reader.GetOrdinal("card_id"));
            int userId = reader.GetInt32(reader.GetOrdinal("user_id"));
            CardType requiredType = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("required_type")));
            int minimumDamage = reader.GetInt32(reader.GetOrdinal("minimum_damage"));

            // Handle nullable types
            ElementType? elementType = null;
            if (!reader.IsDBNull(reader.GetOrdinal("required_element_type")))
            {
                elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("required_element_type")));
            }

            MonsterType? monsterType = null;
            if (!reader.IsDBNull(reader.GetOrdinal("required_monster_type")))
            {
                monsterType = Enum.Parse<MonsterType>(reader.GetString(reader.GetOrdinal("required_monster_type")));
            }

            var trade = new Trade(cardId, userId, requiredType, elementType, monsterType, minimumDamage)
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            };

            return trade;
        }

        // Private Implementation Methods

        /// <summary>
        /// Marks a trade as inactive (soft delete)
        /// </summary>
        private bool DeleteTrade(int tradeId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE trades SET is_active = false WHERE id = @tradeId";
                command.Parameters.AddWithValue("@tradeId", tradeId);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to delete trade {tradeId}: {ex.Message}");
            }
        }

        public List<Trade> GetAllTrades()
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM trades WHERE status = 'ACTIVE'";
            var trades = new List<Trade>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                trades.Add(CreateTradeFromDatabaseRow(reader));
            }
            return trades;
        }

        public bool CreateTrade(Trade trade)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO trades (card_id, user_id, required_type, minimum_damage, status) 
                VALUES (@cardId, @userId, @type, @minDamage, 'ACTIVE')";

            command.Parameters.AddWithValue("@cardId", trade.CardId);
            command.Parameters.AddWithValue("@userId", trade.UserId);
            command.Parameters.AddWithValue("@type", trade.RequiredType);
            command.Parameters.AddWithValue("@minDamage", trade.MinimumDamage);

            return command.ExecuteNonQuery() > 0;
        }
    }
}
