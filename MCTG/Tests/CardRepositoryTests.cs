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
            // Initialize the CardRepository and open a database connection
            _cardRepository = new CardRepository();
            // Use the same connection string as in DatabaseHandler
            _connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=mtcg_user;Password=mtcg_password;Database=mtcg_db");
            _connection.Open();
        }

        [Test]
        public void UpdateCardOwnership_SuccessfulUpdate_ReturnsTrue()
        {
            // Arrange: Set up a card and a new owner ID for a successful update
            var card = new SpellCard(1, "TestCard", 10, ElementType.Fire);
            int newOwnerId = 2;

            // Act: Attempt to update the card's ownership
            bool result = _cardRepository.UpdateCardOwnership(card, newOwnerId);

            // Assert: Verify that the update was successful
            Assert.That(result, Is.True);

            // Verify in database: Check that the card's owner ID was updated in the database
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT user_id FROM cards WHERE id = @cardId";
            command.Parameters.AddWithValue("@cardId", card.Id);
            var actualOwnerId = Convert.ToInt32(command.ExecuteScalar());
            Assert.That(actualOwnerId, Is.EqualTo(newOwnerId));
        }

        [Test]
        public void UpdateCardOwnership_NonExistentCard_ReturnsFalse()
        {
            // Arrange: Set up a non-existent card ID
            var card = new SpellCard(999999, "NonExistentCard", 10, ElementType.Fire);
            int newOwnerId = 2;

            // Act: Attempt to update the card's ownership
            bool result = _cardRepository.UpdateCardOwnership(card, newOwnerId);

            // Assert: Verify that the update failed
            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateCardOwnership_InvalidOwnerId_ReturnsFalse()
        {
            // Arrange: Set up a card and an invalid owner ID
            var card = new SpellCard(1, "TestCard", 10, ElementType.Fire);
            int invalidOwnerId = -1;

            // Act: Attempt to update the card's ownership
            bool result = _cardRepository.UpdateCardOwnership(card, invalidOwnerId);

            // Assert: Verify that the update failed
            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateCardOwnership_VerifyDeckReset()
        {
            // Arrange: Set up a card and a new owner ID
            var card = new SpellCard(1, "TestCard", 10, ElementType.Fire);
            int newOwnerId = 2;

            // Act: Update the card's ownership and verify deck reset
            bool result = _cardRepository.UpdateCardOwnership(card, newOwnerId);

            // Assert: Verify that the card is no longer in the deck and its deck order is reset
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
            // Close and dispose of the database connection
            _connection.Close();
            _connection.Dispose();
        }
    }
}