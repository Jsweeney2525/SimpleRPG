using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.RegionTests
{
    [TestFixture]
    public class RegionManagerTests
    {
        private TestTeamFactory _teamFactory;
        private MockRegionFactory _regionFactory;
        private MockMapManager _mapManager;
        private MockDecisionManager _decisionManager;
        private RegionManager _regionManager;
        private GroupingFactory _groupingFactory;

        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);
        private readonly BattleMove _doNothingMove = MoveFactory.Get(BattleMoveType.DoNothing);
        private Team _oneEnemyTeam;

        private MockChanceService _chanceService;
        private MockOutput _output;
        private MockInput _input;
        private BattleManagerBattleConfiguration _config;
        private TestBattleManager _battleManager;

        private TestHumanFighter _humanFighter1, _humanFighter2;
        private Team _humanTeam;

        [SetUp]
        public void SetUp()
        {
            _chanceService = new MockChanceService();
            _output = new MockOutput();
            _input = new MockInput();
            _battleManager = new TestBattleManager(_chanceService, _input, _output);
            _config = new SilentBattleConfiguration();
            _battleManager.SetConfig(_config);

            _teamFactory = new TestTeamFactory(_chanceService);
            _mapManager = new MockMapManager();
            _decisionManager = new MockDecisionManager(new GodRelationshipManager());
            _regionFactory = new MockRegionFactory(_decisionManager);
            _groupingFactory = new GroupingFactory(_chanceService, _teamFactory, new FighterFactory());

            _humanFighter1 = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanFighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam = new Team(TestMenuManager.GetTestMenuManager(), _humanFighter1, _humanFighter2);

            _oneEnemyTeam = GetSingleEnemyTeam();
        }

        [TearDown]
        public void TearDown()
        {
            _chanceService = null;
            _output = null;
            _input = null;
            _battleManager = null;
            _config = null;

            _regionFactory = null;
            _regionManager = null;
            _groupingFactory = null;

            _teamFactory = null;
            _mapManager = null;

            _humanFighter1 = null;
            _humanFighter2 = null;
            _humanTeam = null;
        }

        private RegionManager GetRegionManager()
        {
            return new RegionManager(_regionFactory, 
                _mapManager, 
                _teamFactory, 
                new MenuFactory(), _decisionManager, new BattlefieldFactory(_teamFactory, _groupingFactory, new MenuFactory(), _chanceService),  _input, _output, _chanceService);
        }

        private Team GetSingleEnemyTeam()
        {
            TestEnemyFighter enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            enemy.SetMove(_doNothingMove);
            return new TestTeam(enemy);
        }

        [Test]
        public void CorrectlySetsBattleMoves_UponEnteringRegion()
        {
            BattleMove superAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 80, 20, 5);
            BattleMove fastAttack = new AttackBattleMove("bar", TargetType.SingleEnemy, 100, 0, 0, 2);

            Assert.False(_humanFighter1.AllSpecialMoves.Contains(superAttack));
            Assert.False(_humanFighter1.AllSpecialMoves.Contains(fastAttack));

            TeamConfiguration bossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1));
            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, 1, new [] {new ChanceEvent<int>(1, 1), }, new[] {FighterType.Egg}, new BattlefieldConfiguration(bossConfiguration, null));
            SubRegion[] subRegions = {subRegion};

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new[] {superAttack, fastAttack}, subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            _teamFactory.PushTeams(_oneEnemyTeam);
            _regionManager = GetRegionManager();

            _humanFighter1.SetMove(_runawayMove);
            _humanFighter1.SetMoveTarget(_humanFighter1);

            _humanFighter2.SetMove(_doNothingMove);
            
            _regionManager.Battle(_battleManager, _humanTeam);

            Assert.True(_humanFighter1.AllSpecialMoves.Contains(superAttack));
            Assert.True(_humanFighter1.AllSpecialMoves.Contains(fastAttack));
        }

        [Test, Sequential]
        public void CorrectlyPrintsIntro_UponEnteringRegion(
            [Values("Welcome to the Jungle!", "flee, for you have entered the graveyard!")] string regionIntroMessage,
            [Values("Welcome to a smaller subset of the jungle", "Spooky ghosts watch your every step")] string subRegionIntroMessage)
        {
            TeamConfiguration bossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1));
            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, 1, new [] {new ChanceEvent<int>(1, 1) }, new[] {FighterType.Egg}, new BattlefieldConfiguration(bossConfiguration, null),
                regionIntro: subRegionIntroMessage);
            SubRegion[] subRegions = {subRegion};

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions, regionIntroMessage);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            _teamFactory.PushTeams(_oneEnemyTeam);
            _regionManager = GetRegionManager();

            _humanFighter1.SetMove(_runawayMove);
            _humanFighter1.SetMoveTarget(_humanFighter1);

            _humanFighter2.SetMove(_doNothingMove);
            
            _regionManager.Battle(_battleManager, _humanTeam);

            MockOutputMessage[] outputs = _output.GetOutputs();

            MockOutputMessage output = outputs[0];

            Assert.AreEqual(regionIntroMessage + "\n", output.Message);

            output = outputs[1];

            Assert.AreEqual(subRegionIntroMessage + "\n", output.Message);
        }

        [Test]
        public void PrintsDefaultIntroString_UponEnteringRegion_NullOrEmptyIntroSuppliedToSubRegion([Values("", null)] string introMessage)
        {
            TeamConfiguration bossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1));
            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, 1, new[] { new ChanceEvent<int>(1, 1) }, new[] {FighterType.Egg}, new BattlefieldConfiguration(bossConfiguration),
                regionIntro: introMessage);
            SubRegion[] subRegions = {subRegion};

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            _teamFactory.PushTeams(_oneEnemyTeam);
            _regionManager = GetRegionManager();

            _humanFighter1.SetMove(_runawayMove);
            _humanFighter1.SetMoveTarget(_humanFighter1);

            _humanFighter2.SetMove(_doNothingMove);
            
            _regionManager.Battle(_battleManager, _humanTeam);

            MockOutputMessage[] outputs = _output.GetOutputs();

            MockOutputMessage output = outputs[0];
            
            Assert.AreEqual($"You have entered the {WorldRegion.Fields} region\n", output.Message);

            output = outputs[1];

            Assert.AreEqual($"You have entered the {WorldSubRegion.Fields} sub region\n", output.Message);
        }

        [Test]
        public void CorrectNumberOfBattlesBeforeBoss([Range(1, 3)] int numberBattles)
        {
            TeamConfiguration bossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Barbarian, 5), new EnemyConfiguration(FighterType.ShieldGuy, 2), new EnemyConfiguration(FighterType.ShieldGuy, 3));
            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, numberBattles, new[] { new ChanceEvent<int>(1, 1) }, new[] {FighterType.Egg},
                new BattlefieldConfiguration(bossConfiguration));
            SubRegion[] subRegions = {subRegion};

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            for (int i = 0; i < numberBattles; ++i)
            {
                Team team = GetSingleEnemyTeam();
                _teamFactory.PushTeams(team);
                _chanceService.PushWhichEventOccurs(0);
            }

            _regionManager = GetRegionManager();

            IFighter target = TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            target.PhysicalDamage(target.MaxHealth);

            _humanFighter1.SetMove(_basicAttackMove, numberBattles);
            _humanFighter1.SetMoveTarget(target);
            _chanceService.PushAttackHitsNotCrit(numberBattles);
            _humanFighter1.SetMove(_runawayMove);

            _humanFighter2.SetMove(_doNothingMove);

            _chanceService.PushWhichEventsOccur(0, 0, 0, 0, 0, 0); //used for when the bosses are selecting their moves

            _regionManager.Battle(_battleManager, _humanTeam);

            List<Team> enemyTeams = _battleManager.GetAllEnemyTeams();

            Assert.AreEqual(numberBattles + 1, enemyTeams.Count);

            Team bossTeam = enemyTeams[numberBattles];

            Assert.AreEqual(3, bossTeam.Fighters.Count);

            Assert.True(bossTeam.Fighters.Exists(f => f is Barbarian && f.Level == 5));
            Assert.True(bossTeam.Fighters.Exists(f => f is ShieldGuy && f.Level == 2));
            Assert.True(bossTeam.Fighters.Exists(f => f is ShieldGuy && f.Level == 3));
        }

        [Test]
        public void CorrectlyGeneratesEnemyTeams_FromRandomizedConfiguration([Values(new [] {FighterType.Goblin, FighterType.Fairy}, new [] { FighterType.Warrior, FighterType.ShieldGuy })] IEnumerable<FighterType> enemyTypes )
        {
            List<FighterType> enemyTypesList = enemyTypes.ToList();

            TeamConfiguration bossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Barbarian, 5), new EnemyConfiguration(FighterType.ShieldGuy, 2), new EnemyConfiguration(FighterType.ShieldGuy, 3));
            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, 1, new[] { new ChanceEvent<int>(2, 1) }, enemyTypesList, new BattlefieldConfiguration(bossConfiguration));
            SubRegion[] subRegions = { subRegion };

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            _regionManager = GetRegionManager();

            _humanFighter1.SetMove(_runawayMove);
            _humanFighter1.SetMoveTarget(_humanFighter1);

            _humanFighter2.SetMove(_doNothingMove);
            
            _chanceService.PushWhichEventsOccur(0, 0, 1); //first roll determines there will be 2 enemies, then the next 2 specify 1 of each type 
            _regionManager.Battle(_battleManager, _humanTeam);

            Team enemyTeam = _battleManager.GetAllEnemyTeams()[0];

            Assert.IsTrue(enemyTypesList[0].IsCorrectType(enemyTeam.Fighters[0]));
            Assert.IsTrue(enemyTypesList[1].IsCorrectType(enemyTeam.Fighters[1]));
        }

        [Test]
        public void CorrectlyGeneratesEnemyTeams_FromBattlefieldConfiguration([Values(new[] { FighterType.Goblin, FighterType.Fairy }, new[] { FighterType.Warrior, FighterType.ShieldGuy })] IEnumerable<FighterType> enemyTypes)
        {
            List<FighterType> enemyTypesList = enemyTypes.ToList();

            TeamConfiguration bossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Barbarian, 5), new EnemyConfiguration(FighterType.ShieldGuy, 2), new EnemyConfiguration(FighterType.ShieldGuy, 3));

            ShadeGroupingConfiguration shadeGroupingConfig = new ShadeGroupingConfiguration(3, 1);
            BellType[] bellTypes = { BellType.Silver, BellType.Copper };
            BellTerrainConfiguration bellConfig = new BellTerrainConfiguration(bellTypes);
            BattlefieldConfiguration battleConfig = new BattlefieldConfiguration(shadeGroupingConfig, bellConfig);

            List<ScriptedBattlefieldConfiguration> scriptedBattlefieldConfigurations = new List
                <ScriptedBattlefieldConfiguration>
                {
                    new ScriptedBattlefieldConfiguration(battleConfig, 0)
                };

            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, 
                1, 
                new[] { new ChanceEvent<int>(2, 1) }, 
                enemyTypesList,
                new BattlefieldConfiguration(bossConfiguration),
                scriptedBattlefieldConfigurations);
            SubRegion[] subRegions = { subRegion };

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            _regionManager = GetRegionManager();

            _humanFighter1.SetMove(_runawayMove);
            _humanFighter1.SetMoveTarget(_humanFighter1);

            _humanFighter2.SetMove(_doNothingMove);

            _regionManager.Battle(_battleManager, _humanTeam);

            Team enemyTeam = _battleManager.GetAllEnemyTeams()[0];
            List<IFighter> enemyFighters = enemyTeam.Fighters;

            Assert.AreEqual(3, enemyFighters.Count);
            Assert.True(enemyFighters.TrueForAll(ef => ef is Shade));

            List<TerrainInteractable> terrainInteractables = _battleManager.GetAllTerrainInteractableLists()[0];

            Assert.NotNull(terrainInteractables);
            Assert.AreEqual(2, terrainInteractables.Count);

            List<Bell> bells = terrainInteractables.OfType<Bell>().ToList();

            Assert.AreEqual(bellTypes.Length, bells.Count);
            for (var i = 0; i < bellTypes.Length; ++i)
            {
                Assert.AreEqual(bellTypes[i], bells[i].BellType);
            }
        }

        [Test]
        public void CorrectlyMovesToNextSubRegion_OnlyOneNextSubRegion()
        {
            const MagicType firstBossEggType = MagicType.Fire;
            const MagicType secondBossEggType = MagicType.Ice;

            TeamConfiguration firstBossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, firstBossEggType));
            SubRegion subRegionA = new SubRegion(WorldSubRegion.Fields, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(firstBossConfiguration));

            const string secondRegionIntro = "Who wants donuts?!?";
            TeamConfiguration secondBossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, secondBossEggType));
            SubRegion subRegionB = new SubRegion(WorldSubRegion.DesertCrypt, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(secondBossConfiguration), regionIntro: secondRegionIntro);

            SubRegion[] subRegions = { subRegionA, subRegionB };
            
            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            AreaMap<Region, WorldRegion> regionMap = new AreaMap<Region, WorldRegion>(fakeFieldsRegion, new MapPath<Region, WorldRegion>(fakeFieldsRegion));

            AreaMap<SubRegion, WorldSubRegion> subRegionMap = new AreaMap<SubRegion, WorldSubRegion>(subRegionA, new MapPath<SubRegion, WorldSubRegion>(subRegionA, subRegionB));

            _mapManager.SetRegionalMap(regionMap);
            _mapManager.SetSubRegionalMap(WorldRegion.Fields, subRegionMap);

            _regionManager = GetRegionManager();
            
            TestEnemyFighter target = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            target.SetHealth(1, 0);
            
            _humanFighter1.SetMove(_basicAttackMove, 1);
            _humanFighter1.SetMove(_runawayMove, 1);
            _humanFighter1.SetMoveTarget(target);

            _humanFighter2.SetMove(_doNothingMove);

            _chanceService.PushAttackHitsNotCrit();
            
            _regionManager.Battle(_battleManager, _humanTeam);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.NotNull(outputs.FirstOrDefault(o => o.Message == secondRegionIntro + "\n"));

            List<Team> enemyTeams = _battleManager.GetAllEnemyTeams();

            Assert.AreEqual(2, enemyTeams.Count);

            Egg secondBoss = enemyTeams[1].Fighters[0] as Egg;

            Assert.NotNull(secondBoss);
            Assert.AreEqual(secondBossEggType, secondBoss.MagicType);

            Assert.AreEqual(WorldSubRegion.DesertCrypt, subRegionMap.CurrentArea.AreaId);
        }

        [Test]
        public void CorrectlyMovesToNextSubRegion_TwoNextSubRegionsButOneIsLocked()
        {
            const MagicType firstBossEggType = MagicType.Fire;
            const MagicType secondBossEggType = MagicType.Ice;

            TeamConfiguration firstBossConfiguration =
                new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, firstBossEggType));
            SubRegion subRegionA = new SubRegion(WorldSubRegion.Fields, 0, new ChanceEvent<int>[0], new FighterType[0],
                new BattlefieldConfiguration(firstBossConfiguration));

            const string secondRegionIntro = "Who wants donuts?!?";
            TeamConfiguration secondBossConfiguration =
                new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, secondBossEggType));
            SubRegion subRegionB = new SubRegion(WorldSubRegion.DesertCrypt, 0, new ChanceEvent<int>[0],
                new FighterType[0], new BattlefieldConfiguration(secondBossConfiguration), regionIntro: secondRegionIntro);

            const string lockedRegionIntro = "watch me do a flip!";
            TeamConfiguration lockedRegionConfiguration =
                new TeamConfiguration(new EnemyConfiguration(FighterType.Goblin, 1));
            SubRegion lockedSubRegion = new SubRegion(WorldSubRegion.AncientLibrary, 0, new ChanceEvent<int>[0],
                new FighterType[0], new BattlefieldConfiguration(lockedRegionConfiguration), regionIntro: lockedRegionIntro);

            SubRegion[] subRegions = {subRegionA, subRegionB, lockedSubRegion};

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            AreaMap<Region, WorldRegion> regionMap = new AreaMap<Region, WorldRegion>(fakeFieldsRegion, new MapPath<Region, WorldRegion>(fakeFieldsRegion));

            MapGrouping<SubRegion, WorldSubRegion> grouping = new MapGrouping<SubRegion, WorldSubRegion>(0, subRegionB, lockedSubRegion);
            grouping.Lock(sr => sr.AreaId == WorldSubRegion.AncientLibrary);

            AreaMap<SubRegion, WorldSubRegion> subRegionMap = new AreaMap<SubRegion, WorldSubRegion>(subRegionA, new MapPath<SubRegion, WorldSubRegion>(subRegionA, grouping));

            _mapManager.SetRegionalMap(regionMap);
            _mapManager.SetSubRegionalMap(WorldRegion.Fields, subRegionMap);

            _regionManager = GetRegionManager();

            TestEnemyFighter target = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            target.SetHealth(1, 0);

            _humanFighter1.SetMove(_basicAttackMove, 1);
            _humanFighter1.SetMove(_runawayMove, 1);
            _humanFighter1.SetMoveTarget(target);

            _humanFighter2.SetMove(_doNothingMove);

            _chanceService.PushAttackHitsNotCrit();

            _regionManager.Battle(_battleManager, _humanTeam);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.Null(outputs.FirstOrDefault(o => o.Message == lockedRegionIntro + "\n"));

            List<Team> enemyTeams = _battleManager.GetAllEnemyTeams();

            Assert.AreEqual(2, enemyTeams.Count);

            Goblin lockedBoss = enemyTeams[1].Fighters[0] as Goblin;

            Assert.Null(lockedBoss);
        }

        [Test]
        public void CorrectlyMovesToNextSubRegion_MultipleNextSubRegions([Values(1, 2)] int selectedArea)
        {
            const int groupingId = 10;
            _decisionManager.SetGroupingChoice(selectedArea - 1);
            TeamConfiguration firstBossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, MagicType.Fire));
            SubRegion firstSubRegion = new SubRegion(WorldSubRegion.Fields, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(firstBossConfiguration));

            FighterType bossA = FighterType.Barbarian;
            WorldSubRegion regionA = WorldSubRegion.DesertCrypt;
            TeamConfiguration secondBossConfiguration = new TeamConfiguration(new EnemyConfiguration(bossA, 1));
            SubRegion subRegionA = new SubRegion(regionA, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(secondBossConfiguration));

            FighterType bossB = FighterType.MegaChicken;
            WorldSubRegion regionB = WorldSubRegion.Oasis;
            TeamConfiguration thirdBossConfiguration = new TeamConfiguration(new EnemyConfiguration(bossB, 1));
            SubRegion subRegionB = new SubRegion(regionB, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(thirdBossConfiguration));

            SubRegion[] subRegions = { firstSubRegion, subRegionA, subRegionB };

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            AreaMap<Region, WorldRegion> regionMap = new AreaMap<Region, WorldRegion>(fakeFieldsRegion, new MapPath<Region, WorldRegion>(fakeFieldsRegion));

            MapGrouping<SubRegion, WorldSubRegion> grouping = new MapGrouping<SubRegion, WorldSubRegion>(groupingId, subRegionA, subRegionB);

            AreaMap<SubRegion, WorldSubRegion> subRegionMap = new AreaMap<SubRegion, WorldSubRegion>(firstSubRegion, new MapPath<SubRegion, WorldSubRegion>(firstSubRegion, grouping));

            _mapManager.SetRegionalMap(regionMap);
            _mapManager.SetSubRegionalMap(WorldRegion.Fields, subRegionMap);

            _regionManager = GetRegionManager();

            TestEnemyFighter target = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            target.SetHealth(1, 0);

            _humanFighter1.SetMove(_basicAttackMove, 1);
            _humanFighter1.SetMove(_runawayMove, 1);
            _humanFighter1.SetMoveTarget(target);

            _humanFighter2.SetMove(_doNothingMove);
            
            _chanceService.PushAttackHitsNotCrit();

            _regionManager.Battle(_battleManager, _humanTeam);

            List<Team> enemyTeams = _battleManager.GetAllEnemyTeams();

            Assert.AreEqual(2, enemyTeams.Count);

            IFighter secondBoss = enemyTeams[1].Fighters[0];

            FighterType selectedBossType = selectedArea == 1 ? bossA : bossB;
            WorldSubRegion selectedBossRegion = selectedArea == 1 ? regionA : regionB;

            Assert.NotNull(secondBoss);
            Assert.True(selectedBossType.IsCorrectType(secondBoss));
            Assert.AreEqual(selectedBossRegion, subRegionMap.CurrentArea.AreaId);
        }

        [Test]
        public void CorrectlyExecutesCutscene_SubRegionCompleted()
        {
            const MagicType firstBossEggType = MagicType.Fire;
            const MagicType secondBossEggType = MagicType.Ice;

            ColorString[] firstSceneLines = { new ColorString("foo"), new ColorString("bar")  };
            SingleScene firstSingleScene = new SingleScene(firstSceneLines);
            ColorString[] secondSceneLines = { new ColorString("baz"), new ColorString("fwee") };
            SingleScene secondSingleScene = new SingleScene(secondSceneLines);

            SingleScene[] scenes = {firstSingleScene, secondSingleScene};
            Cutscene regionACutscene = new Cutscene(scenes);

            TeamConfiguration firstBossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, firstBossEggType));
            SubRegion subRegionA = new SubRegion(WorldSubRegion.Fields, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(firstBossConfiguration), regionCompletedCutscene: regionACutscene);

            TeamConfiguration secondBossConfiguration = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, secondBossEggType));
            SubRegion subRegionB = new SubRegion(WorldSubRegion.DesertCrypt, 0, new ChanceEvent<int>[0], new FighterType[0], new BattlefieldConfiguration(secondBossConfiguration));

            SubRegion[] subRegions = { subRegionA, subRegionB };

            Region fakeFieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], subRegions);
            _regionFactory.SetRegion(WorldRegion.Fields, fakeFieldsRegion);

            AreaMap<Region, WorldRegion> regionMap = new AreaMap<Region, WorldRegion>(fakeFieldsRegion, new MapPath<Region, WorldRegion>(fakeFieldsRegion));

            AreaMap<SubRegion, WorldSubRegion> subRegionMap = new AreaMap<SubRegion, WorldSubRegion>(subRegionA, new MapPath<SubRegion, WorldSubRegion>(subRegionA, subRegionB));

            _mapManager.SetRegionalMap(regionMap);
            _mapManager.SetSubRegionalMap(WorldRegion.Fields, subRegionMap);

            _regionManager = GetRegionManager();

            TestEnemyFighter target = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            target.SetHealth(1, 0);

            _humanFighter1.SetMove(_basicAttackMove, 1);
            _humanFighter1.SetMove(_runawayMove, 1);
            _humanFighter1.SetMoveTarget(target);

            _humanFighter2.SetMove(_doNothingMove);
            
            _chanceService.PushAttackHitsNotCrit();

            _regionManager.Battle(_battleManager, _humanTeam);

            List<MockOutputMessage> outputs = _output.GetOutputs().ToList();

            List<ColorString> allCutsceneLines = firstSceneLines.Concat(secondSceneLines).ToList();

            foreach (ColorString cutsceneLine in allCutsceneLines)
            {
                Assert.NotNull(outputs.FirstOrDefault(output => output.Message == cutsceneLine.Value + "\n" && output.Color == cutsceneLine.Color));
            }
        }
    }
}
