using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class HumanControlledEnemyFighter : EnemyFighter
    {
        private readonly IInput _input;

        private readonly IOutput _output;

        private readonly IMenuFactory _menuFactory;

        public EnemyFighter Fighter { get; private set; }

        public HumanControlledEnemyFighter(string name, IInput input, IOutput output, IMenuFactory menuFactory) 
            : base(name, 1, 1, 0, 0, 0, 0, 0, 0, null)
        {
            ExpGivenOnDefeat = 0;

            _input = input;
            _output = output;
            _menuFactory = menuFactory;
        }

        public void SetEnemy(EnemyFighter fighter)
        {
            Fighter = fighter;

            MaxHealth = fighter.MaxHealth;
            CurrentHealth = MaxHealth;
            MaxMana = fighter.MaxMana;
            CurrentMana = MaxMana;
            MagicStrength = fighter.MagicStrength;
            MagicResistance = fighter.MagicResistance;
            Strength = fighter.Strength;
            Defense = fighter.Defense;
            Speed = fighter.Speed;
            Evade = fighter.Evade;
            Luck = fighter.Luck;
            BaseName = fighter.BaseName;
            ExpGivenOnDefeat = fighter.ExpGivenOnDefeat;

            _availableMoves = fighter.AvailableMoves;
            Spells = fighter.Spells;
        }

        //only used for multi turn moves
        private IFighter _selectedTarget;

        public override BattleMoveWithTarget SetupMove(Team ownTeam, Team enemyTeam)
        {
            BattleMoveWithTarget ret;

            BattleMove move = BeforeSelectMove();

            if (move == null)
            {
                HumanControlledEnemyMenu menu = new HumanControlledEnemyMenu(_input, _output, _menuFactory);

                menu.Build(this, ownTeam, enemyTeam, null);

                MenuSelection menuSelection = menu.GetInput();

                move = menuSelection.Move;
                _selectedTarget = menuSelection.Target;
                ret = new BattleMoveWithTarget(menuSelection, this);
            }
            else
            {
                ret = new BattleMoveWithTarget(move, _selectedTarget, this);
            }

            AfterSelectMove(move);

            return ret;
        }
    }
}
