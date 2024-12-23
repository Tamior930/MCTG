using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly DatabaseHandler _dbHandler;

        public DeckRepository()
        {
            _dbHandler = new DatabaseHandler();
        }

        public void SaveDeck(int userId, List<Card> cards)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // First, remove all cards from the user's deck
                using var clearCmd = conn.CreateCommand();
                clearCmd.CommandText = "UPDATE cards SET in_deck = false WHERE user_id = @userId";
                clearCmd.Parameters.AddWithValue("@userId", userId);
                clearCmd.ExecuteNonQuery();

                // Then add the new cards to the deck
                using var addCmd = conn.CreateCommand();
                addCmd.CommandText = "UPDATE cards SET in_deck = true WHERE id = @cardId AND user_id = @userId";
                
                foreach (var card in cards)
                {
                    addCmd.Parameters.Clear();
                    addCmd.Parameters.AddWithValue("@cardId", card.Id);
                    addCmd.Parameters.AddWithValue("@userId", userId);
                    addCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Card> GetDeckByUserId(int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, name, damage, element_type, card_type 
                FROM cards 
                WHERE user_id = @userId AND in_deck = true";
            cmd.Parameters.AddWithValue("@userId", userId);

            var cards = new List<Card>();
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("id"));
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
                CardType type = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

                Card card = type == CardType.Spell
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
                card.Id = id;
                cards.Add(card);
            }

            return cards;
        }

        public void AddCardToDeck(int userId, Card card)
        {
            if (GetDeckCount(userId) >= 4)
            {
                throw new InvalidOperationException("Deck already has maximum number of cards (4)");
            }

            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE cards SET in_deck = true WHERE id = @cardId AND user_id = @userId";
            cmd.Parameters.AddWithValue("@cardId", card.Id);
            cmd.Parameters.AddWithValue("@userId", userId);

            if (cmd.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Card not found or doesn't belong to user");
            }
        }

        public void RemoveCardFromDeck(int userId, Card card)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE cards SET in_deck = false WHERE id = @cardId AND user_id = @userId";
            cmd.Parameters.AddWithValue("@cardId", card.Id);
            cmd.Parameters.AddWithValue("@userId", userId);

            if (cmd.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Card not found in user's deck");
            }
        }

        public int GetDeckCount(int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM cards WHERE user_id = @userId AND in_deck = true";
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool IsValidDeck(int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM cards WHERE user_id = @userId AND in_deck = true";
            cmd.Parameters.AddWithValue("@userId", userId);

            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count == 4; // A valid deck must have exactly 4 cards
        }

        public Card GetRandomCardFromDeck(int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, name, damage, element_type, card_type 
                FROM cards 
                WHERE user_id = @userId AND in_deck = true 
                ORDER BY RANDOM() 
                LIMIT 1";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("id"));
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
                CardType type = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

                Card card = type == CardType.Spell
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
                card.Id = id;
                return card;
            }

            throw new InvalidOperationException("No cards found in deck");
        }

        public bool TransferCardBetweenDecks(int cardId, int fromUserId, int toUserId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // First verify the card belongs to fromUserId and is in their deck
                using var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = @"
                    SELECT COUNT(*) FROM cards 
                    WHERE id = @cardId AND user_id = @fromUserId AND in_deck = true";
                checkCmd.Parameters.AddWithValue("@cardId", cardId);
                checkCmd.Parameters.AddWithValue("@fromUserId", fromUserId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Transfer the card
                using var transferCmd = conn.CreateCommand();
                transferCmd.CommandText = @"
                    UPDATE cards 
                    SET user_id = @toUserId, in_deck = true 
                    WHERE id = @cardId";
                transferCmd.Parameters.AddWithValue("@cardId", cardId);
                transferCmd.Parameters.AddWithValue("@toUserId", toUserId);

                transferCmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}