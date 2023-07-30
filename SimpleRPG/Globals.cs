using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG
{
    public class Globals
    {
        public static ConsoleColor DefaultColor { get; } = ConsoleColor.Gray;

        public static ConsoleColor ErrorColor { get; } = ConsoleColor.Red;

        public static ConsoleColor DisabledColor { get; } = ConsoleColor.DarkGray;

        public static ConsoleOutput Output { get; } = new ConsoleOutput();

        public static ConsoleInput Input { get; } = new ConsoleInput();

        public static ChanceService ChanceService { get; } = new ChanceService();

        public static GodRelationshipManager GodRelationshipManager { get; } = new GodRelationshipManager();

        public static MenuFactory MenuFactory { get; } = new MenuFactory();

        public static DecisionManager DecisionManager { get; } = new DecisionManager(GodRelationshipManager, ChanceService, MenuFactory, Input, Output);

        public static GroupingKeys GroupingKeys { get; } = new GroupingKeys();

        private static GroupingFactory GroupingFactory { get; } = new GroupingFactory(ChanceService, new TeamFactory(ChanceService, MenuFactory), new FighterFactory());

        public static BattlefieldFactory BattlefieldFactory  { get; } = new BattlefieldFactory(new TeamFactory(ChanceService, MenuFactory), GroupingFactory, MenuFactory, ChanceService);

        public static RegionManager RegionManager { get; } = new RegionManager(
            new RegionFactory(DecisionManager), 
            new MapManager(GroupingKeys), 
            new TeamFactory(ChanceService, MenuFactory),
            MenuFactory,
            DecisionManager, 
            BattlefieldFactory,
            Input, Output, ChanceService);

        //TODO: all error message strings should be grouped inside a class
        public const string GenericErrorMessage = "The given input was not recognized. Please try again...";

        public static MagicType[] EggMagicTypes { get; } = { MagicType.Fire, MagicType.Ice, MagicType.Lightning };

        //TODO: all replace text strings should be grouped inside a class
        public const string TargetReplaceText = "[target]";

        public const string OwnerReplaceText = "[owner]";

        public const string MaxValueReplacementString = "[max]";

        public const string MinValueReplacementString = "[min]";
    }

    public class GroupingKeys
    {
        public int MainRegionalMapGroupingId { get; }

        public int FirstDesertGroupingId { get; }

        public int SecondDesertGroupingId { get; }

        public int ColiseumDesertGroupingId { get; }

        public GroupingKeys()
        {
            int keyValue = 0;

            MainRegionalMapGroupingId = keyValue++;
            FirstDesertGroupingId = keyValue++;
            SecondDesertGroupingId = keyValue++;
            ColiseumDesertGroupingId = keyValue;
        }

        public IEnumerable<int> GetGroupingKeysForRegion(WorldRegion region)
        {
            List<int> ret = new List<int>();
            switch (region)
            {
                case WorldRegion.Desert:
                    ret.Add(FirstDesertGroupingId);
                    ret.Add(SecondDesertGroupingId);
                    ret.Add(ColiseumDesertGroupingId);
                    break;
            }

            return ret;
        }
    }

    public static class EnumHelperMethods
    {
        public static IEnumerable<T> GetAllValuesForEnum<T>()
        {
            Type enumType = typeof(T);

            return enumType.GetEnumValues().OfType<T>();
        }
    }
}
