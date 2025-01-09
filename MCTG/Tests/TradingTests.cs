using MCTG.Business.Models;
using MCTG.Data.Interfaces;
using MCTG.Presentation.Services;
using Moq;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class TradingTests
    {
        private TradingService _tradingService = null!;
        private Mock<ITradeRepository> _mockTradeRepo = null!;
        private Mock<ICardRepository> _mockCardRepo = null!;
        private Mock<IDeckRepository> _mockDeckRepo = null!;
        private User _testUser = null!;
        private Card _testCard = null!;

        [SetUp]
        public void Setup()
        {
            _mockTradeRepo = new Mock<ITradeRepository>();
            _mockCardRepo = new Mock<ICardRepository>();
            _mockDeckRepo = new Mock<IDeckRepository>();

            _tradingService = new TradingService(
                _mockTradeRepo.Object,
                _mockCardRepo.Object,
                _mockDeckRepo.Object
            );

            _testUser = new User("testUser", "password");
            _testUser.SetId(1);

            _testCard = new SpellCard(1, "TestSpell", 50, ElementType.Fire);
        }

        [Test]
        public void CreateTradingDeal_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var trade = new Trade(0, 1, 0, "spell", null, null, 50, true);

            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(1)).Returns(false);
            _mockTradeRepo.Setup(r => r.IsCardInTrade(1)).Returns(false);
            _mockTradeRepo.Setup(r => r.CreateTrade(It.IsAny<Trade>())).Returns(true);

            // Act
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert
            Assert.That(result, Is.EqualTo("Trading deal created successfully"));
        }

        [Test]
        public void CreateTradingDeal_WithCardInDeck_ReturnsError()
        {
            // Arrange
            var trade = new Trade(0, 1, 0, "spell", null, null, 50, true);

            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(1)).Returns(true);

            // Act
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert
            Assert.That(result, Is.EqualTo("Error: Cannot trade cards that are in your deck"));
        }

        [Test]
        public void CreateTradingDeal_WithCardNotOwned_ReturnsError()
        {
            // Arrange
            var trade = new Trade(0, 1, 0, "spell", null, null, 50, true);

            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(false);

            // Act
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert
            Assert.That(result, Is.EqualTo("Error: You don't own this card"));
        }

        [Test]
        public void CreateTradingDeal_WithCardAlreadyInTrade_ReturnsError()
        {
            // Arrange
            var trade = new Trade(0, 1, 0, "spell", null, null, 50, true);

            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(1)).Returns(false);
            _mockTradeRepo.Setup(r => r.IsCardInTrade(1)).Returns(true);

            // Act
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert
            Assert.That(result, Is.EqualTo("Error: This card is already in a trading deal"));
        }

        [Test]
        public void ExecuteTrade_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var trade = new Trade(1, 1, 2, "spell", null, null, 50, true);
            var offeredCard = new SpellCard(2, "OfferedSpell", 60, ElementType.Water);

            _mockCardRepo.Setup(r => r.ValidateCardOwnership(2, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(2)).Returns(false);
            _mockTradeRepo.Setup(r => r.GetTradeById("1")).Returns(trade);
            _mockCardRepo.Setup(r => r.GetCardById(2)).Returns(offeredCard);
            _mockTradeRepo.Setup(r => r.ExecuteTrade("1", 2, 1)).Returns(true);

            // Act
            var result = _tradingService.ExecuteTrade("1", 2, 1);

            // Assert
            Assert.That(result, Is.EqualTo("Trading deal successfully executed"));
        }
    }
}