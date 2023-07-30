using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.HelperTests
{
    [TestFixture]
    public class DecisionManagerTests
    {
        private DecisionManager _decisionManager;
        private GodRelationshipManager _relationshipManager;
        private MockMenuFactory _menuFactory;
        private MockOutput _output;
        private MockInput _input;
        private MockChanceService _chanceService;

        [SetUp]
        public void SetUp()
        {
            _output = new MockOutput();
            _input = new MockInput();
            _chanceService = new MockChanceService();

            _relationshipManager = new GodRelationshipManager();
            FighterFactory.SetGodRelationshipManager(_relationshipManager);
            _menuFactory = new MockMenuFactory();
            

            _decisionManager = new DecisionManager(_relationshipManager, null, _menuFactory, _input, _output);
        }

        [TearDown]
        public void TearDown()
        {
            _decisionManager = null;
            _relationshipManager = null;
        }

        #region .AssignNameBonuses() method

        [Test]
        public void AssignNameBonuses_CorrectlyAssignsBonuses()
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");
            
            int dantePreBonusSpeed = dante.Speed;
            int dantePreBonusFirePower = dante.MagicStrengthBonuses[MagicType.Fire];
            int dantePreBonusFireResistance = dante.MagicResistanceBonuses[MagicType.Fire];

            int arrokohPreBonusStrength = arrokoh.Strength;
            int arrokohPreBonusLightningPower = dante.MagicStrengthBonuses[MagicType.Lightning];
            int arrokohPreBonusLightningResistance = dante.MagicResistanceBonuses[MagicType.Lightning];

            _decisionManager.AssignNameBonuses(dante, arrokoh);
            
            int dantePostBonusSpeed = dante.Speed;
            int dantePostBonusFirePower = dante.MagicStrengthBonuses[MagicType.Fire];
            int dantePostBonusFireResistance = dante.MagicResistanceBonuses[MagicType.Fire];

            int danteSpeedBonus = dantePostBonusSpeed - dantePreBonusSpeed;
            Assert.Greater(danteSpeedBonus, 0);

            int danteFireStrengthBonus = dantePostBonusFirePower - dantePreBonusFirePower;
            Assert.Greater(danteFireStrengthBonus, 0);

            int danteFireResistanceBonus = dantePostBonusFireResistance - dantePreBonusFireResistance;
            Assert.Greater(danteFireResistanceBonus, 0);

            int arrokohPostBonusStrength = arrokoh.Strength;
            int arrokohPostBonusLightningPower = arrokoh.MagicStrengthBonuses[MagicType.Lightning];
            int arrokohPostBonusLightningResistance = arrokoh.MagicResistanceBonuses[MagicType.Lightning];

            int arrokohStrengthBonus = arrokohPostBonusStrength - arrokohPreBonusStrength;
            Assert.Greater(arrokohStrengthBonus, 0);

            int arrokohLightningStrengthBonus = arrokohPostBonusLightningPower - arrokohPreBonusLightningPower;
            Assert.Greater(arrokohLightningStrengthBonus, 0);

            int arrokohLightningResistanceBonus = arrokohPostBonusLightningResistance - arrokohPreBonusLightningResistance;
            Assert.Greater(arrokohLightningResistanceBonus, 0);
        }

        [Test]
        public void AssignNameBonuses_IgnoresCaseOfFighterName([Values("upper", "lower")] string nameCase)
        {
            string danteName = "Dante";
            string arrokohName = "Arrokoh";

            danteName = nameCase == "upper" ? danteName.ToUpperInvariant() : danteName.ToLowerInvariant();
            arrokohName = nameCase == "upper" ? arrokohName.ToUpperInvariant() : arrokohName.ToLowerInvariant();

            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, danteName);
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, arrokohName);

            int arrokohPreBonusStrength = arrokoh.Strength;
            int dantePreBonusSpeed = dante.Speed;

            _decisionManager.AssignNameBonuses(dante, arrokoh);

            int arrokohPostBonusStrength = arrokoh.Strength;
            int dantePostBonusSpeed = dante.Speed;

            int arrokohStrengthBonus = arrokohPostBonusStrength - arrokohPreBonusStrength;
            Assert.Greater(arrokohStrengthBonus, 0);

            int danteSpeedBonus = dantePostBonusSpeed - dantePreBonusSpeed;
            Assert.Greater(danteSpeedBonus, 0);
        }

        [Test]
        public void AssignNameBonuses_IgnoresOrderOfFighters()
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            HumanFighter dante2 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh2 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            int danteSpeedBefore = dante.Speed;
            int arrokohStrengthBefore = arrokoh.Strength;

            Assert.AreEqual(dante.Speed, dante2.Speed);
            Assert.AreEqual(arrokoh.Strength, arrokoh2.Strength);

            _decisionManager.AssignNameBonuses(dante, arrokoh);
            _decisionManager.AssignNameBonuses(arrokoh2, dante2);

            Assert.AreEqual(dante.Speed, dante2.Speed);
            Assert.AreEqual(arrokoh.Strength, arrokoh2.Strength);

            Assert.Greater(dante.Speed - danteSpeedBefore, 0);
            Assert.Greater(arrokoh.Strength - arrokohStrengthBefore, 0);
        }

        [Test]
        public void AssignNameBonuses_CorrectlySetsGodRelationship()
        {
            HumanFighter poopyCarrots = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "PoopyCarrots");
            HumanFighter chesterton = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Chesterton");

            _decisionManager.AssignNameBonuses(poopyCarrots, chesterton);

            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(poopyCarrots, GodEnum.MercyGod));
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(chesterton, GodEnum.MalevolentGod));
        }

        [Test]
        public void AssignNameBonuses_CorrectlySetsPersonalityFlag()
        {
            HumanFighter poopyCarrots = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "PoopyCarrots");
            HumanFighter chesterton = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Chesterton");

            _decisionManager.AssignNameBonuses(poopyCarrots, chesterton);

            Assert.Contains(PersonalityFlag.Heroic, poopyCarrots.PersonalityFlags);
            Assert.Contains(PersonalityFlag.MorallyFlexible, chesterton.PersonalityFlags);
        }

        #endregion .AssignNameBonuses() method

        #region personalityQuiz

        [Test]
        public void PersonalityQuiz_ManualEntry_CorrectlySetsInitializationValues([Values("1", "2")] string selectedFighterInput)
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            HumanFighter selectedFighter = selectedFighterInput == "1" ? dante : arrokoh;
            HumanFighter notSelectedFighter = selectedFighterInput == "1" ? arrokoh : dante;

            int luckBefore = selectedFighter.Luck;

            for (var i = 0; i < 8; ++i)
            {
                _input.Push(selectedFighterInput);
            }

            _decisionManager.PersonalityQuiz(dante, arrokoh);

            int luckAfter = selectedFighter.Luck;

            //who is more enigmatic?
            Assert.Contains(PersonalityFlag.Enigmatic, selectedFighter.PersonalityFlags, "first question should assign enigmatic flag");
            //who always wins at cards?
            Assert.AreEqual(10, luckAfter - luckBefore, "second question should raise luck");
            //who is more likely to seek treasure?
            Assert.Contains(PersonalityFlag.Adventurous, selectedFighter.PersonalityFlags, "third question should assign adventurous flag");
            //who sometimes watches the stars at night?
            Assert.Contains(PersonalityFlag.Dreamer, selectedFighter.PersonalityFlags, "fourth question should assign dreamer flag");
            //who is better at solving maze puzzles?
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(selectedFighter, GodEnum.IntellectGod), "fifth question should raise IntellectGod relationship");
            Assert.Contains(PersonalityFlag.MazeSolver, selectedFighter.PersonalityFlags, "fifth quesiton should assign mazeSolver flag");
            //who would succumb to an evil gem?
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(selectedFighter, GodEnum.MalevolentGod), "sixth question should raise malevolent god relationship for selected fighter");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(notSelectedFighter, GodEnum.MercyGod), "sixth question should raise mercy god relationship for not selected fighter");
            //who believes in ghosts?
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(notSelectedFighter, GodEnum.MachineGod), "seventh question should raise Machine God relationship for selected fighter");
            //who eats the last donut without asking?
            Assert.Contains(PersonalityFlag.SelfishDonutEater, selectedFighter.PersonalityFlags, "eigth question should assign selfishDonutEater flag");
        }

        [Test]
        public void PersonalityQuiz_ManualEntry_CorrectlyHandlesBothOption()
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");
            
            for (var i = 0; i < 8; ++i)
            {
                _input.Push(i == 5 ? "both" : "1");
            }

            _decisionManager.PersonalityQuiz(dante, arrokoh);

            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MalevolentGod), "sixth question should raise malevolent god relationship for both fighters for 'both' result");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MalevolentGod), "sixth question should raise malevolent god relationship for both fighters for 'both' result");
            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MercyGod), "sixth question should not raise mercy god relationship for either fighter for 'both' result");
            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MercyGod), "sixth question should not raise mercy god relationship for either fighter for 'both' result");
        }

        [Test]
        public void PersonalityQuiz_ManualEntry_CorrectlyHandlesNeitherOption()
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            for (var i = 0; i < 8; ++i)
            {
                _input.Push(i == 5 ? "neither" : "1");
            }

            _decisionManager.PersonalityQuiz(dante, arrokoh);

            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MalevolentGod), "sixth question should not raise malevolent god relationship for either fighters for 'neither' result");
            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MalevolentGod), "sixth question should not raise malevolent god relationship for either fighters for 'neither' result");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MercyGod), "sixth question should raise mercy god relationship for both fighters for 'neither' result");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MercyGod), "sixth question should raise mercy god relationship for both fighters for 'neither' result");
        }

        [Test]
        public void PersonalityQuiz_RandomizedEntries_CorrectlySetsInitializationValues([Values(0, 1)] int selectedFighterInput)
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            HumanFighter selectedFighter = selectedFighterInput == 0 ? dante : arrokoh;
            HumanFighter notSelectedFighter = selectedFighterInput == 0 ? arrokoh : dante;

            int luckBefore = selectedFighter.Luck;

            MockChanceService chanceService = new MockChanceService();
            for (var i = 0; i < 8; ++i)
            {
                chanceService.PushWhichEventsOccur(selectedFighterInput);
            }

            _decisionManager.PersonalityQuiz(dante, arrokoh, true, chanceService);

            int luckAfter = selectedFighter.Luck;

            //who is more enigmatic?
            Assert.Contains(PersonalityFlag.Enigmatic, selectedFighter.PersonalityFlags, "first question should assign enigmatic flag");
            //who always wins at cards?
            Assert.AreEqual(10, luckAfter - luckBefore, "second question should raise luck");
            //who is more likely to seek treasure?
            Assert.Contains(PersonalityFlag.Adventurous, selectedFighter.PersonalityFlags, "third question should assign adventurous flag");
            //who sometimes watches the stars at night?
            Assert.Contains(PersonalityFlag.Dreamer, selectedFighter.PersonalityFlags, "fourth question should assign dreamer flag");
            //who is better at solving maze puzzles?
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(selectedFighter, GodEnum.IntellectGod), "fifth question should raise IntellectGod relationship");
            Assert.Contains(PersonalityFlag.MazeSolver, selectedFighter.PersonalityFlags, "fifth quesiton should assign mazeSolver flag");
            //who would succumb to an evil gem?
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(selectedFighter, GodEnum.MalevolentGod), "sixth question should raise malevolent god relationship for selected fighter");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(notSelectedFighter, GodEnum.MercyGod), "sixth question should raise mercy god relationship for not selected fighter");
            //who believes in ghosts?
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(notSelectedFighter, GodEnum.MachineGod), "seventh question should raise Machine God relationship for selected fighter");
            //who eats the last donut without asking?
            Assert.Contains(PersonalityFlag.SelfishDonutEater, selectedFighter.PersonalityFlags, "eigth question should assign selfishDonutEater flag");
        }

        [Test]
        public void PersonalityQuiz_RandomizedEntries_CorrectlyHandlesBothOption()
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            MockChanceService chanceService = new MockChanceService();
            for (var i = 0; i < 8; ++i)
            {
                chanceService.PushWhichEventsOccur(i == 5 ? 2 : 0);
            }

            _decisionManager.PersonalityQuiz(dante, arrokoh, true, chanceService);

            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MalevolentGod), "sixth question should raise malevolent god relationship for both fighters for 'both' result");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MalevolentGod), "sixth question should raise malevolent god relationship for both fighters for 'both' result");
            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MercyGod), "sixth question should not raise mercy god relationship for either fighter for 'both' result");
            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MercyGod), "sixth question should not raise mercy god relationship for either fighter for 'both' result");
        }

        [Test]
        public void PersonalityQuiz_RandomizedEntries_CorrectlyHandlesNeitherOption()
        {
            HumanFighter dante = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            HumanFighter arrokoh = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");

            MockChanceService chanceService = new MockChanceService();
            for (var i = 0; i < 8; ++i)
            {
                chanceService.PushWhichEventsOccur(i == 5 ? 3 : 0);
            }

            _decisionManager.PersonalityQuiz(dante, arrokoh, true, chanceService);

            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MalevolentGod), "sixth question should not raise malevolent god relationship for either fighters for 'neither' result");
            Assert.AreEqual(0, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MalevolentGod), "sixth question should not raise malevolent god relationship for either fighters for 'neither' result");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(dante, GodEnum.MercyGod), "sixth question should raise mercy god relationship for both fighters for 'neither' result");
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(arrokoh, GodEnum.MercyGod), "sixth question should raise mercy god relationship for both fighters for 'neither' result");
        }

        #endregion

        #region .PickNextArea() method

        #region picking next Region

        private void PickNextArea_GroupingSetup_RegionalGroupings(out MapGrouping<Region, WorldRegion> grouping, bool castleUnlocked = false)
        {
            IRegionFactory regionFactory = new RegionFactory(_decisionManager);
            IEnumerable<Region> regions = regionFactory.GetRegions(EnumHelperMethods.GetAllValuesForEnum<WorldRegion>());
            MapManager mapManager = new MapManager(Globals.GroupingKeys);
            AreaMap<Region, WorldRegion> regionalMap = mapManager.GetRegionalMap(regions.ToArray());
            grouping = regionalMap.MapPaths.First(p => p.From.AreaId == WorldRegion.Fields).To;

            if (castleUnlocked)
            {
                grouping.Unlock(r => r.AreaId == WorldRegion.DarkCastle);
            }
        }

        private void PickNextArea_MenuSetup_RegionalGroupings(WorldRegion firstMenuSelection)
        {
            MockMenu menu;

            PickNextArea_MenuSetup_RegionalGroupings(firstMenuSelection, out menu);
        }

        private void PickNextArea_MenuSetup_RegionalGroupings(WorldRegion firstMenuSelection, out MockMenu menu)
        {
            menu = new MockMenu();
            menu.SetNextSelection(new TypedMenuSelection<WorldRegion>(firstMenuSelection, "", null, null));
            _menuFactory.SetMenu(MenuType.NonSpecificMenu, menu);
        }

        [Test]
        public void PickNextArea_RegionalGrouping_InitializesCorrectMenuActions([Values] bool castleUnlocked)
        {
            //Arrange
            MapGrouping<Region, WorldRegion> grouping;
            MockMenu menu;

            PickNextArea_MenuSetup_RegionalGroupings(WorldRegion.Casino, out menu);
            PickNextArea_GroupingSetup_RegionalGroupings(out grouping, castleUnlocked);

            //Act
            _decisionManager.PickNextArea(grouping, new TestTeam());

            //Assert
            List<WorldRegion> expectedRegions = new List<WorldRegion>
            {
                WorldRegion.CrystalCaves, WorldRegion.Casino, WorldRegion.Desert
            };

            if (castleUnlocked)
            {
                expectedRegions.Add(WorldRegion.DarkCastle);
            }

            List<MenuAction> menuActions = menu.MenuActions;

            Assert.True(menuActions.TrueForAll(ma => ma is TypedMenuAction<WorldRegion>));

            Assert.AreEqual(expectedRegions.Count, menuActions.Count);

            List<TypedMenuAction<WorldRegion>> typedMenuActions =
                menuActions.OfType<TypedMenuAction<WorldRegion>>().ToList();

            foreach (WorldRegion regionEnum in expectedRegions)
            {
                Assert.NotNull(typedMenuActions.FirstOrDefault(ma => ma.Item == regionEnum));
            }
        }

        #endregion

        #region picking next desert Sub Region

        /// <summary>
        /// Sets up the Region maps and groupings for the PickNextArea tests
        /// </summary>
        /// <param name="firstGrouping"></param>
        /// <param name="secondGrouping"></param>
        private void PickNextArea_GroupingSetup_DesertGroupings(out MapGrouping<SubRegion, WorldSubRegion> firstGrouping,
            out MapGrouping<SubRegion, WorldSubRegion> secondGrouping)
        {
            Region desertRegion;

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping, out desertRegion);
        }

        /// <summary>
        /// Sets up the Region maps and groupings for the PickNextArea tests
        /// </summary>
        /// <param name="firstGrouping"></param>
        /// <param name="secondGrouping"></param>
        /// <param name="desertRegion"></param>
        private void PickNextArea_GroupingSetup_DesertGroupings(out MapGrouping<SubRegion, WorldSubRegion> firstGrouping,
            out MapGrouping<SubRegion, WorldSubRegion> secondGrouping, out Region desertRegion)
        {
            IRegionFactory regionFactory = new RegionFactory(_decisionManager);
            desertRegion = regionFactory.GetRegion(WorldRegion.Desert);
            MapManager mapManager = new MapManager(Globals.GroupingKeys);
            AreaMap<SubRegion, WorldSubRegion> desertMap = mapManager.GetSubRegionalMap(WorldRegion.Desert, desertRegion.SubRegions);
            firstGrouping = desertMap.MapPaths.First(p => p.From.AreaId == WorldSubRegion.DesertIntro).To;
            secondGrouping = desertMap.MapPaths.First(p => p.From.AreaId == WorldSubRegion.DesertCrypt).To;
        }

        private void PickNextArea_MenuSetup_DesertGroupings(WorldSubRegion firstMenuSelection, WorldSubRegion secondMenuSelection)
        {
            MockMenu firstMenu, secondMenu;

            PickNextArea_MenuSetup_DesertGroupings(firstMenuSelection, secondMenuSelection, out firstMenu, out secondMenu);
        }

        private void PickNextArea_MenuSetup_DesertGroupings(WorldSubRegion firstMenuSelection, WorldSubRegion secondMenuSelection, 
            out MockMenu firstMenu, out MockMenu secondMenu)
        {
            firstMenu = new MockMenu();
            firstMenu.SetChanceService(_chanceService);
            firstMenu.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(firstMenuSelection, "", null, null));

            secondMenu = new MockMenu();
            secondMenu.SetChanceService(_chanceService);
            secondMenu.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(secondMenuSelection, "", null, null));

            _menuFactory.SetMenu(MenuType.NonSpecificMenu, firstMenu);
            _menuFactory.SetMenu(MenuType.NonSpecificMenu, secondMenu);

            _chanceService.SetShuffleIndices(new [] { 0, 1, 2 });
            _chanceService.SetShuffleIndices(new [] { 0, 1, 2 });
        }

        private Team PickNextArea_TeamSetup_DesertGroupings(int numFighterForMazeFlag = 1, string name1 = null, string name2 = null)
        {
            TestHumanFighter fighter1 = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, name1);
            TestHumanFighter fighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, name2);

            switch (numFighterForMazeFlag)
            {
                case 1:
                    fighter1.AddPersonalityFlag(PersonalityFlag.MazeSolver);
                    break;
                case 2:
                    fighter2.AddPersonalityFlag(PersonalityFlag.MazeSolver);
                    break;
            }

            return new TestTeam(fighter1, fighter2);
        }

        [Test]
        public void PickNextAreaMethod_CorrectlySelectsSecondSubRegion_DesertGroupings([Range(0, 3)] int selectedSubRegionIndex)
        {
            //arrange
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            List<WorldSubRegion> firstGroupingSubRegions =
                WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.FirstDesertGroupingId).ToList();
            WorldSubRegion selectedSubRegionEnum = firstGroupingSubRegions[selectedSubRegionIndex];

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(selectedSubRegionEnum, WorldSubRegion.BeastTemple);
            Team team = PickNextArea_TeamSetup_DesertGroupings();

            //all sub regions must be unlocked for the Decision Manager to be called
            Assert.True(firstGrouping.Values.TrueForAll(grouping => !grouping.IsLocked));

            MapGroupingItem<SubRegion, WorldSubRegion> selectedRegionGroupingItem = firstGrouping.Values[selectedSubRegionIndex];

            //Act
            SubRegion nextRegion = _decisionManager.PickNextArea(firstGrouping, team);

            //Assert
            Assert.AreEqual(selectedRegionGroupingItem.Item, nextRegion);

            List<MapGroupingItem<SubRegion, WorldSubRegion>> notSelectedGroupingItems =
                firstGrouping.Values.Where(groupingItem => groupingItem.Item.AreaId != selectedSubRegionEnum).ToList();

            Assert.False(selectedRegionGroupingItem.IsLocked);
            Assert.True(notSelectedGroupingItems.TrueForAll(grouping => grouping.IsLocked));
        }

        [Test]
        public void PickNextAreaMethod_CorrectlySelectsThirdSubRegion_DesertGroupings([Range(0, 3)] int selectedSubRegionIndex)
        {
            //arrange
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;

            List<WorldSubRegion> secondGroupingSubRegions =
                WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.SecondDesertGroupingId).ToList();
            WorldSubRegion selectedSubRegionEnum = secondGroupingSubRegions[selectedSubRegionIndex];

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(WorldSubRegion.DesertCrypt, selectedSubRegionEnum);
            Team team = PickNextArea_TeamSetup_DesertGroupings();

            //all sub regions must be unlocked for the Decision Manager to be called
            Assert.True(secondGrouping.Values.TrueForAll(grouping => !grouping.IsLocked));

            MapGroupingItem<SubRegion, WorldSubRegion> selectedRegionGroupingItem = secondGrouping.Values[selectedSubRegionIndex];
            
            //Act
            _decisionManager.PickNextArea(firstGrouping, team);

            //Assert
            List<MapGroupingItem<SubRegion, WorldSubRegion>> notSelectedGroupingItems =
                secondGrouping.Values.Where(groupingItem => groupingItem.Item.AreaId != selectedSubRegionEnum).ToList();

            Assert.False(selectedRegionGroupingItem.IsLocked);
            Assert.True(notSelectedGroupingItems.TrueForAll(grouping => grouping.IsLocked));
        }

        [Test]
        public void PickNextAreaMethod_SecretOptionsAreHidden_DesertGroupings()
        {
            //Arrange
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            MockMenu firstMenu, secondMenu;

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(WorldSubRegion.DesertCrypt, WorldSubRegion.TempleOfDarkness, out firstMenu, out secondMenu);
            Team team = PickNextArea_TeamSetup_DesertGroupings();

            //Act
            _decisionManager.PickNextArea(firstGrouping, team);

            List<TypedMenuAction<WorldSubRegion>> firstMenuActions = firstMenu.MenuActions.OfType<TypedMenuAction<WorldSubRegion>>().ToList();

            TypedMenuAction<WorldSubRegion> firstSecretOption =
                firstMenuActions.Single(ma => ma.Item == WorldSubRegion.Oasis);

            Assert.True(firstSecretOption.IsHidden);

            List<TypedMenuAction<WorldSubRegion>> secondMenuActions = secondMenu.MenuActions.OfType<TypedMenuAction<WorldSubRegion>>().ToList();

            TypedMenuAction<WorldSubRegion> secondSecretOption =
                secondMenuActions.Single(ma => ma.Item == WorldSubRegion.BeastTemple);

            Assert.True(secondSecretOption.IsHidden);
        }

        [Test]
        public void PickNextAreaMethod_FirstMenuKeysOffOfMazeSolverFlag_DesertGroupings([Values(1, 2)] int whichPlayerGetsFlag)
        {
            //arrange
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            MockMenu menu1, menu2;
            string name1 = "Tony";
            string name2 = "Chris";
            
            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(WorldSubRegion.DesertCrypt, WorldSubRegion.CliffsOfAThousandPushups, out menu1, out menu2);
            Team team = PickNextArea_TeamSetup_DesertGroupings(whichPlayerGetsFlag, name1, name2);

            //Act
            _decisionManager.PickNextArea(firstGrouping, team);

            //Assert
            string expectedName1, expectedName2;

            if (whichPlayerGetsFlag == 1)
            {
                expectedName1 = name1;
                expectedName2 = name2;
            }
            else
            {
                expectedName1 = name2;
                expectedName2 = name1;
            }

            Assert.AreEqual(expectedName1, menu1.Owner.DisplayName);
            Assert.AreEqual(expectedName2, menu2.Owner.DisplayName);
        }

        [Test]
        public void PickNextAreaMethod_CorrectMenuPromptsDisplayed_DesertGroupings([Values(1, 2)] int whichPlayerGetsFlag)
        {
            //arrange
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            MockMenu menu1, menu2;
            string name1 = "Jeff";
            string name2 = "Daniel";

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(WorldSubRegion.DesertCrypt, WorldSubRegion.CliffsOfAThousandPushups, out menu1, out menu2);
            Team team = PickNextArea_TeamSetup_DesertGroupings(whichPlayerGetsFlag, name1, name2);

            //Act
            _decisionManager.PickNextArea(firstGrouping, team);

            //Assert
            string expectedName1, expectedName2;

            if (whichPlayerGetsFlag == 1)
            {
                expectedName1 = name1;
                expectedName2 = name2;
            }
            else
            {
                expectedName1 = name2;
                expectedName2 = name1;
            }

            MockOutputMessage[] outputs = _output.GetOutputs();

            MockOutputMessage firstPrompt = outputs[0];
            Assert.True(firstPrompt.Message.StartsWith(expectedName1 + ":"));
            
            MockOutputMessage secondPrompt = outputs[4]; //first menu prompt, 3 visible options
            Assert.True(secondPrompt.Message.StartsWith(expectedName2 + ":"));
        }


        [Test, Pairwise]
        public void PickNextAreaMethod_CorrectGodRelationshipBonusesAssigned_DesertGroupings(
            [Values(1, 2)] int whichPlayerGetsFlag, [Range(1,4)] int firstMenuSelection, [Range(1,4)] int secondMenuSelection)
        {
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            MockMenu menu1, menu2;

            List<WorldSubRegion> firstGroupingSubRegions =
                WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.FirstDesertGroupingId).ToList();
            List<WorldSubRegion> secondGroupingSubRegions =
                WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.SecondDesertGroupingId).ToList();

            WorldSubRegion firstSubRegion = firstGroupingSubRegions[firstMenuSelection - 1];
            WorldSubRegion secondSubRegion = secondGroupingSubRegions[secondMenuSelection - 1];

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(firstSubRegion, secondSubRegion, out menu1, out menu2);
            Team team = PickNextArea_TeamSetup_DesertGroupings(whichPlayerGetsFlag, "Stan", "Bill");

            List<HumanFighter> humanFighters = team.Fighters.OfType<HumanFighter>().ToList();

            HumanFighter mazeSolverFighter = humanFighters.First(f => f.PersonalityFlags.Contains(PersonalityFlag.MazeSolver));
            HumanFighter notMazeSolverFighter = humanFighters.First(f => !f.PersonalityFlags.Contains(PersonalityFlag.MazeSolver));

            menu1.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(firstSubRegion, "", null, null));
            menu2.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(secondSubRegion, "", null, null));

            //Act
            _decisionManager.PickNextArea(firstGrouping, team);

            List<GodEnum> allGodEnums = EnumHelperMethods.GetAllValuesForEnum<GodEnum>().ToList();

            GodEnum mazeSolverSelectedRelationship = WorldSubRegions.GetGodEnumBySubRegion(firstSubRegion);
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(mazeSolverFighter, mazeSolverSelectedRelationship));

            IEnumerable<GodEnum> notSelectedGods = allGodEnums.Where(g => g != mazeSolverSelectedRelationship);
            foreach (GodEnum notSelectedGod in notSelectedGods)
            {
                int relationshipValue = _relationshipManager.GetFighterRelationshipValue(mazeSolverFighter, notSelectedGod);
                Assert.AreEqual(0, relationshipValue, $"fighter {mazeSolverFighter.DisplayName} should not have any points assigned to {notSelectedGod}");
            }


            GodEnum notMazeSolverSelectedRelationship = WorldSubRegions.GetGodEnumBySubRegion(secondSubRegion);
            Assert.AreEqual(1, _relationshipManager.GetFighterRelationshipValue(notMazeSolverFighter, notMazeSolverSelectedRelationship));

            notSelectedGods = allGodEnums.Where(g => g != notMazeSolverSelectedRelationship);
            foreach (GodEnum notSelectedGod in notSelectedGods)
            {
                int relationshipValue = _relationshipManager.GetFighterRelationshipValue(notMazeSolverFighter, notSelectedGod);
                Assert.AreEqual(0, relationshipValue, $"fighter {notMazeSolverFighter.DisplayName} should not have any points assigned to {notSelectedGod}");
            }
        }

        [Test]
        public void PickNextAreaMethod_CorrectlyReturnsEarlierResult([Range(0, 1)] int selectedSubRegionIndex)
        {
            //arrange
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            MockMenu menu1, menu2;

            List<WorldSubRegion> firstGroupingSubRegions =
                WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.FirstDesertGroupingId).ToList();
            WorldSubRegion selectedSubRegionEnum = firstGroupingSubRegions[selectedSubRegionIndex];

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping);
            PickNextArea_MenuSetup_DesertGroupings(selectedSubRegionEnum, WorldSubRegion.VillageCenter, out menu1, out menu2);
            Team team = PickNextArea_TeamSetup_DesertGroupings();

            //all sub regions must be unlocked for the Decision Manager to be called
            Assert.True(secondGrouping.Values.TrueForAll(grouping => !grouping.IsLocked));

            MapGroupingItem<SubRegion, WorldSubRegion> selectedRegionGroupingItem = secondGrouping.Values[selectedSubRegionIndex];
            _decisionManager.PickNextArea(firstGrouping, team);

            //will set the menus to return a different selection, and so if the returned region matches the original selection, the test has passed
            WorldSubRegion secondMenuSelection = firstGroupingSubRegions[selectedSubRegionIndex + 1];
            menu1.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(secondMenuSelection, "", null, null));
            menu2.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(WorldSubRegion.BeastTemple, "", null, null));

            //Act
            SubRegion secondReturnedRegion = _decisionManager.PickNextArea(firstGrouping, team);

            //Assert
            Assert.AreEqual(selectedSubRegionEnum, secondReturnedRegion.AreaId);
        }

        //TODO: need tests that the text of the menu actions are correct

        #endregion

        #endregion

        #region .ResetDecisionsAfterRegionCleared() method

        [Test]
        public void CorrectlyClearsEarlierGroupingResult_WhenDesertRegionCompleted([Range(0, 1)] int selectedSubRegionIndex)
        {
            //arrange
            Region desertRegion;
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping, secondGrouping;
            MockMenu menu1, menu2;

            List<WorldSubRegion> firstGroupingSubRegions =
                WorldSubRegions.GetSubRegionsByGroupingId(Globals.GroupingKeys.FirstDesertGroupingId).ToList();
            WorldSubRegion selectedSubRegionEnum = firstGroupingSubRegions[selectedSubRegionIndex];

            PickNextArea_GroupingSetup_DesertGroupings(out firstGrouping, out secondGrouping, out desertRegion);
            PickNextArea_MenuSetup_DesertGroupings(selectedSubRegionEnum, WorldSubRegion.VillageCenter, out menu1, out menu2);
            Team team = PickNextArea_TeamSetup_DesertGroupings();

            //all sub regions must be unlocked for the Decision Manager to be called
            Assert.True(secondGrouping.Values.TrueForAll(grouping => !grouping.IsLocked));
            
            _decisionManager.PickNextArea(firstGrouping, team);
            
            desertRegion.OnRegionCompleted(new RegionCompletedEventArgs(desertRegion));

            //will set the menus to return a different selection, and so if the returned region matches the original selection, the test has failed
            WorldSubRegion secondMenuSelection = firstGroupingSubRegions[selectedSubRegionIndex + 1];
            menu1.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(secondMenuSelection, "", null, null));
            menu2.SetNextSelection(new TypedMenuSelection<WorldSubRegion>(WorldSubRegion.BeastTemple, "", null, null));

            _menuFactory.SetMenu(MenuType.NonSpecificMenu, menu1);
            _menuFactory.SetMenu(MenuType.NonSpecificMenu, menu2);

            firstGrouping.Unlock(s => true);
            secondGrouping.Unlock(s => true);

            //Act
            SubRegion secondReturnedRegion = _decisionManager.PickNextArea(firstGrouping, team);

            //Assert
            Assert.AreEqual(secondMenuSelection, secondReturnedRegion.AreaId);
        }

        #endregion
    }
}
