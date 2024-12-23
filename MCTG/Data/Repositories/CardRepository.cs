using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly DatabaseHandler _dbHandler;

        public CardRepository()
        {
            _dbHandler = new DatabaseHandler();
        }

        public void AddCard(Card card, int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO cards (name, damage, element_type, card_type, user_id, in_deck)
                VALUES (@name, @damage, @elementType, @cardType, @userId, false)";

            cmd.Parameters.AddWithValue("@name", card.Name);
            cmd.Parameters.AddWithValue("@damage", card.Damage);
            cmd.Parameters.AddWithValue("@elementType", card.ElementType.ToString());
            cmd.Parameters.AddWithValue("@cardType", card.Type.ToString());
            cmd.Parameters.AddWithValue("@userId", userId);

            cmd.ExecuteNonQuery();
        }

        public Card GetCardById(int cardId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM cards WHERE id = @cardId";
            cmd.Parameters.AddWithValue("@cardId", cardId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
                CardType type = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

                return type == CardType.Spell
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
            }
            return null;
        }

        public List<Card> GetRandomCardsForPackage(int count)
        {
            var cards = new List<Card>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM cards WHERE user_id IS NULL ORDER BY RANDOM() LIMIT @count";
            cmd.Parameters.AddWithValue("@count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
                CardType type = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

                Card card = type == CardType.Spell
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
                
                cards.Add(card);
            }

            return cards;
        }

        public List<Card> GetAllCardsForUser(int userId)
        {
            var cards = new List<Card>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM cards WHERE user_id = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
                CardType type = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

                Card card = type == CardType.Spell
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
                
                cards.Add(card);
            }

            return cards;
        }

        public void RemoveCard(int cardId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM cards WHERE id = @cardId";
            cmd.Parameters.AddWithValue("@cardId", cardId);

            cmd.ExecuteNonQuery();
        }

        public bool UpdateCardOwner(int cardId, int newUserId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE cards SET user_id = @newUserId, in_deck = false WHERE id = @cardId";
            cmd.Parameters.AddWithValue("@cardId", cardId);
            cmd.Parameters.AddWithValue("@newUserId", newUserId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool IsCardInDeck(int cardId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT in_deck FROM cards WHERE id = @cardId";
            cmd.Parameters.AddWithValue("@cardId", cardId);

            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        public void SetCardInDeck(int cardId, bool inDeck)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE cards SET in_deck = @inDeck WHERE id = @cardId";
            cmd.Parameters.AddWithValue("@cardId", cardId);
            cmd.Parameters.AddWithValue("@inDeck", inDeck);

            cmd.ExecuteNonQuery();
        }

        public bool IsCardInTrade(int cardId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM trades WHERE card_id = @cardId AND status = 'ACTIVE'";
            cmd.Parameters.AddWithValue("@cardId", cardId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<Card> GetCardsByType(string cardType, int userId)
        {
            var cards = new List<Card>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM cards WHERE user_id = @userId AND card_type = @cardType";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@cardType", cardType);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
                CardType type = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

                Card card = type == CardType.Spell
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
                
                cards.Add(card);
            }

            return cards;
        }

        public List<Card> GetCardsByElement(ElementType elementType, int userId)
        {
            var cards = new List<Card>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM cards WHERE user_id = @userId AND element_type = @elementType";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@elementType", elementType.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("name"));
                int damage = reader.GetInt32(reader.GetOrdinal("damage"));

                Card card = name.ToLower().Contains("spell") 
                    ? new SpellCard(name, damage, elementType)
                    : new MonsterCard(name, damage, elementType);
                
                cards.Add(card);
            }

            return cards;
        }
    }
}
