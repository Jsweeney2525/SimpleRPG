using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Regions
{
    public class RegionFactory : IRegionFactory
    {
        protected IDecisionManager DecisionManager;

        public RegionFactory(IDecisionManager decisionManager)
        {
            DecisionManager = decisionManager;
        }

        public Region GetRegion(WorldRegion region)
        {
            IEnumerable<BattleMove> movesForRegion = GetRegionMovesForRegion(region);
            IEnumerable<SubRegion> subRegionsForRegion = GetSubRegionsForRegion(region);

            Region ret = new Region(region, movesForRegion, subRegionsForRegion);

            ret.RegionCompleted += DecisionManager.ResetDecisionsAfterRegionCleared;

            return ret;
        }

        public IEnumerable<Region> GetRegions(IEnumerable<WorldRegion> regions)
        {
            return regions.Select(GetRegion);
        }

        private static IEnumerable<BattleMove> GetRegionMovesForRegion(WorldRegion region)
        {
            BattleMove[] ret;

            switch (region)
            {
                case WorldRegion.Desert:
                    ret = new[] { MoveFactory.Get(BattleMoveType.ShieldBuster), MoveFactory.Get(BattleMoveType.Attack, "feint") };
                    break;
                default:
                    ret = new BattleMove[0];
                    break;
            }

            return ret;
        }

        private static IEnumerable<SubRegion> GetSubRegionsForRegion(WorldRegion region)
        {
            IEnumerable<SubRegion> subRegions = WorldSubRegions.GetSubRegionsForRegions(region).Select(GetSubRegion);

            return subRegions;
        }

        private static SubRegion GetSubRegion(WorldSubRegion subRegion)
        {
            //SubRegion ret = new SubRegion(subRegion,
            //    4, GetNumberEnemyFighterChancesForRegion(subRegion),
            //    GetFighterTypesForRegion(subRegion), 
            //    GetBossConfigurationForBossRegion(subRegion),
            //    GetRegionIntro(subRegion),
            //    GetCutsceneForRegion(subRegion));

            SubRegion ret = new SubRegion(subRegion,
                GetNumberEnemiesBeforeBoss(subRegion), 
                GetNumberEnemyFighterChancesForRegion(subRegion),
                GetFighterTypesForRegion(subRegion), 
                GetBossConfigurationForBossRegion(subRegion),
                GetScriptedBattlefieldConfigurationsForRegion(subRegion),
                GetRegionIntro(subRegion),
                GetCutsceneForRegion(subRegion));

            return ret;
        }

        private static int GetNumberEnemiesBeforeBoss(WorldSubRegion subRegion)
        {
            int ret = 0;

            switch (subRegion)
            {
                case WorldSubRegion.DesertCrypt:
                    ret = 1;
                    break;
            }

            return ret;
        }

        private static IEnumerable<ChanceEvent<int>> GetNumberEnemyFighterChancesForRegion(WorldSubRegion subRegion)
        {
            ChanceEvent<int>[] ret;

            switch (subRegion)
            {
                default:
                    ret = new [] { new ChanceEvent<int>(2, 25), new ChanceEvent<int>(3, 65), new ChanceEvent<int>(4, 10) };
                    break;
            }

            return ret;
        }

        private static IEnumerable<ChanceEvent<FighterType>> GetFighterTypesForRegion(WorldSubRegion subRegion)
        {
            ChanceEvent<FighterType>[] ret;

            switch (subRegion)
            {
                case WorldSubRegion.Fields:
                    ret = new[] {
                        new ChanceEvent<FighterType>(FighterType.Fairy, .20),
                        new ChanceEvent<FighterType>(FighterType.Goblin, .30),
                        new ChanceEvent<FighterType>(FighterType.Ogre, .25),
                        new ChanceEvent<FighterType>(FighterType.Golem, .25)
                    };
                    break;
                case WorldSubRegion.DesertIntro:
                    ret = new[]
                    {
                        new ChanceEvent<FighterType>(FighterType.ShieldGuy, .5),
                        new ChanceEvent<FighterType>(FighterType.Warrior, .5)
                    };
                    break;
                //TODO: needs to be filled out
                case WorldSubRegion.DesertCrypt:
                case WorldSubRegion.TavernOfHeroes:
                case WorldSubRegion.AncientLibrary:
                case WorldSubRegion.Oasis:
                case WorldSubRegion.CliffsOfAThousandPushups:
                case WorldSubRegion.TempleOfDarkness:
                case WorldSubRegion.VillageCenter:
                case WorldSubRegion.BeastTemple:
                case WorldSubRegion.Coliseum:
                case WorldSubRegion.CasinoIntro:
                case WorldSubRegion.CavesIntro:
                case WorldSubRegion.DarkCastleIntro:
                    ret = new[]
                    {
                        new ChanceEvent<FighterType>(FighterType.Egg, 1)
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subRegion), subRegion, null);
            }

            return ret;
        }

        private static BattlefieldConfiguration GetBossConfigurationForBossRegion(WorldSubRegion subRegion)
        {
            TeamConfiguration teamInfo;
            TerrainInteractablesConfiguration fieldInfo = null;

            switch (subRegion)
            {
                case WorldSubRegion.Fields:
                    //teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.MegaChicken, 1));
                    teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, MagicType.Fire), new EnemyConfiguration(FighterType.Egg, 1, MagicType.Ice));
                    break;
                case WorldSubRegion.DesertIntro:
                    //teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.Barbarian, 1));
                    teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, MagicType.Fire), new EnemyConfiguration(FighterType.Egg, 1, MagicType.Ice));
                    break;
                //TODO: needs to be filled out
                case WorldSubRegion.DesertCrypt:
                case WorldSubRegion.TavernOfHeroes:
                case WorldSubRegion.AncientLibrary:
                case WorldSubRegion.Oasis:
                case WorldSubRegion.CliffsOfAThousandPushups:
                case WorldSubRegion.TempleOfDarkness:
                case WorldSubRegion.VillageCenter:
                case WorldSubRegion.BeastTemple:
                case WorldSubRegion.Coliseum:
                case WorldSubRegion.CasinoIntro:
                case WorldSubRegion.CavesIntro:
                case WorldSubRegion.DarkCastleIntro:
                    teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.MegaChicken, 1), new EnemyConfiguration(FighterType.ShieldGuy, 1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subRegion), subRegion, null);
            }

            var ret = new BattlefieldConfiguration(teamInfo, fieldInfo);

            return ret;
        }

        private static List<ScriptedBattlefieldConfiguration> GetScriptedBattlefieldConfigurationsForRegion(WorldSubRegion subRegion)
        {
            List<ScriptedBattlefieldConfiguration> generatedScriptedBattlefieldConfigurations = new List<ScriptedBattlefieldConfiguration>();

            switch (subRegion)
            {
                case WorldSubRegion.DesertCrypt:
                    BattlefieldConfiguration firstBattleConfig = new BattlefieldConfiguration(new ShadeGroupingConfiguration(3, 1), new BellTerrainConfiguration(BellType.Copper, BellType.Silver));
                    ScriptedBattlefieldConfiguration firstBattleScript = new ScriptedBattlefieldConfiguration(firstBattleConfig, 0);
                    generatedScriptedBattlefieldConfigurations.Add(firstBattleScript);
                    break;
            }

            return generatedScriptedBattlefieldConfigurations;
        }

        private static ColorString GetRegionIntro(WorldSubRegion subRegion)
        {
            return null;
        }

        private static Cutscene GetCutsceneForRegion(WorldSubRegion subRegion)
        {
            Cutscene ret;

            List<ColorString[]> sceneLines = new List<ColorString[]>();

            switch (subRegion)
            {
                case WorldSubRegion.Fields:
                    sceneLines.Add(new []
                    {
                        new ColorString("As you savor the sweet taste of victory, an old man approaches."),
                        new ColorString("He has the air of wisdom about him,"),
                        new ColorString("like he's the sort of man who has seen more than the average person,"),
                        new ColorString("Or perhaps he was an adventurer in his youth.")
                    });

                    sceneLines.Add(new[]
                    {
                        new ColorString("Old man: Heh, looks like you gave that nasty chicken the ole what-for."),
                        new ColorString("Good for you! I remember a time before so many monsters plagued the world."),
                        new ColorString("Could it be that you two wanted to make the world a better place,"),
                        new ColorString("and thought fighting monsters was more exciting than renovating the community center?"),
                        new ColorString("Well, I have a story to tell you! And don't worry, it won't be long or overly complicated,"),
                        new ColorString("Some of us have Bingo tonight and I've been on quite the lucky streak these past 50 moons")
                    });

                    sceneLines.Add(new[]
                    {
                        new ColorString("Old man: A long time ago, the gods watched over us, and the land was propserous."),
                        new ColorString("We didn't have a monster problem, and colors were brighter, I tell you."),
                        new ColorString("Then one day the gods went away. Some thought they were just out buying lotto tickets,"),
                        new ColorString("but I think there's an explanation-"),
                        new ColorString("We used to have ", new ColorString("Heroes ", ConsoleColor.Cyan), "back in the day, you know, people that served the will of the gods,"),
                        new ColorString("mortals who were the physical instruments of divine will."),
                        new ColorString("When their strength faded, and no new heroes arose, it weakened the power of the gods over their domain.")
                    });

                    sceneLines.Add(new[]
                    {
                        new ColorString(new ColorString("Old man: When I look at you two, I see the "), new ColorString("seeds of the future", ConsoleColor.Cyan), ","),
                        new ColorString("Yup, two aspiring heroes who may, with just the right encouragement, be able to revive the old gods"),
                        new ColorString("So that's your task, then. Go out and fight the great evils that plague us,"),
                        new ColorString("become the instruments of justice,"),
                        new ColorString("and save the world! No pressure, of course.")
                    });

                    sceneLines.Add(new[]
                    {
                        new ColorString(
                            "Old man: why, I was just reading on my fancy iPad device about ", 
                            new ColorString("3", ConsoleColor.Cyan), 
                            " lands corrupted by evil."),
                        new ColorString("Or maybe it was 'The top 3 evils that need to be vanquished right now' or some such..."),
                        new ColorString("Eh, regardless, you should go to the following regions- and I don't think order matters:"),
                        new ColorString(new ColorString("The desert ", ConsoleColor.Yellow), new ColorString("home of strong warriors")),
                        new ColorString(new ColorString("The crystal caves ", ConsoleColor.Cyan), "land of strange magic ", new ColorString("(not actually implemented)", ConsoleColor.Red)),
                        new ColorString(new ColorString("The casino ", ConsoleColor.Blue), "where luck may hold the key to your fate ", new ColorString("(not actually implemented)", ConsoleColor.Red)),
                        new ColorString("Good luck, heroes. If you aren't murdered violently by evil forces, I'll see that they sing songs about you!")
                    });

                    ret = new Cutscene(sceneLines.Select(sl => new SingleScene(sl)).Cast<CutsceneComponent>().ToArray());
                    break;
                case WorldSubRegion.DesertIntro:
                    sceneLines.AddRange(new[]
                    {
                        new[]
                        {
                            new ColorString("As the Barbarian runs as fast as his muscular legs can carry him,"),
                            new ColorString("your... mentor? Father figure?"),
                            new ColorString("We certainly haven't established anything about our character's family..."),
                            new ColorString("Regardless, the old man has returned, having witnessed your battle against the ",
                                new ColorString("Barbarian", ConsoleColor.Red))
                        },
                        new[]
                        {
                            new ColorString("Old man: Surprised to see me? Bingo got out early!"),
                            new ColorString("What a rascal that man is!"),
                            new ColorString("Still, even though he ran, he seemed rather strong..."),
                            new ColorString("I don't think you should just go blindly chasing after him, he might have more goons."),
                            new ColorString("Take a moment to strategize, and remember ", new ColorString("the gods", ConsoleColor.Cyan), " are watching."),
                            new ColorString("So... what will you do?") 
                        }
                    });

                    IEnumerable<CutsceneComponent> components = sceneLines.Select(sl => new SingleScene(sl));

                    components = components.Concat(new [] {new DecisionScene(Globals.GroupingKeys.FirstDesertGroupingId)});

                    sceneLines = new List<ColorString[]>
                    {
                        new[]
                        {
                            new ColorString("Old man: Oh, so that's how you'll approach him, eh?"),
                            new ColorString("Well then, I suggest you go to ",
                                new ColorString("that place ", ConsoleColor.Cyan), "and ",
                                new ColorString("that other place", ConsoleColor.Cyan)),
                            new ColorString("Though I can't say anything will happen much."),
                            new ColorString(
                                "What's that? Why can't I be more specific? And you want bonuses for clearing the next two areas?"),
                            new ColorString("Then try coming back when more of the game has been implemented! Jeez!"),
                            new ColorString("Back in my day we were grateful for every line of code, I tell you...")
                        }
                    };

                    components = components.Concat(sceneLines.Select(sl => new SingleScene(sl)));

                    ret = new Cutscene(components.ToArray());
                    break;
                default:
                    ret = null;
                    break;
            }

            return ret;
        }
    }
}
