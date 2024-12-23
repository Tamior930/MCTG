using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;

namespace MCTG.PresentationLayer.Services
{
    public class CardService
    {
        private readonly ICardRepository _cardRepository;
        private const int PACKAGE_SIZE = 5;
        private const int PACKAGE_COST = 5;

        private static readonly (string Name, int MinDamage, int MaxDamage)[] MONSTER_TEMPLATES = new[]
        {
            ("Goblin", 5, 15),
            ("Dragon", 40, 60),
            ("Wizard", 20, 35),
            ("Ork", 25, 45),
            ("Knight", 20, 40),
            ("Kraken", 35, 55),
            ("FireElf", 15, 25)
        };

        private static readonly (string Name, int MinDamage, int MaxDamage)[] SPELL_TEMPLATES = new[]
        {
            ("WaterSpell", 20, 40),
            ("FireSpell", 25, 45),
            ("RegularSpell", 15, 35)
        };

        public CardService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        public List<Card> CreateRandomPackage()
        {
            var package = new List<Card>();
            var random = new Random();

            for (int i = 0; i < PACKAGE_SIZE; i++)
            {
                // 60% chance for monster, 40% for spell
                bool isMonster = random.Next(100) < 60;
                var template = isMonster 
                    ? MONSTER_TEMPLATES[random.Next(MONSTER_TEMPLATES.Length)]
                    : SPELL_TEMPLATES[random.Next(SPELL_TEMPLATES.Length)];

                var elementType = (ElementType)random.Next(3); // 0=Fire, 1=Water, 2=Normal
                var damage = random.Next(template.MinDamage, template.MaxDamage + 1);

                Card card = isMonster
                    ? new MonsterCard(template.Name, damage, elementType)
                    : new SpellCard(template.Name, damage, elementType);

                package.Add(card);
            }

            return package;
        }

        public List<Card> GetCardsByIds(List<string> cardIds)
        {
            var cards = new List<Card>();
            foreach (var idString in cardIds)
            {
                if (int.TryParse(idString, out int cardId))
                {
                    var card = _cardRepository.GetCardById(cardId);
                    if (card != null)
                    {
                        cards.Add(card);
                    }
                }
            }
            return cards;
        }

        public bool IsCardAvailableForTrade(int cardId, int userId)
        {
            var card = _cardRepository.GetCardById(cardId);
            return card != null && !_cardRepository.IsCardInDeck(cardId);
        }

        public bool AssignCardsToUser(List<Card> cards, int userId)
        {
            try
            {
                foreach (var card in cards)
                {
                    _cardRepository.AddCard(card, userId);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int GetPackageCost()
        {
            return PACKAGE_COST;
        }

        public static int GetPackageSize()
        {
            return PACKAGE_SIZE;
        }
    }
}
