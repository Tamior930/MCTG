using MCTG.BusinessLayer.Models;

namespace MCTG.BusinessLayer.Controller
{
    public class TradingMechanism
    {
        public class TradeRequirement
        {
            public bool RequiresSpell { get; set; }
            public bool RequiresMonster { get; set; }
            public ElementType? RequiredElementType { get; set; }
            public int MinimumDamage { get; set; }
        }

        public class TradeOffer
        {
            public string Id { get; } = Guid.NewGuid().ToString();
            public string OfferingUserId { get; set; }
            public Card OfferedCard { get; set; }
            public TradeRequirement Requirements { get; set; }
            public bool IsActive { get; set; } = true;
        }

        private readonly List<TradeOffer> _activeTradeOffers = new();

        public string CreateTradeOffer(User user, Card cardToTrade, TradeRequirement requirements)
        {
            if (!user.IsCardAvailableForTrade(cardToTrade))
            {
                return "Error: Card must be in your stack but not in your deck to be traded.";
            }

            var tradeOffer = new TradeOffer
            {
                OfferingUserId = user.Username,
                OfferedCard = cardToTrade,
                Requirements = requirements
            };

            _activeTradeOffers.Add(tradeOffer);
            user.RemoveCard(cardToTrade);

            return $"Trade offer created successfully. Trade ID: {tradeOffer.Id}";
        }

        public string AcceptTrade(string tradeId, User acceptingUser, Card offeredCard)
        {
            var tradeOffer = _activeTradeOffers.FirstOrDefault(t => t.Id == tradeId && t.IsActive);
            
            if (tradeOffer == null)
            {
                return "Error: Trade offer not found or no longer active.";
            }

            if (acceptingUser.Username == tradeOffer.OfferingUserId)
            {
                return "Error: Cannot trade with yourself.";
            }

            if (!acceptingUser.IsCardAvailableForTrade(offeredCard))
            {
                return "Error: Offered card must be in your stack but not in your deck.";
            }

            // Verify requirements
            if (!MeetsTradeRequirements(offeredCard, tradeOffer.Requirements))
            {
                return "Error: Offered card does not meet trade requirements.";
            }

            // Execute trade
            var offeringUser = GetUserByUsername(tradeOffer.OfferingUserId); // You'll need to implement this
            if (offeringUser == null)
            {
                return "Error: Original trading user not found.";
            }

            // Exchange cards
            acceptingUser.RemoveCard(offeredCard);
            acceptingUser.AddCard(tradeOffer.OfferedCard);
            offeringUser.AddCard(offeredCard);

            // Close trade offer
            tradeOffer.IsActive = false;
            _activeTradeOffers.Remove(tradeOffer);

            return "Trade completed successfully!";
        }

        public List<TradeOffer> GetActiveTradeOffers()
        {
            return _activeTradeOffers.Where(t => t.IsActive).ToList();
        }

        public string DeleteTradeOffer(string tradeId, User requestingUser)
        {
            var tradeOffer = _activeTradeOffers.FirstOrDefault(t => t.Id == tradeId && t.IsActive);
            
            if (tradeOffer == null)
            {
                return "Error: Trade offer not found or no longer active.";
            }

            if (tradeOffer.OfferingUserId != requestingUser.Username)
            {
                return "Error: Only the creator of the trade offer can delete it.";
            }

            // Return card to user's stack
            requestingUser.AddCard(tradeOffer.OfferedCard);
            
            // Remove trade offer
            tradeOffer.IsActive = false;
            _activeTradeOffers.Remove(tradeOffer);

            return "Trade offer deleted successfully.";
        }

        private bool MeetsTradeRequirements(Card offeredCard, TradeRequirement requirements)
        {
            if (requirements.RequiresSpell && !(offeredCard is SpellCard))
                return false;

            if (requirements.RequiresMonster && !(offeredCard is MonsterCard))
                return false;

            if (requirements.RequiredElementType.HasValue && offeredCard.ElementType != requirements.RequiredElementType.Value)
                return false;

            if (offeredCard.Damage < requirements.MinimumDamage)
                return false;

            return true;
        }

        private User GetUserByUsername(string username)
        {
            // This should be implemented to fetch the user from your user repository
            // You'll need to inject IUserRepository or similar
            throw new NotImplementedException();
        }
    }
}
