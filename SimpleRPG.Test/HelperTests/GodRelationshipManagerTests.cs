using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;

namespace SimpleRPG.Test.HelperTests
{
    [TestFixture]
    public class GodRelationshipManagerTests
    {
        private GodRelationshipManager _relationshipManager;

        private HumanFighter _fighter;

        private IEnumerable<GodEnum> _allGodValues;

        [SetUp]
        public void SetUp()
        {
            _relationshipManager = new GodRelationshipManager();
            _allGodValues = EnumHelperMethods.GetAllValuesForEnum<GodEnum>();

            FighterFactory.SetGodRelationshipManager(_relationshipManager);
            _fighter = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
        }

        [TearDown]
        public void TearDown()
        {
            _relationshipManager = null;
            _fighter = null;
        }

        [Test]
        public void InitializeForBattle_CorrectlyInitializes()
        {
            _fighter = new HumanFighter("Dante", 1);
            _relationshipManager.InitializeForFighter(_fighter);

            foreach (GodEnum god in _allGodValues)
            {
                int relationshipValue = _relationshipManager.GetFighterRelationshipValue(_fighter, god);
                Assert.AreEqual(0, relationshipValue);
            }
        }

        [Test]
        public void FighterFactory_AutoInitializesHumanFighters_GodRelationshipManagerSet()
        {
            FighterFactory.SetGodRelationshipManager(_relationshipManager);
            _fighter = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);

            Assert.DoesNotThrow(() => _relationshipManager.GetFighterRelationshipValue(_fighter, GodEnum.MachineGod));
        }

        [Test]
        public void UpdateRelationshipMethod_CorrectlyUpdatesRelationship([Values(1, 3)] int incrementValue, [Values(GodEnum.IntellectGod, GodEnum.MalevolentGod)] GodEnum god)
        {
            _relationshipManager.UpdateRelationship(_fighter, god, incrementValue);

            int updatedValue = _relationshipManager.GetFighterRelationshipValue(_fighter, god);

            Assert.AreEqual(incrementValue, updatedValue);
        }
    }
}
