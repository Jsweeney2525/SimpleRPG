using NUnit.Framework;
using SimpleRPG.Battle;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class LevelUpManagerTests
    {
        //TODO: missing other tests

        [Test]
        public void TestGetDefenseByLevelMethod()
        {
            int[] expected = { 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (var i = 0; i < 11; ++i)
            {
                Assert.AreEqual(expected[i], LevelUpManager.GetDefenseByLevel(i));
            }
        }

        [Test]
        public void TestDefenseBoostByLevelMethod_PerLevel()
        {
            int[] boosts = { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1};

            for (var i = 1; i < 11; ++i)
            {
                Assert.AreEqual(boosts[i - 1], LevelUpManager.DefenseBoostByLevel(i));
            }
        }

        [Test]
        public void TestDefenseBoostByLevelMethod_BySum()
        {
            var total = LevelUpManager.DefenseBoostByLevel(1);

            for (var i = 1; i < 11; ++i)
            {
                total += LevelUpManager.DefenseBoostByLevel(i);
                var foo = LevelUpManager.GetDefenseByLevel(i);
                Assert.AreEqual(total, foo);
            }
        }

        [Test]
        public void TestGetSpeedByLevelMethod()
        {
            int[] expected = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            for (var i = 0; i < 11; ++i)
            {
                Assert.AreEqual(expected[i], LevelUpManager.GetSpeedByLevel(i));
            }
        }

        [Test]
        public void TestSpeedBoostByLevelMethod_PerLevel()
        {
            var boosts = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            for (var i = 1; i < 11; ++i)
            {
                Assert.AreEqual(boosts[i - 1], LevelUpManager.SpeedBoostByLevel(i));
            }
        }

        [Test]
        public void TestSpeedBoostByLevelMethod_BySum()
        {
            int total = 0;
            for (var i = 1; i < 11; ++i)
            {
                total += LevelUpManager.SpeedBoostByLevel(i);
                var totalByLevel = LevelUpManager.GetSpeedByLevel(i);
                Assert.AreEqual(total, totalByLevel);
            }
        }
    }
}
