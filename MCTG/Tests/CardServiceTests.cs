using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using MCTG.PresentationLayer.Services;
using Moq;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class CardServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ICardRepository> _cardRepositoryMock;
        private Mock<IDeckRepository> _deckRepositoryMock;
        private Mock<ITradeRepository> _tradeRepositoryMock;
        private CardService _cardService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _cardRepositoryMock = new Mock<ICardRepository>();
            _deckRepositoryMock = new Mock<IDeckRepository>();
            _tradeRepositoryMock = new Mock<ITradeRepository>();

            _cardService = new CardService(
                _userRepositoryMock.Object,
                _cardRepositoryMock.Object,
                _deckRepositoryMock.Object,
                _tradeRepositoryMock.Object
            );
        }

        [Test]
        public void PurchasePackage_InsufficientCoins_ReturnsErrorMessage()
        {
            // Arrange
            var user = new User("testuser", "password");
            user.SetId(1);
            user.UpdateStats(2, 100, 0, 0); // Set coins to 2 (less than package cost)

            // Act
            var result = _cardService.PurchasePackage(user);

            // Assert
            Assert.That(result, Is.EqualTo("Error: Insufficient coins"));
        }

        [Test]
        public void GetUserCards_UserHasNoCards_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            _cardRepositoryMock.Setup(x => x.GetAllCardsForUser(userId))
                .Returns(new List<Card>());

            // Act
            var result = _cardService.GetUserCards(userId);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ConfigureDeck_InvalidCardCount_ReturnsErrorMessage()
        {
            // Arrange
            int userId = 1;
            var cards = new List<Card>
            {
                new MonsterCard(1, "Dragon", 10, ElementType.Fire, MonsterType.Dragon),
                new MonsterCard(2, "Goblin", 5, ElementType.Normal, MonsterType.Goblin),
                new SpellCard(3, "Fireball", 8, ElementType.Fire)
            }; // Only 3 cards instead of required 4

            // Act
            var result = _cardService.ConfigureDeck(userId, cards);

            // Assert
            Assert.That(result, Does.StartWith("Error:"));
            Assert.That(result, Does.Contain("exactly 4 cards"));
        }

        [Test]
        public void PurchasePackage_SuccessfulPurchase_UpdatesUserCoinsAndAddsCards()
        {
            // Arrange
            var user = new User("testuser", "password");
            user.SetId(1);
            user.UpdateStats(20, 100, 0, 0); // Set initial coins to 20

            var packageCards = new List<Card>
            {
                new MonsterCard(1, "Dragon", 10, ElementType.Fire, MonsterType.Dragon),
                new MonsterCard(2, "Goblin", 5, ElementType.Normal, MonsterType.Goblin),
                new SpellCard(3, "Fireball", 8, ElementType.Fire),
                new MonsterCard(4, "Knight", 7, ElementType.Normal, MonsterType.Knight),
                new SpellCard(5, "WaterSpell", 9, ElementType.Water)
            };

            _cardRepositoryMock.Setup(x => x.GetRandomCardsForPackage(5))
                .Returns(packageCards);
            _userRepositoryMock.Setup(x => x.UpdateUserCoins(user.Id, -5))
                .Returns(true);

            // Act
            var result = _cardService.PurchasePackage(user);

            // Assert
            Assert.That(result, Does.Contain("successfully"));
            _cardRepositoryMock.Verify(x => x.GetRandomCardsForPackage(5), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateUserCoins(user.Id, -5), Times.Once);
        }

        [Test]
        public void GetUserDeck_ValidDeck_ReturnsDeckCards()
        {
            // Arrange
            int userId = 1;
            var expectedDeck = new List<Card>
            {
                new MonsterCard(1, "Dragon", 10, ElementType.Fire, MonsterType.Dragon),
                new MonsterCard(2, "Goblin", 5, ElementType.Normal, MonsterType.Goblin),
                new SpellCard(3, "Fireball", 8, ElementType.Fire),
                new SpellCard(4, "WaterSpell", 9, ElementType.Water)
            };

            _deckRepositoryMock.Setup(x => x.GetDeckByUserId(userId))
                .Returns(expectedDeck);

            // Act
            var result = _cardService.GetUserDeck(userId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDeck));
            Assert.That(result.Count, Is.EqualTo(4));
            _deckRepositoryMock.Verify(x => x.GetDeckByUserId(userId), Times.Once);
        }
    }
}