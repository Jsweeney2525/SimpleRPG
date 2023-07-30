using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Events;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class EventHandlerPrinterTests
    {
        private HumanFighter _fighter;

        private MockOutput _output;
        private EventHandlerPrinter _printer;

        [SetUp]
        public void SetUp()
        {
            _fighter = (HumanFighter) FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);

            _output = new MockOutput();
            _printer = new EventHandlerPrinter(_output);

            _printer.Subscribe(_fighter);
        }

        [TearDown]
        public void TearDown()
        {
            _fighter = null;
            _output = null;
            _printer = null;
        }

        [Test]
        public void CorrectlyPrintsMessage_StatBonusAdded([Values]StatType statType, [Values(1, 3)] int bonusAmount)
        {
            StatBonusAppliedEventArgs e = new StatBonusAppliedEventArgs(statType, bonusAmount, false);
            _fighter.OnStatBonusApplied(e);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            MockOutputMessage output = outputs[0];

            Assert.AreEqual($"{_fighter.DisplayName} gained +{bonusAmount} {statType.ToString().ToLower()}\n", output.Message);
        }

        [Test]
        public void CorrectlySuppressesMessage_StatBonusAdded_SecretStatBonus()
        {
            StatBonusAppliedEventArgs e = new StatBonusAppliedEventArgs(StatType.Defense, 2, true);
            _fighter.OnStatBonusApplied(e);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void CorrectlyPrintsMessage_MagicBonusAdded([Values(MagicType.Fire, MagicType.Ice, MagicType.Water)]MagicType magicType, 
            [Values(1, 3)] int bonusAmount,
            [Values]MagicStatType magicStatType)
        {
            MagicBonusAppliedEventArgs e = new MagicBonusAppliedEventArgs(magicStatType, magicType, bonusAmount, false);
            _fighter.OnMagicBonusApplied(e);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            MockOutputMessage output = outputs[0];

            Assert.AreEqual($"{_fighter.DisplayName} gained +{bonusAmount} {magicType.ToString().ToLower()} magic {magicStatType.ToString().ToLower()}\n", output.Message);
        }

        [Test]
        public void CorrectlySuppressesMessage_MagicBonusAdded_SecretStatBonus()
        {
            MagicBonusAppliedEventArgs e = new MagicBonusAppliedEventArgs(MagicStatType.Power, MagicType.Lightning, 4, true);
            _fighter.OnMagicBonusApplied(e);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void CorrectlySubscribesToMultipleFighters()
        {
            _output = new MockOutput();
            EventHandlerPrinter printer = new EventHandlerPrinter(_output);

            HumanFighter fighter2 = (HumanFighter) FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
            HumanFighter fighter3 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);

            printer.Subscribe(_fighter, fighter2, fighter3);

            StatBonusAppliedEventArgs e = new StatBonusAppliedEventArgs(StatType.Defense, 2, false);
            _fighter.OnStatBonusApplied(e);
            fighter2.OnStatBonusApplied(e);
            fighter3.OnStatBonusApplied(e);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(3, outputs.Length);
        }
    }
}
