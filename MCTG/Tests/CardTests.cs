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
            _cardRepository = new CardRepository(); // We don't need DB for these tests
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
            // Act
            var result = _cardRepository.DetermineMonsterType(cardName);

            // Assert
            Assert.That(result, Is.EqualTo(expectedType));
        }

        [Test]
        public void DetermineMonsterType_InvalidName_ThrowsArgumentException()
        {
            // Arrange
            string invalidName = "InvalidMonster";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _cardRepository.DetermineMonsterType(invalidName));
        }

        [Test]
        public void GenerateRandomCard_GeneratesValidName()
        {
            // Act
            var card = Card.GenerateRandomCard();

            // Assert
            Assert.That(card.Name, Is.Not.Null.Or.Empty);
            if (card is MonsterCard monsterCard)
            {
                Assert.DoesNotThrow(() => _cardRepository.DetermineMonsterType(card.Name));
            }
            else
            {
                Assert.That(card.Name, Does.Contain("Spell"));
            }
        }
    }
}