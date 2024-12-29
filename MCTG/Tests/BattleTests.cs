using MCTG.BusinessLayer.Models;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class BattleTests
    {
        [Test]
        public void GoblinVsDragon_GoblinTooAfraid_Returns0Damage()
        {
            // Arrange
            var goblin = new MonsterCard(1, "Goblin", 15, ElementType.Normal, MonsterType.Goblin);
            var dragon = new MonsterCard(2, "Dragon", 50, ElementType.Fire, MonsterType.Dragon);

            // Act
            double damage = goblin.CalculateDamage(dragon);

            // Assert
            Assert.That(damage, Is.EqualTo(0));
        }

        [Test]
        public void FireElfVsDragon_FireElfEvades_ReturnsNormalDamage()
        {
            // Arrange
            var fireElf = new MonsterCard(1, "FireElf", 25, ElementType.Fire, MonsterType.FireElf);
            var dragon = new MonsterCard(2, "Dragon", 50, ElementType.Fire, MonsterType.Dragon);

            // Act
            double damage = fireElf.CalculateDamage(dragon);

            // Assert
            Assert.That(damage, Is.EqualTo(fireElf.Damage));
        }

        [Test]
        public void KrakenVsSpell_KrakenImmune_ReturnsNormalDamage()
        {
            // Arrange
            var kraken = new MonsterCard(1, "Kraken", 40, ElementType.Water, MonsterType.Kraken);
            var spell = new SpellCard(2, "WaterSpell", 30, ElementType.Water);

            // Act
            double damage = kraken.CalculateDamage(spell);

            // Assert
            Assert.That(damage, Is.EqualTo(kraken.Damage));
        }

        [Test]
        public void KnightVsWaterSpell_KnightDrowns_Returns0Damage()
        {
            // Arrange
            var knight = new MonsterCard(1, "Knight", 30, ElementType.Normal, MonsterType.Knight);
            var waterSpell = new SpellCard(2, "WaterSpell", 20, ElementType.Water);

            // Act
            double damage = knight.CalculateDamage(waterSpell);

            // Assert
            Assert.That(damage, Is.EqualTo(0));
        }

        [Test]
        public void WaterSpellVsFireMonster_DoubledDamage()
        {
            // Arrange
            var waterSpell = new SpellCard(1, "WaterSpell", 20, ElementType.Water);
            var fireMonster = new MonsterCard(2, "Dragon", 40, ElementType.Fire, MonsterType.Dragon);

            // Act
            double damage = waterSpell.CalculateDamage(fireMonster);

            // Assert
            Assert.That(damage, Is.EqualTo(waterSpell.Damage * 2));
        }

        [Test]
        public void MonsterVsMonster_PureFight_NormalDamage()
        {
            // Arrange
            var ork = new MonsterCard(1, "Ork", 40, ElementType.Normal, MonsterType.Ork);
            var troll = new MonsterCard(2, "Troll", 35, ElementType.Fire, MonsterType.Knight);

            // Act
            double damage = ork.CalculateDamage(troll);

            // Assert
            Assert.That(damage, Is.EqualTo(ork.Damage));
        }

        [Test]
        public void FireSpellVsWaterMonster_HalvedDamage()
        {
            // Arrange
            var fireSpell = new SpellCard(1, "FireSpell", 40, ElementType.Fire);
            var waterMonster = new MonsterCard(2, "WaterGoblin", 30, ElementType.Water, MonsterType.Goblin);

            // Act
            double damage = fireSpell.CalculateDamage(waterMonster);

            // Assert
            Assert.That(damage, Is.EqualTo(fireSpell.Damage * 0.5));
        }
    }
}