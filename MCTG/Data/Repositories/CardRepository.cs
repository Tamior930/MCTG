using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly DatabaseHandler _databaseHandler;

        public CardRepository()
        {
            _databaseHandler = new DatabaseHandler();
        }

        // Basic CRUD Operations

        /// <summary>
        /// Adds a new card to a user's collection
        /// </summary>
        public void AddCard(Card card, int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET user_id = @userId WHERE id = @cardId";
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@cardId", card.Id);
            
            command.ExecuteNonQuery();
        }

        // /// <summary>
        // /// Removes a card from the database
        // /// </summary>
        // public void RemoveCard(int cardId)
        // {
        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = "DELETE FROM cards WHERE id = @cardId";
        //     command.Parameters.AddWithValue("@cardId", cardId);

        //     try
        //     {
        //         int rowsAffected = command.ExecuteNonQuery();
        //         if (rowsAffected == 0)
        //         {
        //             throw new InvalidOperationException($"Card with ID {cardId} not found");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to remove card {cardId}: {ex.Message}");
        //     }
        // }

        /// <summary>
        /// Gets a card by its ID
        /// </summary>
        // public Card GetCardById(int cardId)
        // {
        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = "SELECT * FROM cards WHERE id = @cardId";
        //     command.Parameters.AddWithValue("@cardId", cardId);

        //     try
        //     {
        //         using var reader = command.ExecuteReader();
        //         if (reader.Read())
        //         {
        //             return CreateCardFromDatabaseRow(reader);
        //         }
        //         return null!;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to get card {cardId}: {ex.Message}");
        //     }
        // }

        /// <summary>
        /// Updates the owner of a card
        /// </summary>
        // public bool UpdateCardOwner(int cardId, int newUserId)
        // {
        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = "UPDATE cards SET user_id = @newUserId, in_deck = false WHERE id = @cardId";
        //     command.Parameters.AddWithValue("@cardId", cardId);
        //     command.Parameters.AddWithValue("@newUserId", newUserId);

        //     try
        //     {
        //         return command.ExecuteNonQuery() > 0;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to update card owner: {ex.Message}");
        //     }
        // }

        // Card Retrieval Operations

        /// <summary>
        /// Gets all cards owned by a specific user
        /// </summary>
        public List<Card> GetAllCardsForUser(int userId)
        {
            var userCards = new List<Card>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cards WHERE user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                userCards.Add(CreateCardFromDatabaseRow(reader));
            }
            return userCards;
        }

        /// <summary>
        /// Gets all cards in a user's deck
        /// </summary>
        public List<Card> GetDeckCards(int userId)
        {
            var deckCards = new List<Card>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cards WHERE user_id = @userId AND in_deck = true";
            command.Parameters.AddWithValue("@userId", userId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                deckCards.Add(CreateCardFromDatabaseRow(reader));
            }
            return deckCards;
        }

        /// <summary>
        /// Gets random cards for creating a new package
        /// </summary>
        public List<Card> GetRandomCardsForPackage(int count)
        {
            var cards = new List<Card>();
            
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM cards 
                WHERE user_id IS NULL 
                ORDER BY RANDOM() 
                LIMIT @count";
            command.Parameters.AddWithValue("@count", count);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                cards.Add(CreateCardFromDatabaseRow(reader));
            }
            
            return cards;
        }

        /// <summary>
        /// Gets all cards of a specific type owned by a user
        /// </summary>
        // public List<Card> GetCardsByType(CardType cardType, int userId)
        // {
        //     var typeCards = new List<Card>();

        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = "SELECT * FROM cards WHERE user_id = @userId AND card_type = @cardType";
        //     command.Parameters.AddWithValue("@userId", userId);
        //     command.Parameters.AddWithValue("@cardType", cardType.ToString());

        //     try
        //     {
        //         using var reader = command.ExecuteReader();
        //         while (reader.Read())
        //         {
        //             Card card = CreateCardFromDatabaseRow(reader);
        //             typeCards.Add(card);
        //         }
        //         return typeCards;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to get cards by type {cardType} for user {userId}: {ex.Message}");
        //     }
        // }

        /// <summary>
        /// Gets all cards of a specific element owned by a user
        /// </summary>
        // public List<Card> GetCardsByElement(ElementType elementType, int userId)
        // {
        //     var elementCards = new List<Card>();

        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = "SELECT * FROM cards WHERE user_id = @userId AND element_type = @elementType";
        //     command.Parameters.AddWithValue("@userId", userId);
        //     command.Parameters.AddWithValue("@elementType", elementType.ToString());

        //     try
        //     {
        //         using var reader = command.ExecuteReader();
        //         while (reader.Read())
        //         {
        //             Card card = CreateCardFromDatabaseRow(reader);
        //             elementCards.Add(card);
        //         }
        //         return elementCards;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to get cards by element {elementType} for user {userId}: {ex.Message}");
        //     }
        // }

        // /// <summary>
        // /// Gets multiple cards by their IDs
        // /// </summary>
        // public List<Card> GetCardsByIds(List<int> cardIds)
        // {
        //     var cards = new List<Card>();
        //     if (!cardIds.Any()) return cards;

        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = $"SELECT * FROM cards WHERE id = ANY(@cardIds)";
        //     command.Parameters.AddWithValue("@cardIds", cardIds.ToArray());

        //     try
        //     {
        //         using var reader = command.ExecuteReader();
        //         while (reader.Read())
        //         {
        //             Card card = CreateCardFromDatabaseRow(reader);
        //             cards.Add(card);
        //         }
        //         return cards;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to get cards by IDs: {ex.Message}");
        //     }
        // }

        // Validation Operations

        /// <summary>
        /// Checks if a user owns a specific card
        /// </summary>
        // public bool ValidateCardOwnership(int cardId, int userId)
        // {
        //     using var connection = _databaseHandler.GetConnection();
        //     connection.Open();

        //     using var command = connection.CreateCommand();
        //     command.CommandText = "SELECT COUNT(*) FROM cards WHERE id = @cardId AND user_id = @userId";
        //     command.Parameters.AddWithValue("@cardId", cardId);
        //     command.Parameters.AddWithValue("@userId", userId);

        //     try
        //     {
        //         return Convert.ToInt32(command.ExecuteScalar()) > 0;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to validate card ownership: {ex.Message}");
        //     }
        // }

        // Helper Methods

        /// <summary>
        /// Creates a card object from database row data
        /// </summary>
        private Card CreateCardFromDatabaseRow(NpgsqlDataReader reader)
        {
            // Read basic card information
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            int damage = reader.GetInt32(reader.GetOrdinal("damage"));
            ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
            CardType cardType = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

            // Create appropriate card type
            if (cardType == CardType.Spell)
            {
                return new SpellCard(id, name, damage, elementType);
            }
            else // CardType.Monster
            {
                MonsterType monsterType = DetermineMonsterType(name);
                return new MonsterCard(id, name, damage, elementType, monsterType);
            }
        }

        private MonsterType DetermineMonsterType(string name)
        {
            if (name.Contains("Goblin")) return MonsterType.Goblin;
            if (name.Contains("Dragon")) return MonsterType.Dragon;
            if (name.Contains("Wizard")) return MonsterType.Wizard;
            if (name.Contains("Ork")) return MonsterType.Ork;
            if (name.Contains("Knight")) return MonsterType.Knight;
            if (name.Contains("Kraken")) return MonsterType.Kraken;
            if (name.Contains("FireElf")) return MonsterType.FireElf;

            return MonsterType.Goblin; // Default type
        }
    }
}
