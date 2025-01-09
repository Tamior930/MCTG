using MCTG.Business.Models;
using MCTG.Data.Repositories;
using Npgsql;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class CardRepositoryTests
    {
        private CardRepository _cardRepository = null!;
        private NpgsqlConnection _connection = null!;

        [SetUp]
        public void Setup()
        {
            _cardRepository = new CardRepository();
            // Verwenden Sie die gleiche Verbindungszeichenfolge wie in DatabaseHandler
            _connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=mtcg_user;Password=mtcg_password;Database=mtcg_db");
            _connection.Open();
        }

        [Test]
        public void UpdateCardOwnership_SuccessfulUpdate_ReturnsTrue()
        {
            // Arrange
            var card = new SpellCard(1, "TestCard", 10, ElementType.Fire);
            int newOwnerId = 2;

            // Act
            bool result = _cardRepository.UpdateCardOwnership(card, newOwnerId);

            // Assert
            Assert.That(result, Is.True);

            // Verify in database
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT user_id FROM cards WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", card.Id);
            var actualOwnerId = Convert.ToInt32(command.ExecuteScalar());
            Assert.That(actualOwnerId, Is.EqualTo(newOwnerId));
        }

        [Test]
        public void UpdateCardOwnership_NonExistentCard_ReturnsFalse()
        {
            // Arrange
            var card = new SpellCard(999999, "NonExistentCard", 10, ElementType.Fire);
            int newOwnerId = 2;

            // Act
            bool result = _cardRepository.UpdateCardOwnership(card, newOwnerId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateCardOwnership_InvalidOwnerId_ReturnsFalse()
        {
            // Arrange
            var card = new SpellCard(1, "TestCard", 10, ElementType.Fire);
            int invalidOwnerId = -1;

            // Act
            bool result = _cardRepository.UpdateCardOwnership(card, invalidOwnerId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateCardOwnership_VerifyDeckReset()
        {
            // Arrange
            var card = new SpellCard(1, "TestCard", 10, ElementType.Fire);
            int newOwnerId = 2;

            // Act
            bool result = _cardRepository.UpdateCardOwnership(card, newOwnerId);

            // Assert
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT in_deck, deck_order FROM cards WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", card.Id);
            using var reader = command.ExecuteReader();

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.GetBoolean(0), Is.False); // in_deck should be false
            Assert.That(reader.IsDBNull(1), Is.True);    // deck_order should be NULL
        }

        [TearDown]
        public void Cleanup()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}