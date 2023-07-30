using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Regions
{
    public class SubRegion : Area<WorldSubRegion>
    {
        /// <summary>
        /// How many battles before the boss
        /// </summary>
        public int NumberRegularBattles { get; }

        /// <summary>
        /// Which enemies can be encountered here, and how rare each is
        /// </summary>
        public IEnumerable<ChanceEvent<FighterType>> EnemyTypes { get; }

        /// <summary>
        /// Sets the various chances of running into an enemy group of X fighters
        /// </summary>
        public IEnumerable<ChanceEvent<int>> NumberEnemyFighterChances { get; }

        public List<ScriptedBattlefieldConfiguration> ScriptedBattlefieldConfigurations { get; }

        /// <summary>
        /// The description of what the boss fight will look like
        /// </summary>
        public BattlefieldConfiguration BossConfiguration { get; }

        public Cutscene RegionCompletedCutscene { get; }

        #region events

        public EventHandler<SubRegionCompletedEventArgs> SubRegionCompleted { get; set; }

        public void OnSubRegionCompleted(SubRegionCompletedEventArgs e)
        {
            SubRegionCompleted?.Invoke(this, e);
        }

        public EventHandler<FooEventArgs> Fooed { get; set; }

        #endregion

        private static IEnumerable<ChanceEvent<FighterType>> ConvertToChanceEvents(IEnumerable<FighterType> enemyTypes)
        {
            List<FighterType> enemyTypesList = enemyTypes.ToList();
            int enemyTypesCount = enemyTypesList.Count;
            double enemyTypeChance = 1.0 / enemyTypesCount;

            return enemyTypesList.Select(et => new ChanceEvent<FighterType>(et, enemyTypeChance));
        }

        /// <summary>
        /// Call this constructor when all enemy types have an equal chance of being selected, it will construct the appropriate input into the "real" constructor
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="numberRegularBattles"></param>
        /// <param name="numberEnemyFighterChances"></param>
        /// <param name="enemyTypes"></param>
        /// <param name="bossConfiguration"></param>
        /// <param name="scriptedBattlefieldConfigurations"></param>
        /// <param name="regionIntro"></param>
        /// <param name="regionCompletedCutscene"></param>
        public SubRegion(WorldSubRegion areaId,
            int numberRegularBattles,
            IEnumerable<ChanceEvent<int>> numberEnemyFighterChances,
            IEnumerable<FighterType> enemyTypes,
            BattlefieldConfiguration bossConfiguration,
            List<ScriptedBattlefieldConfiguration> scriptedBattlefieldConfigurations = null, 
            string regionIntro = null,
            Cutscene regionCompletedCutscene = null) 
            : this(areaId, numberRegularBattles, numberEnemyFighterChances, ConvertToChanceEvents(enemyTypes),
                  bossConfiguration, scriptedBattlefieldConfigurations, regionIntro, regionCompletedCutscene)
        {
        }

        public SubRegion(WorldSubRegion areaId, 
            int numberRegularBattles,
            IEnumerable<ChanceEvent<int>> numberEnemyFighterChances,
            IEnumerable<ChanceEvent<FighterType>> enemyTypes, 
            BattlefieldConfiguration bossConfiguration,
            List<ScriptedBattlefieldConfiguration> scriptedBattlefieldConfigurations = null,
            ColorString regionIntro = null,
            Cutscene regionCompletedCutscene = null) :
            base(areaId, regionIntro)
        {
            NumberEnemyFighterChances = numberEnemyFighterChances;
            NumberRegularBattles = numberRegularBattles;
            EnemyTypes = enemyTypes;
            BossConfiguration = bossConfiguration;
            ScriptedBattlefieldConfigurations = scriptedBattlefieldConfigurations ??
                                                new List<ScriptedBattlefieldConfiguration>();
            RegionCompletedCutscene = regionCompletedCutscene;

            if (RegionCompletedCutscene != null)
            {
                RegionCompletedCutscene.Fooed += PropagateFooEvent;
            }
        }

        public void ExecuteCutscene(IInput input, IOutput output, HumanFighter fighter1, HumanFighter fighter2)
        {
            RegionCompletedCutscene?.ExecuteCutscene(input, output, fighter1, fighter2);
        }

        private void PropagateFooEvent(object sender, FooEventArgs e)
        {
            Fooed?.Invoke(sender, e);
        }
    }
}
