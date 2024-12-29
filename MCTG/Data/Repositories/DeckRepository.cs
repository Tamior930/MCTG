using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly DatabaseHandler _databaseHandler;
        private const int MAX_CARDS_IN_DECK = 4;

        public DeckRepository()
        {
            _databaseHandler = new DatabaseHandler();
        }

        // Basic CRUD Operations

        /// <summary>
        /// Saves a new deck for a user by replacing all their current cards
        /// </summary>
        public void SaveDeck(int userId, List<Card> cards)
        {
            // Input validation
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards), "Cards list cannot be null");
            }

            if (cards.Count > MAX_CARDS_IN_DECK)
            {
                throw new InvalidOperationException($"Deck cannot contain more than {MAX_CARDS_IN_DECK} cards");
            }

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Step 1: Remove all cards from current deck
                RemoveAllCardsFromDeck(userId, connection);

                // Step 2: Add new cards to deck
                AddCardsToUserDeck(userId, cards, connection);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Failed to save deck: " + ex.Message);
            }
        }

        private void RemoveAllCardsFromDeck(int userId, NpgsqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET in_deck = false WHERE user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }

        private void AddCardsToUserDeck(int userId, List<Card> cards, NpgsqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET in_deck = true WHERE id = @cardId AND user_id = @userId";

            foreach (var card in cards)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@cardId", card.Id);
                command.Parameters.AddWithValue("@userId", userId);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"Card with ID {card.Id} does not belong to user {userId}");
                }
            }
        }

        /// <summary>
        /// Gets all cards in a user's deck
        /// </summary>
        public List<Card> GetDeckByUserId(int userId)
        {
            var userCards = new List<Card>();

            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name, damage, element_type, card_type 
                FROM cards 
                WHERE user_id = @userId AND in_deck = true";
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Card card = CreateCardFromDatabaseRow(reader);
                userCards.Add(card);
            }

            return userCards;
        }

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
            else
            {
                MonsterType monsterType = DetermineMonsterType(name);
                return new MonsterCard(id, name, damage, elementType, monsterType);
            }
        }

        /// <summary>
        /// Adds a single card to user's deck
        /// </summary>
        public void AddCardToDeck(int userId, Card card)
        {
            // Check if deck is full
            int currentDeckSize = GetDeckCount(userId);
            if (currentDeckSize >= MAX_CARDS_IN_DECK)
            {
                throw new InvalidOperationException($"Cannot add card. Deck already has maximum number of cards ({MAX_CARDS_IN_DECK})");
            }

            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET in_deck = true WHERE id = @cardId AND user_id = @userId";
            command.Parameters.AddWithValue("@cardId", card.Id);
            command.Parameters.AddWithValue("@userId", userId);

            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Could not add card {card.Id} to deck. Card either doesn't exist or doesn't belong to user {userId}");
            }
        }

        /// <summary>
        /// Removes a single card from user's deck
        /// </summary>
        public void RemoveCardFromDeck(int userId, Card card)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET in_deck = false WHERE id = @cardId AND user_id = @userId";
            command.Parameters.AddWithValue("@cardId", card.Id);
            command.Parameters.AddWithValue("@userId", userId);

            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Could not remove card {card.Id}. Card is not in user's deck");
            }
        }

        /// <summary>
        /// Removes all cards from a user's deck
        /// </summary>
        public void ClearDeck(int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                RemoveAllCardsFromDeck(userId, connection);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Failed to clear deck: " + ex.Message);
            }
        }

        // Deck Management Operations

        /// <summary>
        /// Gets a random card from user's deck (used in battles)
        /// </summary>
        public Card GetRandomCardFromDeck(int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name, damage, element_type, card_type 
                FROM cards 
                WHERE user_id = @userId AND in_deck = true 
                ORDER BY RANDOM() 
                LIMIT 1";
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return CreateCardFromDatabaseRow(reader);
            }

            return null!;
        }

        /// <summary>
        /// Transfers a card between two users' decks (used in trading)
        /// </summary>
        public bool TransferCardBetweenDecks(int cardId, int fromUserId, int toUserId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Check if card belongs to fromUser
                if (!VerifyCardOwnership(cardId, fromUserId, connection))
                {
                    return false;
                }

                // Transfer the card
                TransferCard(cardId, toUserId, connection);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        private bool VerifyCardOwnership(int cardId, int userId, NpgsqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM cards WHERE id = @cardId AND user_id = @userId AND in_deck = true";
            command.Parameters.AddWithValue("@cardId", cardId);
            command.Parameters.AddWithValue("@userId", userId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private void TransferCard(int cardId, int newUserId, NpgsqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET user_id = @newUserId, in_deck = true WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", cardId);
            command.Parameters.AddWithValue("@newUserId", newUserId);
            command.ExecuteNonQuery();
        }

        public void SetCardInDeck(int cardId, bool inDeck)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET in_deck = @inDeck WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", cardId);
            command.Parameters.AddWithValue("@inDeck", inDeck);

            command.ExecuteNonQuery();
        }

        // Validation Operations

        /// <summary>
        /// Gets the number of cards in a user's deck
        /// </summary>
        public int GetDeckCount(int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM cards WHERE user_id = @userId AND in_deck = true";
            command.Parameters.AddWithValue("@userId", userId);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Checks if a deck has exactly 4 cards (required for battles)
        /// </summary>
        public bool IsValidDeck(int userId)
        {
            int deckSize = GetDeckCount(userId);
            return deckSize == MAX_CARDS_IN_DECK;
        }

        /// <summary>
        /// Checks if a specific card is in any deck
        /// </summary>
        public bool IsCardInDeck(int cardId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM cards WHERE id = @cardId AND in_deck = true";
            command.Parameters.AddWithValue("@cardId", cardId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        // Helper Methods

        /// <summary>
        /// Determines the monster type based on the card name
        /// </summary>
        private MonsterType DetermineMonsterType(string cardName)
        {
            // Check card name for specific monster types
            if (cardName.Contains("Goblin")) return MonsterType.Goblin;
            if (cardName.Contains("Dragon")) return MonsterType.Dragon;
            if (cardName.Contains("Wizard")) return MonsterType.Wizard;
            if (cardName.Contains("Ork")) return MonsterType.Ork;
            if (cardName.Contains("Knight")) return MonsterType.Knight;
            if (cardName.Contains("Kraken")) return MonsterType.Kraken;
            if (cardName.Contains("FireElf")) return MonsterType.FireElf;

            // Default to Goblin if no specific type is found
            return MonsterType.Goblin;
        }
    }
}