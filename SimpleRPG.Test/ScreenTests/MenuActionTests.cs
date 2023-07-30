using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class MenuActionTests
    {
        private static readonly BattleMove DoNothingMove = new DoNothingMove("sits around");
        private static readonly IFighter Fighter = FighterFactory.GetFighter(FighterType.Goblin, 1);
        private const string DisplayText = "Foo";

        [Test]
        public void ConvertToMenuSelection_ReturnsCorrectResult([Values("foo", "bar")] string displayText)
        {
            MenuAction basicMenuAction = new MenuAction(displayText, move: DoNothingMove, fighter: Fighter);

            MenuSelection returnedSelection = basicMenuAction.ConvertToMenuSelection();

            Assert.AreEqual(displayText, returnedSelection.Description);
            Assert.AreEqual(DoNothingMove, returnedSelection.Move);
            Assert.AreEqual(Fighter, returnedSelection.Target);
        }

        [Test]
        public void ConvertToMenuSelection_ReturnsCorrectResult_TypedMenuAction([Values(MagicType.Lightning, MagicType.Water, MagicType.None)] MagicType actionMagicType)
        {
            TypedMenuAction<MagicType> basicMenuAction = new TypedMenuAction<MagicType>(actionMagicType, DisplayText, move: DoNothingMove, fighter: Fighter);

            TypedMenuSelection<MagicType> returnedSelection = basicMenuAction.ConvertToMenuSelection() as TypedMenuSelection<MagicType>;

            Assert.NotNull(returnedSelection);

            Assert.AreEqual(DisplayText, returnedSelection.Description);
            Assert.AreEqual(DoNothingMove, returnedSelection.Move);
            Assert.AreEqual(Fighter, returnedSelection.Target);
            Assert.AreEqual(actionMagicType, returnedSelection.Item);
        }
    }
}
