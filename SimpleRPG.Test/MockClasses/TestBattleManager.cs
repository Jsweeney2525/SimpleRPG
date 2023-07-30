using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Test.MockClasses
{
    public class TestBattleManager : BattleManager
    {
        private readonly BattleManagerBattleConfiguration _myConfig;

        /// <summary>
        /// Every time <see cref="BattleManager.Battle(Team,Team,List{TerrainInteractable},BattleManagerBattleConfiguration)"/>
        /// is called, the enemy team is  added to this list
        /// </summary>
        private readonly List<Team> _enemyTeams;

        private readonly List<List<TerrainInteractable>> _terrainInteractableLists;

        private readonly Queue<BattleMoveQueue> _battleMoveQueues;

        public TestBattleManager(IChanceService chanceService, IInput input, IOutput output) 
                : base(chanceService, input, output)
        {
            _myConfig = new BattleManagerBattleConfiguration();
            _enemyTeams = new List<Team>();
            _terrainInteractableLists = new List<List<TerrainInteractable>>();
            _battleMoveQueues = new Queue<BattleMoveQueue>();
        }

        #region event handlers

        public void SuppressBattleIntroAndOutroMessages()
        {
            _myConfig.ShowIntroAndOutroMessages = false;
        }

        public void SuppressCastSpellMessages()
        {
            _myConfig.ShowCastSpellMessages = false;
        }

        public void SuppressMagicalDamageMessages()
        {
            _myConfig.ShowMagicalDamageMessages = false;
        }

        #endregion event handlers

        public override BattleEndStatus Battle(Team humanTeam, Team enemyTeam, 
            List<TerrainInteractable> terrainInteractables = null, 
            BattleManagerBattleConfiguration config = null)
        {
            _enemyTeams.Add(enemyTeam);
            _terrainInteractableLists.Add(terrainInteractables);
            return base.Battle(humanTeam, enemyTeam, terrainInteractables, config ?? _myConfig);
        }

        public List<Team> GetAllEnemyTeams()
        {
            return _enemyTeams;
        }

        public List<List<TerrainInteractable>> GetAllTerrainInteractableLists()
        {
            return _terrainInteractableLists;
        }

        public void SetEnemyTeam(Team enemyTeam)
        {
            if (_enemyTeam != null)
            {
                ClearTeamEvents(_enemyTeam);
            }

            _enemyTeam = enemyTeam;
            
            SetTeamEvents(_enemyTeam);
        }

        public void SetHumanTeam(Team team)
        {
            //remove old team
            if (_humanTeam != null)
            {
                ClearTeamEvents(_humanTeam);
            }

            //setup new team
            _humanTeam = team;

            SetTeamEvents(_humanTeam);
        }

        public void SetConfig(BattleManagerBattleConfiguration config)
        {
            Config = config;
        }

        public List<FieldEffectCounter> GetFieldEffects()
        {
            return Config.FieldEffectCounters;
        }

        public void ResetAllDanceEffects()
        {
            Config.ClearDanceEffects();
        }

        public List<BattleMoveWithTarget> GetInputs()
        {
            var testHumanTeam = _humanTeam as TestTeam;
            List<BattleMoveWithTarget> moves;

            if (testHumanTeam != null)
            {
                testHumanTeam.InitializeForBattle(_enemyTeam, _input, _output);
                moves = testHumanTeam.GetInputs(_enemyTeam);
            }
            else
            {
                moves = _humanTeam.GetInputs(_enemyTeam);
            }
            moves.AddRange(_enemyTeam.GetInputs(_humanTeam));
            moves = moves.Where(m => m != null).OrderByDescending(move => CalculateEffectiveSpeed(move.Owner)).ToList();

            return moves;
        }

        public void ImplementEffects(CombinedFieldEffect effect, int effectDuration, IFighter dancer1, IFighter dancer2)
        {
            effect.Effects.ForEach(fe => ImplementEffect(fe, effectDuration, dancer1, dancer2));
        }

        /// <summary>
        /// Performs the OnTurnEnd() method
        /// </summary>
        public void TestOnTurnEnd()
        {
            OnRoundEnd();
        }

        public void TestPrintKilledMessage(object sender, KilledEventArgs e)
        {
            PrintKilledMessage(sender, e);
        }

        public void SetBattleMoveQueues(params BattleMoveQueue[] battleMoveQueues)
        {
            foreach (BattleMoveQueue queue in battleMoveQueues)
            {
                _battleMoveQueues.Enqueue(queue);
            }
        }

        protected override BattleMoveQueue GetOrderedInputs()
        {
            return _battleMoveQueues.Count == 0 ? base.GetOrderedInputs() : _battleMoveQueues.Dequeue();
        }
    }
}
