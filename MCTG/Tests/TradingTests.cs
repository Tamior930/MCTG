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
            // Initialize mock repositories and the trading service
            _mockTradeRepo = new Mock<ITradeRepository>();
            _mockCardRepo = new Mock<ICardRepository>();
            _mockDeckRepo = new Mock<IDeckRepository>();

            _tradingService = new TradingService(
                _mockTradeRepo.Object,
                _mockCardRepo.Object,
                _mockDeckRepo.Object
            );

            // Create a test user and card
            _testUser = new User("testUser", "password");
            _testUser.SetId(1);

            _testCard = new SpellCard(1, "TestSpell", 50, ElementType.Fire);
        }

        [Test]
        public void CreateTradingDeal_WithValidData_ReturnsSuccess()
        {
            // Arrange: Set up a valid trade scenario
            var trade = new Trade(1, "spell", 50);

            // Mock the repository methods to simulate valid conditions
            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(1)).Returns(false);
            _mockTradeRepo.Setup(r => r.IsCardInTrade(1)).Returns(false);
            _mockTradeRepo.Setup(r => r.CreateTrade(It.IsAny<Trade>())).Returns(true);

            // Act: Attempt to create a trading deal
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert: Verify that the trading deal was created successfully
            Assert.That(result, Is.EqualTo("Trading deal created successfully"));
        }

        [Test]
        public void CreateTradingDeal_WithCardInDeck_ReturnsError()
        {
            // Arrange: Set up a scenario where the card is in the user's deck
            var trade = new Trade(1, "spell", 50);
            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(1)).Returns(true);

            // Act: Attempt to create a trading deal
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert: Verify that an error message is returned
            Assert.That(result, Is.EqualTo("Error: Cannot trade cards that are in your deck"));
        }

        [Test]
        public void CreateTradingDeal_WithCardNotOwned_ReturnsError()
        {
            // Arrange: Set up a scenario where the user does not own the card
            var trade = new Trade(1, "spell", 50);
            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(false);

            // Act: Attempt to create a trading deal
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert: Verify that an error message is returned
            Assert.That(result, Is.EqualTo("Error: You don't own this card"));
        }

        [Test]
        public void CreateTradingDeal_WithCardAlreadyInTrade_ReturnsError()
        {
            // Arrange: Set up a scenario where the card is already in a trade
            var trade = new Trade(1, "spell", 50);
            _mockCardRepo.Setup(r => r.ValidateCardOwnership(1, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(1)).Returns(false);
            _mockTradeRepo.Setup(r => r.IsCardInTrade(1)).Returns(true);

            // Act: Attempt to create a trading deal
            var result = _tradingService.CreateTradingDeal(_testUser, trade);

            // Assert: Verify that an error message is returned
            Assert.That(result, Is.EqualTo("Error: This card is already in a trading deal"));
        }

        [Test]
        public void ExecuteTrade_WithValidData_ReturnsSuccess()
        {
            // Arrange: Set up a valid trade execution scenario
            var trade = new Trade(1, "spell", 50);
            trade.Id = 1;
            trade.UserId = 2;
            var offeredCard = new SpellCard(2, "OfferedSpell", 60, ElementType.Water);
            var tradedCard = new SpellCard(1, "TestSpell", 50, ElementType.Fire);

            // Mock the repository methods to simulate valid conditions
            _mockCardRepo.Setup(r => r.ValidateCardOwnership(2, 1)).Returns(true);
            _mockDeckRepo.Setup(r => r.IsCardInDeck(2)).Returns(false);
            _mockTradeRepo.Setup(r => r.GetTradeById(1)).Returns(trade);
            _mockCardRepo.Setup(r => r.GetCardById(2)).Returns(offeredCard);
            _mockCardRepo.Setup(r => r.GetCardById(1)).Returns(tradedCard);
            _mockTradeRepo.Setup(r => r.ExecuteTrade(1, 2, 1)).Returns(true);

            // Act: Attempt to execute the trade
            var result = _tradingService.ExecuteTrade(1, 2, 1);

            // Assert: Verify that the trade was executed successfully
            Assert.That(result, Is.EqualTo("Trading deal successfully executed"));
        }
    }
}