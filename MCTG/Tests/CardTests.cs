using MCTG.Business.Models;
using MCTG.Data.Repositories;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class CardTests
    {
        private CardRepository _cardRepository;

        [SetUp]
        public void Setup()
        {
            // Initialize the CardRepository
            _cardRepository = new CardRepository();
        }

        [Test]
        [TestCase("FireGoblin", MonsterType.Goblin)]
        [TestCase("WaterDragon", MonsterType.Dragon)]
        [TestCase("NormalKnight", MonsterType.Knight)]
        [TestCase("FireWizard", MonsterType.Wizard)]
        [TestCase("WaterKraken", MonsterType.Kraken)]
        [TestCase("NormalOrk", MonsterType.Ork)]
        [TestCase("FireFireElf", MonsterType.FireElf)]
        public void DetermineMonsterType_ValidNames_ReturnsCorrectType(string cardName, MonsterType expectedType)
        {
            // Act: Determine the monster type based on the card name
            var result = _cardRepository.DetermineMonsterType(cardName);

            // Assert: Verify that the correct monster type is returned
            Assert.That(result, Is.EqualTo(expectedType));
        }

        [Test]
        public void DetermineMonsterType_InvalidName_ThrowsArgumentException()
        {
            // Arrange: Set up an invalid card name
            string invalidName = "InvalidMonster";

            // Act & Assert: Verify that an ArgumentException is thrown for an invalid name
            Assert.Throws<ArgumentException>(() => _cardRepository.DetermineMonsterType(invalidName));
        }

        [Test]
        public void GenerateRandomCard_GeneratesValidName()
        {
            // Act: Generate a random card
            var card = Card.GenerateRandomCard();

            // Assert: Verify that the generated card has a valid name
            Assert.That(card.Name, Is.Not.Null.Or.Empty);
            if (card is MonsterCard monsterCard)
            {
                // If the card is a MonsterCard, ensure it has a valid monster type
                Assert.DoesNotThrow(() => _cardRepository.DetermineMonsterType(card.Name));
            }
            else
            {
                // If the card is not a MonsterCard, ensure it contains "Spell" in its name
                Assert.That(card.Name, Does.Contain("Spell"));
            }
        }
    }
}