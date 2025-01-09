using MCTG.Business.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly DatabaseHandler _databaseHandler;
        private const int MAX_DECK_SIZE = 4;

        public DeckRepository()
        {
            _databaseHandler = new DatabaseHandler();
        }

        public List<Card> GetDeckCards(int userId)
        {
            var deckCards = new List<Card>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT * FROM cards 
                WHERE user_id = @userId AND in_deck = true 
                ORDER BY deck_order";
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                deckCards.Add(CreateCardFromDatabaseRow(reader));
            }
            return deckCards;
        }

        private Card CreateCardFromDatabaseRow(NpgsqlDataReader reader)
        {
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            int damage = reader.GetInt32(reader.GetOrdinal("damage"));
            ElementType element = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
            string cardType = reader.GetString(reader.GetOrdinal("card_type"));

            if (cardType == "Monster")
            {
                MonsterType monsterType = Enum.Parse<MonsterType>(reader.GetString(reader.GetOrdinal("monster_type")));
                return new MonsterCard(id, name, damage, element, monsterType);
            }
            else
            {
                return new SpellCard(id, name, damage, element);
            }
        }

        public bool SaveDeck(int userId, List<Card> cards)
        {
            if (cards.Count != MAX_DECK_SIZE)
                return false;

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var clearCommand = connection.CreateCommand();
                clearCommand.CommandText = @"
                    UPDATE cards 
                    SET in_deck = false, deck_order = NULL 
                    WHERE user_id = @userId";
                clearCommand.Parameters.AddWithValue("@userId", userId);
                clearCommand.ExecuteNonQuery();

                for (int i = 0; i < cards.Count; i++)
                {
                    using var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = @"
                        UPDATE cards 
                        SET in_deck = true, deck_order = @order 
                        WHERE id = @cardId AND user_id = @userId";
                    updateCommand.Parameters.AddWithValue("@order", i + 1);
                    updateCommand.Parameters.AddWithValue("@cardId", cards[i].Id);
                    updateCommand.Parameters.AddWithValue("@userId", userId);
                    updateCommand.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool IsCardInDeck(int cardId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT in_deck FROM cards WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", cardId);

            using var reader = command.ExecuteReader();
            return reader.Read() && reader.GetBoolean(0);
        }
    }
}