using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Regions
{
    /// <summary>
    /// Keeps track of each individual region and the individual properties therein. 
    /// Has the user unlocked the secret region, are they going to fight the boss in a region, or do they have more fighting to do?
    /// </summary>
    public class RegionManager
    {
        private readonly AreaMap<Region, WorldRegion> _regionalMap;

        private readonly IMapManager _mapManager;

        private readonly ITeamFactory _teamFactory;

        private readonly IMenuFactory _menuFactory;

        private readonly IDecisionManager _decisionManager;

        private readonly BattlefieldFactory _battlefieldFactory;

        private readonly IInput _input;

        private readonly IOutput _output;

        private readonly IChanceService _chanceService;

        public RegionManager(
            IRegionFactory regionFactory,
            IMapManager mapManager, 
            ITeamFactory teamFactory,
            IMenuFactory menuFactory,
            IDecisionManager decisionManager,
            BattlefieldFactory battlefieldFactory,
            IInput input,
            IOutput output,
            IChanceService chanceService)
        {
            _mapManager = mapManager;
            _teamFactory = teamFactory;
            _decisionManager = decisionManager;
            _menuFactory = menuFactory;
            _battlefieldFactory = battlefieldFactory;
            
            _input = input;
            _output = output;
            _chanceService = chanceService;

            IEnumerable<WorldRegion> allRegionEnums = EnumHelperMethods.GetAllValuesForEnum<WorldRegion>();
            IEnumerable<Region> allRegions = regionFactory.GetRegions(allRegionEnums);
            _regionalMap = mapManager.GetRegionalMap(allRegions.ToArray());
        }

        public void Battle(BattleManager manager, Team playerTeam)
        {
            bool continuer = true;

            while (continuer)
            {
                Region region = _regionalMap.CurrentArea;

                ColorString introString = new ColorString($"You have entered the {region.AreaId} region");

                if (!string.IsNullOrWhiteSpace(region.RegionIntro?.GetFullString()))
                {
                    introString = region.RegionIntro;
                }

                _output.WriteLine(introString);

                BattleEndStatus battleEndStatus = BattleThroughRegion(region, manager, playerTeam);

                if (battleEndStatus != BattleEndStatus.Victory)
                {
                    continuer = false;
                }
                else
                {
                    region = _regionalMap.Advance(_decisionManager, playerTeam);

                    if (region == null)
                    {
                        continuer = false;
                    }
                }
            }
        }

        private BattleEndStatus BattleThroughSubRegion(SubRegion subRegion, BattleManager manager, Team playerTeam)
        {
            bool continuer = true;
            BattleEndStatus battleEndStatus = BattleEndStatus.None;

            int numberRegularBattles = subRegion.NumberRegularBattles;
            for (var i = 0; i <= numberRegularBattles && continuer; ++i)
            {
                //var enemyTeam = GetEnemies(subRegion, i);
                BattleFieldInfo battlefiendInfo = GetBattleFieldInfo(subRegion, i);

                BattleManagerBattleConfiguration config = GetBattleConfiguration(subRegion, i);

                if (i == numberRegularBattles)
                {
                    PrintBossIntro(subRegion.AreaId);
                }

                battleEndStatus = manager.Battle(playerTeam, battlefiendInfo.EnemyTeam, battlefiendInfo.TerrainInteractables.ToList(), config);

                if (battleEndStatus != BattleEndStatus.Victory)
                {
                    continuer = false;
                }
                else
                {
                    foreach (IFighter player in playerTeam.Fighters)
                    {
                        if (!player.IsAlive())
                        {
                            player.Revive(1);
                        }
                        player.Heal(player.MaxHealth);
                        player.RestoreMana(player.MaxMana);
                    }

                    _output.WriteLine("HP and Mana fully restored!");
                    _input.WaitAndClear(_output);
                }
            }

            return battleEndStatus;
        }

        private BattleEndStatus BattleThroughRegion(Region region, BattleManager manager, Team playerTeam)
        {
            bool continuer = true;
            BattleEndStatus battleEndStatus = BattleEndStatus.None;

            foreach (HumanFighter fighter in playerTeam.Fighters.OfType<HumanFighter>())
            {
                foreach (BattleMove regionMove in region.MovesUnlockedUponEnteringRegion)
                {
                    fighter.AddMove(regionMove);
                }
            }

            AreaMap<SubRegion, WorldSubRegion> subRegionalMap = _mapManager.GetSubRegionalMap(region.AreaId, region.SubRegions);

            while (continuer)
            {
                SubRegion subRegion = subRegionalMap.CurrentArea;
                subRegion.Fooed += PleaseDeleteMeIAmTerribleCode;
                _playerTeam = playerTeam;

                ColorString introString = new ColorString($"You have entered the {subRegion.AreaId} sub region");

                if (!string.IsNullOrWhiteSpace(subRegion.RegionIntro?.GetFullString()))
                {
                    introString = subRegion.RegionIntro;
                }

                _output.WriteLine(introString);

                battleEndStatus = BattleThroughSubRegion(subRegion, manager, playerTeam);

                if (battleEndStatus != BattleEndStatus.Victory)
                {
                    continuer = false;
                    subRegion.Fooed += PleaseDeleteMeIAmTerribleCode;
                }
                else
                {
                    List<HumanFighter> fighters = playerTeam.GetHumanFighters().ToList();
                    subRegion.ExecuteCutscene(_input, _output, fighters[0], fighters[1]);

                    subRegion.Fooed += PleaseDeleteMeIAmTerribleCode;

                    subRegion = subRegionalMap.Advance(_decisionManager, playerTeam);

                    if (subRegion == null)
                    {
                        continuer = false;
                    }
                }
            }

            return battleEndStatus;
        }

        private Team _playerTeam;
        public void PleaseDeleteMeIAmTerribleCode(object sender, FooEventArgs e)
        {
            MapGrouping<SubRegion, WorldSubRegion> grouping = _mapManager.GetGrouping<SubRegion, WorldSubRegion>(e.GroupingId);
            _decisionManager.PickNextArea(grouping, _playerTeam);
        }

        public BattleFieldInfo GetBattleFieldInfo(SubRegion region, int round)
        {
            BattleFieldInfo generatedBattleFieldInfo = null;

            if (round < region.NumberRegularBattles)
            {
                BattlefieldConfiguration battlefieldConfig =
                    region.ScriptedBattlefieldConfigurations.FirstOrDefault(script => script.BattleIndex == round)?
                        .BattlefieldConfig;

                if (battlefieldConfig == null)
                {
                    Team generatedTeam = _teamFactory.GetTeam(region);
                    generatedBattleFieldInfo = new BattleFieldInfo(generatedTeam, new List<TerrainInteractable>());
                }
                else
                {
                    generatedBattleFieldInfo = _battlefieldFactory.GetBattleFieldSetUp(battlefieldConfig);
                }
            }
            else
            {
                generatedBattleFieldInfo = _battlefieldFactory.GetBattleFieldSetUp(region.BossConfiguration);
            }

            return generatedBattleFieldInfo;
        }

        /*public Team GetEnemies(SubRegion region, int round)
        {
            Team ret = round < region.NumberRegularBattles ? 
                 : 
                

            return ret;
        }*/

        public BattleManagerBattleConfiguration GetBattleConfiguration(SubRegion subRegion, int round)
        {
            if (subRegion.AreaId == WorldSubRegion.DesertIntro && round == subRegion.NumberRegularBattles)
            {
                return new BattleManagerBattleConfiguration
                {
                    SpecialBattleFlag = BattleConfigurationSpecialFlag.FirstBarbarianBattle
                };
            }
            else
            {
                return new BattleManagerBattleConfiguration();
            }
        }

        /// <summary>
        /// first round, 2/3rds chance of fighting 2 enemies, 1/3 chance fighting 3.
        /// second round, 3/4th to fight 3, 1/4 to fight 4
        /// third round, 1/6 chance to fight 3, 5/6 to fight 4
        /// </summary>
        /// <param name="round"></param>
        /// <returns></returns>
        public int GetNumOpponents(int round)
        {
            var odds = new double[2];
            var rets = new int[2];

            switch (round)
            {
                default:
                    odds[0] = 2.0 / 3.0;
                    rets[0] = 2;
                    rets[1] = 3;
                    break;
                case 2:
                    odds[0] = 3.0 / 4.0;
                    rets[0] = 3;
                    rets[1] = 4;
                    break;
                case 3:
                    odds[0] = 1.0 / 6.0;
                    rets[0] = 3;
                    rets[1] = 4;
                    break;
            }

            odds[1] = 1.0 - odds[0];

            var outcome = _chanceService.WhichEventOccurs(odds);

            var ret = rets[outcome];

            return ret;
        }

        /// <summary>
        /// Each region has its own unique boss. This returns the appropriate boss for a region.
        /// If, later, certain bosses appear with side kicks, those will also be generated
        /// </summary>
        /// <param name="subRegion"></param>
        /// <returns></returns>
        public Team GetBossTeamForRegion(SubRegion subRegion)
        {
            List<IFighter> bossFighters = new List<IFighter>();

            switch (subRegion.AreaId)
            {
                case WorldSubRegion.Fields:
                    MegaChicken chicken = (MegaChicken)FighterFactory.GetFighter(FighterType.MegaChicken, 1);
                    bossFighters.Add(chicken);
                    break;
                case WorldSubRegion.DesertIntro:
                    Barbarian barbarian = (Barbarian)FighterFactory.GetFighter(FighterType.Barbarian, 1);
                    bossFighters.Add(barbarian);
                    break;
                default:
                    throw new NotImplementedException($"Main.GetBossTeamForRegion() is not yet built to handle sub region value {subRegion.AreaId}");
            }

            Team ret = new Team(new MenuManager(_input, _output, _menuFactory), bossFighters);

            return ret;
        }

        private void PrintBossIntro(WorldSubRegion subRegion)
        {
            switch (subRegion)
            {
                case WorldSubRegion.Fields:
                    _output.WriteLine("The land shudders in apprehension...");
                    _output.WriteLine("The sky is ripped asunder...");
                    _output.WriteLine("From an otherworldly portal appears the first boss monster...");
                    _output.Write("It's time to face the ");
                    _output.WriteError("Mega Chicken");
                    _output.WriteLine(", Master of the poultry magical arts.");
                    _input.WaitAndClear(_output);
                    break;
                case WorldSubRegion.DesertIntro:
                    _output.WriteLine("As the harsh sun looks upon you");
                    _output.WriteLine("You see a lone figure standing atop a dune");
                    _output.WriteLine("He looks ready for a fight, like it's been a long time since he's been hugged.");//" flanked by his flunkies");
                    _output.Write("It's time to face the ");
                    _output.WriteError("Barbarian");
                    _output.WriteLine(", Strongest of the desert");
                    _input.WaitAndClear(_output);
                    break;
            }
        }
    }
}
