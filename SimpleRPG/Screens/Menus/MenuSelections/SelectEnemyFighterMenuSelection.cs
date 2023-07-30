using SimpleRPG.Enums;

namespace SimpleRPG.Screens.Menus.MenuSelections
{
    /// <summary>
    /// A class to represent something that can be selected in a menu. The current use cases are:
    /// SubMenu is null- this represents a "leaf" in a menu tree. Fighter should be non-null
    /// </summary>
    public class SelectEnemyFighterMenuSelection : MenuSelection
    {
        public FighterType FighterType { get; }

        public int FighterLevel { get; }

        public BattleConfigurationSpecialFlag SpecialFlag { get; }

        public SelectEnemyFighterMenuSelection(FighterType fighterType, int fighterLevel, BattleConfigurationSpecialFlag flag)
            :base(string.Empty, null, null)
        {
            FighterType = fighterType;
            FighterLevel = fighterLevel;
            SpecialFlag = flag;
        }
    }
}
