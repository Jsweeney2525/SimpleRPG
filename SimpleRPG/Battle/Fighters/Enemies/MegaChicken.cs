using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class MegaChicken: EnemyFighter
    {
        private readonly SpecialMove _layEgg;
        private readonly SpecialMove _lay2Eggs;
        private readonly SpecialMove _castEggs;

        private readonly List<Egg> _eggs;

        public MegaChicken(int level, IChanceService chanceService) 
            : base("Mega Chicken",
                  level,
                  LevelUpManager.GetHealthByLevel<MegaChicken>(level),
                  LevelUpManager.GetManaByLevel<MegaChicken>(level),
                  LevelUpManager.GetStrengthByLevel<MegaChicken>(level),
                  LevelUpManager.GetDefenseByLevel<MegaChicken>(level),
                  LevelUpManager.GetSpeedByLevel<MegaChicken>(level),
                  LevelUpManager.GetEvadeByLevel<MegaChicken>(level),
                  LevelUpManager.GetLuckByLevel<MegaChicken>(level),
                  chanceService, 
                  SpellFactory.GetSpellsByLevel<MegaChicken>(level)
                  , MoveFactory.GetMovesByLevel<MegaChicken>(level))
        {
            MagicStrength = 3;

            ExpGivenOnDefeat = 20 + level*5;

            _layEgg = new SpecialMove("lay egg", BattleMoveType.Special, TargetType.Self, null);
            _lay2Eggs = new SpecialMove("lay 2 eggs", BattleMoveType.Special, TargetType.Self, null);
            _castEggs = new SpecialMove("cast eggs", BattleMoveType.Special, TargetType.Self, null);
            _eggs = new List<Egg>();

        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            BattleMove move = null;

            var halfHealth = MaxHealth/2;

            if (CanCastEggs())
            {
                move = _castEggs;
            }

            move = move ?? ((CurrentHealth > halfHealth) ? _layEgg : _lay2Eggs);

            return move;
        }

        public override void ExecuteMove(BattleManager.BattleManager battleManager, BattleMoveWithTarget move, Team ownTeam, Team enemyTeam, IOutput output)
        {
            if (move.Move.Description == "cast eggs" && !CanCastEggs())
            {
                var newMove = SelectMove(ownTeam, enemyTeam);
                move = new BattleMoveWithTarget(newMove, move.Target, move.Owner);
            }

            switch (move.Move.Description)
            {
                case "lay 2 eggs":
                    LayEgg(ownTeam, output);
                    LayEgg(ownTeam, output);
                    break;
                case "cast eggs":
                    CastEggs(battleManager, ownTeam, enemyTeam, output);
                    break;
                default: //"lay egg" is assumed default move
                    LayEgg(ownTeam, output);
                    break;
            }
        }

        public override int PhysicalDamage(int amount)
        {
            if (amount > 1)
            {
                amount = 1;
            }

            return base.PhysicalDamage(amount);
        }

        private void LayEgg(Team ownTeam, IOutput output)
        {
            var typeIndex = ChanceService.WhichEventOccurs(Globals.EggMagicTypes.Length);
            var type = Globals.EggMagicTypes[typeIndex];

            var prefix = "a";

            if (type == MagicType.Ice)
            {
                prefix += "n";
            }

            var egg = (Egg)FighterFactory.GetFighter(FighterType.Egg, 1, null, type);
            egg.Killed += OnEggKilled;
            ownTeam.Add(egg);
            _eggs.Add(egg);
            output.WriteLine($"{DisplayName} laid {prefix} {egg.BaseName}!");
        }

        private bool CanCastEggs()
        {
            return _eggs.Count >= 3;
        }

        private void CastEggs(BattleManager.BattleManager battleManager, Team ownTeam, Team enemyTeam, IOutput output)
        {
            var types = new Dictionary<MagicType, int> { { MagicType.Fire, 0  }, { MagicType.Lightning, 0}, { MagicType.Ice, 0} };
            string spellName;

            for (var i = 0; i < 3; ++i)
            {
                var egg = _eggs[0];
                types[egg.MagicType] += 1;
                _eggs.Remove(egg);
                ownTeam.Remove(egg);
            }

            //all 3 match
            if (types[MagicType.Fire] == 3)
            {
                spellName = "inferno egg";
            }
            else if (types[MagicType.Fire] == 2)
            {
                spellName = "blaze egg";
            }
            else if (types[MagicType.Lightning] == 3)
            {
                spellName = "tempest egg";
            }
            else if (types[MagicType.Lightning] == 2)
            {
                spellName = "thunder egg";
            }
            else if (types[MagicType.Ice] == 3)
            {
                spellName = "blizzard egg";
            }
            else if (types[MagicType.Ice] == 2)
            {
                spellName = "frost egg";
            }
            else
            {
                spellName = "chaos egg";
            }

            output.WriteLine($"{DisplayName} draws in the power of its magic eggs, sacrificing them and casts {spellName}!");

            var spell = Spells.Single(s => s.Description == spellName);

            if (spellName != "chaos egg")
            {
                var target = _selectTarget(spell, ownTeam, enemyTeam);

                battleManager.ExecuteSpell(new BattleMoveWithTarget(spell, target, this), false);
            }
            else
            {
                foreach (var enemy in enemyTeam.Fighters.Where(f => f.IsAlive()))
                {
                    battleManager.ExecuteSpell(new BattleMoveWithTarget(spell, enemy, this), false);
                }

                battleManager.ExecuteSpell(new BattleMoveWithTarget(spell, this, this), false);
            }
        }

        private void OnEggKilled(object sender, KilledEventArgs e)
        {
            var egg = sender as Egg;
            _eggs.Remove(egg);
        }
    }
}
