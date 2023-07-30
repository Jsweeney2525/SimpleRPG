using System;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Barbarian : EnemyFighter, IBossEnemy
    {
        private static readonly IronBattleShield SuperShield = new IronBattleShield(1, 50, 0, 1, "a", "really impressive-looking iron shield");
        private static readonly ShieldMove SuperShieldMove = new ShieldMove("super shield", TargetType.Self, "creates an un-bustable shield", SuperShield);
        private IOutput _output;
        //TODO: this should be conditional, only displaying a message if the barbarian is equipped with a shield
        private static readonly DoNothingMove Taunt = new DoNothingMove(": Go on, I'll sit here with my impenetrable shield, fool!");
        
        private static readonly MultiTurnBattleMove FirstBattleRunawayMove = new MultiTurnBattleMove("runaway move", TargetType.Self,
            //TODO: this should be a color string that highlights how many turns are left
            new DoNothingMove(": That's it, I'm going to unleash my most powerful attack. Just give me two more turns!", 1),
            new DoNothingMove(": Only one more turn until I destroy you, you damn fools!", 1),
            MoveFactory.Get(BattleMoveType.Runaway));

        public Barbarian(int level, IChanceService chanceService, string name = null) :
            base(name ?? "Barbarian"
                , level
                , LevelUpManager.GetHealthByLevel<Barbarian>(level)
                , LevelUpManager.GetManaByLevel<Barbarian>(level)
                , LevelUpManager.GetStrengthByLevel<Barbarian>(level)
                , LevelUpManager.GetDefenseByLevel<Barbarian>(level)
                , LevelUpManager.GetSpeedByLevel<Barbarian>(level)
                , LevelUpManager.GetEvadeByLevel<Barbarian>(level)
                , LevelUpManager.GetLuckByLevel<Barbarian>(level)
                , chanceService
                , SpellFactory.GetSpellsByLevel<Barbarian>(level)
                , MoveFactory.GetMovesByLevel<Barbarian>(level))
        {
            _shieldBusterFails = 0;
        }

        public void PreBattleSetup(Team ownTeam, Team enemyTeam, IOutput output, BattleConfigurationSpecialFlag specialFlag)
        {
            _output = output;

            foreach (IFighter fighter in enemyTeam.Fighters)
            {
                fighter.SpecialMoveFailed += LogSpecialMoveFailed;
            }
        }

        public BattleMoveWithTarget GetZeroTurnMove(Team ownTeam, Team enemyTeam)
        {
            _output.WriteLine("Barbarian: I've seen you making your way through the desert.");
            _output.WriteLine("Do you really think your shield buster move will best me?");
            _output.WriteLine("I shall show you the futility of your efforts!");
            _output.Write("Behold, my "); _output.Write("unbustable shield", ConsoleColor.Cyan); _output.WriteLine("!");

            return new BattleMoveWithTarget(SuperShieldMove, this, this);
        }

        private int _shieldBusterFails;

        public void LogSpecialMoveFailed(object sender, SpecialMoveFailedEventArgs e)
        {
            if (e.Move is ShieldBusterMove)
            {
                ++_shieldBusterFails;

                switch (_shieldBusterFails)
                {
                    case 1:
                        _output.WriteLine("\nBarbarian: Ha! I already told you it was useless!\nTry it another thousand times, idiot, it'll be the same!");
                        break;
                    case 3:
                        _output.WriteLine("\nBarbarian: Huh? Did you crack my shield?\nHa! I'm just kidding, it won't work, fool!");
                        break;
                }
            }
        }

        public void SetOutput(IOutput output)
        {
            _output = output;
        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            if (BattleShield != null)
            {
                return Taunt;
            }
            else
            {
                return FirstBattleRunawayMove;
            }
        }
    }
}
