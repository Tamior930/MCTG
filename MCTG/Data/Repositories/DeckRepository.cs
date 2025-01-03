using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly DatabaseHandler _databaseHandler;

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

            command.CommandText = "SELECT * FROM cards WHERE user_id = @userId AND in_deck = true";
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
            // Get basic card info from database
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            int damage = reader.GetInt32(reader.GetOrdinal("damage"));
            ElementType element = (ElementType)reader.GetInt32(reader.GetOrdinal("element_type"));
            string cardType = reader.GetString(reader.GetOrdinal("type"));

            // Create either a Monster or Spell card
            if (cardType == "Monster")
            {
                MonsterType monsterType = (MonsterType)reader.GetInt32(reader.GetOrdinal("monster_type"));
                return new MonsterCard(id, name, damage, element, monsterType);
            }
            else
            {
                return new SpellCard(id, name, damage, element);
            }
        }

        public bool SaveDeck(int userId, List<Card> cards)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, remove all cards from user's deck
                using var clearCommand = connection.CreateCommand();
                clearCommand.CommandText = "UPDATE cards SET in_deck = false WHERE user_id = @userId";
                clearCommand.Parameters.AddWithValue("@userId", userId);
                clearCommand.ExecuteNonQuery();

                // Then, add new cards to deck
                using var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE cards SET in_deck = true WHERE id = ANY(@cardIds)";
                updateCommand.Parameters.AddWithValue("@cardIds", cards.Select(c => c.Id).ToArray());
                updateCommand.ExecuteNonQuery();

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