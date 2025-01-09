using MCTG.Business.Models;
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

        public bool UpdateCardOwnership(Card card, int newOwnerId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE cards 
                    SET user_id = @newOwnerId,
                        in_deck = false,
                        deck_order = NULL
                    WHERE id = @cardId
                    RETURNING id";

                command.Parameters.AddWithValue("@newOwnerId", newOwnerId);
                command.Parameters.AddWithValue("@cardId", card.Id);

                var result = command.ExecuteScalar();

                if (result != null)
                {
                    transaction.Commit();
                    return true;
                }

                transaction.Rollback();
                return false;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }

        public Card? GetCardById(int cardId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM cards WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", cardId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? CreateCardFromDatabaseRow(reader) : null;
        }

        public List<Card> GetAllCardsForUser(int userId)
        {
            var userCards = new List<Card>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cards WHERE user_id = @userId ORDER BY id";
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                userCards.Add(CreateCardFromDatabaseRow(reader));
            }
            return userCards;
        }

        public List<Card> GetRandomCardsForPackage(int count)
        {
            var cards = new List<Card>();
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var card = Card.GenerateRandomCard();

                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO cards (name, damage, element_type, card_type, monster_type)
                        VALUES (@name, @damage, @elementType, @cardType, @monsterType)
                        RETURNING id";

                    command.Parameters.AddWithValue("@name", card.Name);
                    command.Parameters.AddWithValue("@damage", card.Damage);
                    command.Parameters.AddWithValue("@elementType", card.ElementType.ToString());
                    command.Parameters.AddWithValue("@cardType", card.Type.ToString());

                    if (card is MonsterCard monsterCard)
                    {
                        command.Parameters.AddWithValue("@monsterType", monsterCard.MonsterType.ToString());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@monsterType", DBNull.Value);
                    }

                    card.Id = Convert.ToInt32(command.ExecuteScalar());
                    cards.Add(card);
                }

                transaction.Commit();
                return cards;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating package: {ex.Message}");
                transaction.Rollback();
                return new List<Card>();
            }
        }

        public bool ValidateCardOwnership(int cardId, int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT COUNT(*) FROM cards WHERE id = @cardId AND user_id = @userId";
            command.Parameters.AddWithValue("@cardId", cardId);
            command.Parameters.AddWithValue("@userId", userId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private Card CreateCardFromDatabaseRow(NpgsqlDataReader reader)
        {
            int id = reader.GetInt32(reader.GetOrdinal("id"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            int damage = reader.GetInt32(reader.GetOrdinal("damage"));
            ElementType elementType = Enum.Parse<ElementType>(reader.GetString(reader.GetOrdinal("element_type")));
            CardType cardType = Enum.Parse<CardType>(reader.GetString(reader.GetOrdinal("card_type")));

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

        public MonsterType DetermineMonsterType(string name)
        {
            foreach (MonsterType monsterType in Enum.GetValues<MonsterType>())
            {
                if (name.Contains(monsterType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return monsterType;
                }
            }

            throw new ArgumentException($"Could not determine monster type from name: {name}");
        }
    }
}
