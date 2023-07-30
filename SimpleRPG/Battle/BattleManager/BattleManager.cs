using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Battle.BattleManager
{
    public class BattleManagerBattleConfiguration
    {
        public bool ShowIntroAndOutroMessages;

        public bool ShowCastSpellMessages;

        public bool ShowMagicalDamageMessages;

        //if true, will display all "___ attacked ___" messages, including crits and misses
        public bool ShowAttackMessages;

        //if true, will display all "___ took x damage" messages if they were the result of physical damage
        public bool ShowPhysicalDamageMessages;

        //if true, will display "____ has died" messages
        public bool ShowDeathMessages;

        public bool ShowExpAndLevelUpMessages;

        public bool ShowShieldAddedMessage;

        public FieldEffectCombiner FieldEffectCombiner;

        public bool ContinueBattling;

        public bool HumanTeamRan;

        public bool EnemyTeamRan;

        public int ExpGainedOnVictory;

        public BattleEndStatus EndStatus;

        public List<FieldEffectCounter> FieldEffectCounters;

        public List<DanceEffectCounter> HumanDanceEffects;

        public List<DanceEffectCounter> EnemyDanceEffects;

        public BattleConfigurationSpecialFlag SpecialBattleFlag;

        public BattleManagerBattleConfiguration()
        {
            ShowIntroAndOutroMessages = true;
            ShowCastSpellMessages = true;
            ShowMagicalDamageMessages = true;
            ShowAttackMessages = true;
            ShowPhysicalDamageMessages = true;
            ShowDeathMessages = true;
            ShowExpAndLevelUpMessages = true;
            ShowShieldAddedMessage = true;
            FieldEffectCombiner = new FieldEffectCombiner();
            ContinueBattling = true;
            HumanTeamRan = false;
            EnemyTeamRan = false;
            ExpGainedOnVictory = 0;
            EndStatus = BattleEndStatus.None;
            SpecialBattleFlag = BattleConfigurationSpecialFlag.None;

            FieldEffectCounters = new List<FieldEffectCounter>();
            HumanDanceEffects = new List<DanceEffectCounter>();
            EnemyDanceEffects = new List<DanceEffectCounter>();

        }

        public List<DanceEffectCounter> GetDanceEffects(bool getHumanEffects)
        {
            return getHumanEffects ? HumanDanceEffects : EnemyDanceEffects;
        }

        public List<DanceEffectCounter> GetAllDanceEffects()
        {
            return HumanDanceEffects.Concat(EnemyDanceEffects).ToList();
        }

        public void ClearDanceEffects()
        {
            HumanDanceEffects.Clear();
            EnemyDanceEffects.Clear();
        }

        public void ClearAllCounters()
        {
            FieldEffectCounters.Clear();
            ClearDanceEffects();
        }
    }

    public partial class BattleManager
    {
        protected Team _humanTeam;

        protected Team _enemyTeam;
        
        protected IInput _input;

        protected IOutput _output;
        
        protected IChanceService _chanceService;

        protected List<TerrainInteractable> TerrainInteractables;

        protected BattleManagerBattleConfiguration Config;

        #region events

        public EventHandler<FieldEffectExecutedEventArgs> FieldEffectExecuted { get; set; }

        protected void OnFieldEffectExecuted(FieldEffectExecutedEventArgs e)
        {
            FieldEffectExecuted?.Invoke(this, e);
        }

        protected static EventHandler<BattleEndedEventArgs> BattleEnded { get; set; }

        protected void OnBattleEnded(BattleEndedEventArgs e)
        {
            BattleEnded?.Invoke(this, e);
        }

        #endregion events

        public BattleManager(IChanceService chanceService, IInput input, IOutput output)
        {
            _input = input;
            _output = output;
            _chanceService = chanceService;

            FieldEffectExecuted += PrintExecutedFieldEffect;
        }

        public virtual BattleEndStatus Battle(Team humanTeam, Team enemyTeam, 
            List<TerrainInteractable> terrainInteractables = null, BattleManagerBattleConfiguration config = null)
        {
            TerrainInteractables = terrainInteractables ?? new List<TerrainInteractable>();
            Config = config ?? new BattleManagerBattleConfiguration();
            _humanTeam = humanTeam;
            _enemyTeam = enemyTeam;

            BeforeBattle();

            while (Config.ContinueBattling)
            {
                var moves = GetOrderedInputs();

                if (Config.HumanTeamRan) break;

                ExecuteMoves(moves);

                OnRoundEnd();
            }

            AfterBattle();

            return Config.EndStatus;
        }

        protected List<MenuAction> GetSpecialMenuActions()
        {
            List<MenuAction> ret = TerrainInteractables.SelectMany(interactable => interactable.GetInteractableMenuActions(_input, _output)).ToList();

            return ret;
        }

        protected virtual BattleMoveQueue GetOrderedInputs()
        {
            List<BattleMoveWithTarget> moves = _humanTeam.GetInputs(_enemyTeam, GetSpecialMenuActions());

            if (Config.HumanTeamRan)
            {
                return null;
            }
            
            moves.AddRange(_enemyTeam.GetInputs(_humanTeam, GetSpecialMenuActions()));
            moves = moves.Where(m => m != null).ToList();
            BattleMoveQueue ret = new BattleMoveQueue(moves);
            ret.Sort(CalculateEffectiveSpeed);

            return ret;
        }

        protected void ExecuteMoves(BattleMoveQueue queue)
        {
            while(Config.ContinueBattling && queue.Count > 0)
            {
                BattleMoveWithTarget move = queue.SortAndPop(CalculateEffectiveSpeed);
                
                if (CanMoveBeExecuted(move))
                {
                    if (move.Target != null && !move.Target.IsAlive())
                    {
                        SelectNewTarget(move);
                    }

                    ExecuteMove(move);

                    var doNothingMove = move.Move as DoNothingMove;

                    if (move.Move.MoveType != BattleMoveType.DoNothing ||
                        //wait and clear if the move was anything but a do nothing move
                        (!string.IsNullOrEmpty(doNothingMove?.Message)))
                        //wait and clear if the move was a doNothing but it had a valid message to be displayed
                    {
                        _input.WaitAndClear(_output);
                    }
                }

                move.Owner.OnTurnEnded(new TurnEndedEventArgs(move.Owner));
            }
        }

        protected bool CanMoveBeExecuted(BattleMoveWithTarget moveWithTarget)
        {
            bool ret;
            IFighter owner = moveWithTarget.Owner;
            BattleMove move = moveWithTarget.Move;

            if (!owner.IsAlive())
            {
                ret = false;
            }
            else if (move.MoveType == BattleMoveType.Spell)
            {
                var spell = move as Spell;

                if (spell == null)
                {
                    throw new ArgumentException($"The battle move {move.Description} is classified as a spell type, but cannot be cast to the Spell class!");
                }

                if (!owner.HasSpell(spell))
                {
                    throw new ArgumentException($"Fighter {owner.DisplayName} cannot cast {spell.Description}, they have not learned this spell!");
                }

                if (owner.Statuses.Exists(s => s is MagicSealedStatus))
                {
                    ret = false;
                }
                else
                {
                    int realCost = CalculateSpellCost(owner, spell);

                    ret = realCost <= owner.CurrentMana;
                }
            }
            else
            {
                ret = true;
            }

            return ret;
        }

        protected void ExecuteMove(BattleMoveWithTarget move)
        {
            if (!move.Target.IsAlive())
            {
                return;
            }

            SpecialMoveExecutedEventArgs e;
            string executionText;

            switch (move.Move.MoveType)
            {
                case BattleMoveType.Runaway:
                    if (_humanTeam.Contains(move.Owner))
                    {
                        OnHumanTeamRan(_humanTeam, new RanEventArgs());
                    }
                    else
                    {
                        OnEnemyTeamRan(_enemyTeam, new RanEventArgs());
                    }
                    break;
                case BattleMoveType.ConditionalPowerAttack:
                    IConditionalPowerExecutor conditionalPowerExecutor = move.Owner as IConditionalPowerExecutor;
                    ConditionalPowerAttackBattleMove conditionalAttack = move.Move as ConditionalPowerAttackBattleMove;
                    conditionalAttack.SetConditionalPower(conditionalPowerExecutor.GetAttackBonus());
                    goto case BattleMoveType.Attack;
                case BattleMoveType.Attack:
                    ExecuteAttack(move);
                    break;
                case BattleMoveType.Spell:
                    ExecuteSpell(move);
                    break;
                case BattleMoveType.DoNothing:
                    var doNothing = move.Move as DoNothingMove;
                    if (doNothing == null)
                    {
                        throw new ArgumentException(
                            $"Error! The battle move '{move.Move.Description}' had BattleMoveType DoNothing but was not a DoNothingMove!");
                    }
                    if (!string.IsNullOrEmpty(doNothing.Message))
                    {
                        _output.WriteLine($"{move.Owner.DisplayName} {doNothing.Message}");
                    }
                    break;
                case BattleMoveType.Special:
                    var enemy = move.Owner as EnemyFighter;
                    if (enemy == null)
                    {
                        throw new NotImplementedException(
                            $"Error! This player attempted to use the forbidden technique!\nFighter: {move.Owner.DisplayName}\n Move: {move.Move.Description}");
                    }
                    enemy.ExecuteMove(this, move, _enemyTeam, _humanTeam, _output);
                    break;
                case BattleMoveType.Dance:
                case BattleMoveType.MultiTurn:
                    var multiTurn = move.Move as MultiTurnBattleMove;
                    if (multiTurn == null)
                    {
                        throw new ArgumentException(
                            $"Error! The battle move '{move.Move.Description}' " +
                            $"had BattleMoveType '{move.Move.MoveType}' " +
                            "but could not be cast to type MultiTurnBattleMove!");
                    }
                    var withTarget = new BattleMoveWithTarget(multiTurn.Moves[0], move.Target, move.Owner);
                    ExecuteMove(withTarget);
                    break;
                case BattleMoveType.Field:
                    break;
                case BattleMoveType.Status:
                    StatusMove statusMove = move.Move as StatusMove;

                    if (statusMove == null)
                    {
                        throw new ArgumentException(
                           $"Error! The battle move '{move.Move.Description}' " +
                           $"had BattleMoveType '{move.Move.MoveType}' " +
                           "but could not be cast to type StatusMove!");
                    }

                    e = new SpecialMoveExecutedEventArgs(move.Owner, move.Target, statusMove);
                    move.Owner.OnSpecialMoveExecuted(e);

                    double statusWorksChance = statusMove.Accuracy/100.0;

                    if (_chanceService.EventOccurs(statusWorksChance))
                    {
                        ExecuteStatus(statusMove.Status, move.Target);
                    }
                    break;
                case BattleMoveType.Shield:

                    ShieldMove shieldMove = move.Move as ShieldMove;

                    if (shieldMove == null)
                    {
                        throw new ArgumentException(
                           $"Error! The battle move '{move.Move.Description}' " +
                           $"had BattleMoveType '{move.Move.MoveType}' " +
                           "but could not be cast to type ShieldMove!");
                    }

                    e = new SpecialMoveExecutedEventArgs(move.Owner, move.Target, shieldMove);
                    move.Owner.OnSpecialMoveExecuted(e);

                    ExecuteShieldMove(shieldMove, move.Target);

                    break;
                case BattleMoveType.ShieldFortifier:

                    ShieldFortifyingMove shieldFortifyingMove = move.Move as ShieldFortifyingMove;

                    if (shieldFortifyingMove == null)
                    {
                        throw new ArgumentException(
                           $"Error! The battle move '{move.Move.Description}' " +
                           $"had BattleMoveType '{move.Move.MoveType}' " +
                           "but could not be cast to type shieldFortifyingMove!");
                    }

                    e = new SpecialMoveExecutedEventArgs(move.Owner, move.Target, shieldFortifyingMove);
                    move.Owner.OnSpecialMoveExecuted(e);

                    ExecuteShieldFortifyingMove(shieldFortifyingMove, move.Target);

                    break;
                case BattleMoveType.ShieldBuster:

                    ShieldBusterMove shieldBusterMove = move.Move as ShieldBusterMove;

                    if (shieldBusterMove == null)
                    {
                        throw new ArgumentException(
                           $"Error! The battle move '{move.Move.Description}' " +
                           $"had BattleMoveType '{move.Move.MoveType}' " +
                           "but could not be cast to type ShieldBusterMove!");
                    }

                    //TODO: this should be something like "on special move about to be fired" or something
                    e = new SpecialMoveExecutedEventArgs(move.Owner, move.Target, shieldBusterMove);
                    move.Owner.OnSpecialMoveExecuted(e);

                    ExecuteShieldBusterMove(shieldBusterMove, move.Owner, move.Target);
                    break;
                case BattleMoveType.BellMove:
                    executionText = string.Copy(((BellMove)move.Move).ExecutionText);
                    IFighter owner = move.Owner;

                    executionText = executionText.Replace(Globals.OwnerReplaceText, owner.DisplayName);
                    executionText = executionText.Replace(Globals.TargetReplaceText, move.Target.DisplayName);

                    BellMoveType moveType = ((BellMove) move.Move).BellMoveType;

                    int valueBefore = moveType == BellMoveType.SealMove ? owner.CurrentHealth : owner.MaxHealth;

                    _output.WriteLine($"{move.Owner.DisplayName} {executionText}");

                    IMoveExecutor moveExecutor = move.MoveExecutor;

                    bool bellMoveSucceeded = moveExecutor.ExecuteMove(move);

                    if (bellMoveSucceeded)
                    {
                        string successMessage = null;
                        string resultMessage = null;
                        int valueAfter, valueDiff;

                        switch (moveType)
                        {
                            case BellMoveType.SealMove:
                                successMessage = $"{move.Target.DisplayName} has been sealed!";

                                valueAfter = owner.CurrentHealth;
                                valueDiff = valueAfter - valueBefore;
                                resultMessage = $"{owner.DisplayName} has been healed for {valueDiff} HP!";
                                break;
                            case BellMoveType.ControlMove:
                                successMessage = $"{move.Target.DisplayName} has been absorbed!";

                                valueAfter = owner.MaxHealth;
                                valueDiff = valueAfter - valueBefore;
                                resultMessage = $"{owner.DisplayName} has had their max HP increased by {valueDiff}!";
                                break;
                        }

                        _output.WriteLine(successMessage);
                        _output.WriteLine(resultMessage);
                    }
                    else
                    {
                        _output.WriteLine($"but {move.Target.DisplayName} was too strong!");
                    }
                    break;
                case BattleMoveType.AbsorbShade:
                    //TODO remove the null check, have HumanControlledEnemies be the normal enemy type but menu driven inpu
                    Shade ownerAsShade = move.Owner as Shade ?? ((HumanControlledEnemyFighter)move.Owner).Fighter as Shade;
                    Shade targetAsShade = move.Target as Shade ?? ((HumanControlledEnemyFighter)move.Owner).Fighter as Shade;

                    if (ownerAsShade == null)
                    {
                        throw new InvalidOperationException("A move of type 'AbsorbShade' was used by something other than a shade, the only class currently able to implement the move");
                    }
                    if (targetAsShade == null)
                    {
                        throw new InvalidOperationException("A move of type 'AbsorbShade' was used to target something other than a shade, currently the only valid type to target");
                    }

                    executionText = $"{move.Owner.DisplayName} {move.Move.ExecutionText}";

                    executionText = executionText.Replace(Globals.TargetReplaceText, move.Target.DisplayName);
                    executionText = executionText.Replace(Globals.OwnerReplaceText, move.Owner.DisplayName);

                    _output.WriteLine(executionText);

                    ownerAsShade.AbsorbShade(targetAsShade);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(move), $"BattleMove's move type was not recognized. MoveType: {move.Move.MoveType}");
            }

            var fieldEffectMove = move.Move as IFieldEffectMove;

            if (fieldEffectMove != null)
            {
                ExecuteFieldTechnique(move);
            }
        }

        protected void ExecuteAttack(BattleMoveWithTarget move, bool isCounter = false)
        {
            IFighter attacker = move.Owner;
            IFighter target = move.Target;
            AttackBattleMove attackMove = move.Move as AttackBattleMove;

            if (attackMove == null)
            {
                throw new ArgumentException("BattleManager.ExecuteAttack() was called with a BattleMoveWithTarget that did not have an AttackBattleMove move!");
            }

            if (Config.ShowAttackMessages && !isCounter)
            {
                string attackMessage = $"{attacker.DisplayName} attacks {target.DisplayName}!";

                if (!string.IsNullOrEmpty(attackMove.ExecutionText))
                {
                    attackMessage = $"{attacker.DisplayName} {attackMove.ExecutionText}!";

                    attackMessage = attackMessage.Replace(Globals.TargetReplaceText, target.DisplayName);
                }

                _output.WriteLine(attackMessage);
            }

            bool shouldAlsoCounter;

            if (DoesDefenderDodge(target, attackMove, out shouldAlsoCounter))
            {
                AutoEvadedEventArgs e = new AutoEvadedEventArgs(target, shouldAlsoCounter);
                target.OnAutoEvaded(e);

                if (shouldAlsoCounter)
                {
                    ExecuteCounterAttack(attacker, target);
                }
            }
            else if (!DoesAttackHit(attackMove, attacker, target))
            {
                attacker.OnAttackMissed(new AttackMissedEventArgs(target));
            }
            else
            {
                var isCrit = IsCriticalHit(attackMove, attacker);

                if (isCrit)
                {
                    attacker.OnCriticalAttack(new CriticalAttackEventArgs());
                }

                int attackStrength = CalculateAttackPower(attacker, target, attackMove, isCrit);
                int calculatedDefense = CalculateDefensePower(target);

                int calculatedDamage = attackStrength - calculatedDefense;
                calculatedDamage = Math.Max(0, calculatedDamage);
                var damageDealt = target.PhysicalDamage(calculatedDamage);

                attacker.OnAttackSuccessful(new AttackSuccessfulEventArgs(target, damageDealt));
                
                if (!target.IsAlive())
                {
                    attacker.OnEnemyKilled(new EnemyKilledEventArgs(target));
                }

                IEnumerable<BattleMoveEffect> onAttackHitEffects = move.Move.BattleMoveEffects
                    .Where(e => e.EffectActivationType == BattleMoveEffectActivationType.OnAttackHit && IsEffectConditionMet(e, move));
                foreach (BattleMoveEffect effect in onAttackHitEffects)
                {
                    ExecuteOnSuccessfulAttackBattleMoveEffect(effect, move);
                }

                if (!isCounter && DoesDefenderCounter(target))
                {
                    EnemyAttackCounteredEventArgs e = new EnemyAttackCounteredEventArgs(target, attacker);
                    target.OnEnemyAttackCountered(e);

                    ExecuteCounterAttack(attacker, target);
                }
            }
        }

        public void ExecuteCounterAttack(IFighter attacker, IFighter target)
        {
            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);
            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(attack, attacker, target);
            ExecuteAttack(moveWithTarget, true);
        }

        /// <summary>
        /// Determiens the damage done by a particular spell, printing out who used the spell against whom
        /// </summary>
        /// <param name="move">The move that determines who is casting the spell, who is targetted, and which spell is being cast</param>
        /// <param name="showCast">If true, displays a line like "Joe casts fireball against Bill!", if false, omits this output </param>
        public void ExecuteSpell(BattleMoveWithTarget move, bool showCast = true)
        {
            var spell = move.Move as Spell;

            if (spell == null)
            {
                throw new ArgumentException("BattleManager.ExecuteSpell() called with a BattleMoveWithTarget that did not contain a spell type BattleMove", nameof(move));
            }

            IFighter owner = move.Owner;

            int spellCost = CalculateSpellCost(owner, spell);

            owner.SpendMana(spellCost);

            if (showCast && Config.ShowCastSpellMessages)
            {
                _output.WriteLine(
                    $"{owner.DisplayName} casts {move.Move.Description} against {move.Target.DisplayName}!");
            }

            int magicPower = CalculateMagicalStrength(owner, spell);
            IFighter target = move.Target;
            double reflectMultiplier = 1.0;

            if (ShouldReflect(spell, target, ref reflectMultiplier))
            {
                if (showCast && Config.ShowCastSpellMessages)
                {
                    _output.WriteLine("But it was reflected!");
                }
                target = owner;
                
                if (ShouldReflect(spell, owner))
                {
                    _output.WriteLine("And then was reflected again, disappearing into nothingness!");
                    return;
                }
            }

            magicPower = (int) (magicPower*reflectMultiplier);
            int magicDefense = CalculateEffectiveResistance(target, spell.ElementalType);
            int effectivePower = magicPower - magicDefense;
            effectivePower = Math.Max(effectivePower, 0);

            //TODO: Not all spell types are offensive!
            int damageDealt = target.MagicalDamage(effectivePower, spell.ElementalType);

            owner.OnSpellSuccessful(new SpellSuccessfulEventArgs(target, spell, damageDealt));
            
            if (!target.IsAlive())
            {
                owner.OnEnemyKilled(new EnemyKilledEventArgs(target));
            }

        }

        protected bool ShouldReflect(Spell spell, IFighter target)
        {
            double dummy = 0.0;
            return ShouldReflect(spell, target, ref dummy);
        }

        protected bool ShouldReflect(Spell spell, IFighter target, ref double reflectMultiplier)
        {
            Func<ReflectStatus, bool> getStatusesByType = s => s.MagicType == spell.ElementalType || s.MagicType == MagicType.All;

            IEnumerable<ReflectStatus> individualStatuses = target.Statuses.OfType<ReflectStatus>().Where(getStatusesByType).ToList();
            
            bool shouldReflect = individualStatuses.Any();
            if (shouldReflect)
            {
                IEnumerable<double> multipliers = individualStatuses.Select(s => s.MultiplierBonus);
                reflectMultiplier = multipliers.Max();
            }

            return shouldReflect;
        }

        /// <summary>
        /// Adds the field effect and their durations to the list of field effect moves.
        /// If the move is a dance move, will also add the dance effects to the list of dance effects
        /// </summary>
        /// <param name="move"></param>
        protected void ExecuteFieldTechnique(BattleMoveWithTarget move)
        {
            var fieldMove = move.Move as IFieldEffectMove;

            if (fieldMove == null)
            {
                throw new ArgumentException(
                    $"Error! The battle move '{move.Move.Description}' had BattleMoveType 'Field' but was not a FieldEffectMove!");
            }

            fieldMove.FieldEffects.ForEach(e =>
            {
                var hasOwnerFieldEffect = e as IHasOwnerFieldEffect;
                hasOwnerFieldEffect?.SetOwner(move.Owner);
                ImplementEffect(e, e.EffectDuration ?? fieldMove.EffectDuration, move.Owner);
            });

            var danceMove = move.Move as DanceMove;

            if (danceMove != null && danceMove.DanceEffect != DanceEffectType.None)
            {
                AddDanceEffect(danceMove, fieldMove.EffectDuration, move.Owner);
            }
        }

        /// <summary>
        /// Adds dance effects to the list of dance effects. If a combination for dance effects exists, implements that
        /// </summary>
        /// <param name="danceMove"></param>
        /// <param name="effectDuration"></param>
        /// <param name="owner"></param>
        protected void AddDanceEffect(DanceMove danceMove, int effectDuration, IFighter owner)
        {
            var isOwnerHuman = IsFighterHuman(owner);
            var danceEffects = Config.GetDanceEffects(isOwnerHuman);

            foreach (var effect in danceEffects)
            {
                var fieldEffect = Config.FieldEffectCombiner.Combine(danceMove.DanceEffect, effect.Effect);

                if (fieldEffect != null)
                {
                    _output.WriteLine($"They combined to become {fieldEffect.Description}");

                    fieldEffect.Effects.ForEach(fe =>
                    {
                        (fe as IHasOwnerFieldEffect)?.SetOwner(effect.Owner);

                        ImplementEffect(fe, effectDuration, owner, effect.Owner);
                    });
                }
            }

            danceEffects.Add(new DanceEffectCounter(owner, effectDuration, danceMove.DanceEffect));
        }

        protected List<Team> GetFieldEffectTargettedTeam(IFighter owner, TargetType targetType)
        {
            List<Team> ret = new List<Team>();
            var ownerIsHuman = IsFighterHuman(owner);

            switch (targetType)
            {
                case TargetType.OwnTeam:
                    ret.Add(ownerIsHuman ? _humanTeam : _enemyTeam);
                    break;
                case TargetType.EnemyTeam:
                    ret.Add(ownerIsHuman ? _enemyTeam : _humanTeam);
                    break;
                case TargetType.Field:
                    ret.Add(_humanTeam);
                    ret.Add(_enemyTeam);
                    break;
            }

            return ret;
        }

        protected void ExecuteFieldEffect(FieldEffectCounter fieldEffect)
        {
            List<Team> teams = GetFieldEffectTargettedTeam(fieldEffect.Owners[0], fieldEffect.Effect.TargetType);

            foreach (Team team in teams)
            {
                ExecuteFieldEffect(fieldEffect.Effect, team, fieldEffect.Owners.ToArray());
            }

            fieldEffect.DecrementTurnsLeft();
        }

        //TODO: can just make this several different ExecuteFieldEffect() methods, where each takes a specific FieldEffect type (MagicAttack, UndoDebuffs, etc.)
        protected void ExecuteFieldEffect(FieldEffect effect, Team targettedTeam, params IFighter[] owners)
        {
            UndoDebuffsFieldEffect undoDebuff = effect as UndoDebuffsFieldEffect;
            MagicAttackFieldEffect magicAttack = effect as MagicAttackFieldEffect;
            ShieldFieldEffect shieldEffect = effect as ShieldFieldEffect;
            RestoreHealthPercentageFieldEffect restoreHealthEffect = effect as RestoreHealthPercentageFieldEffect;
            RestoreManaPercentageFieldEffect restoreManaEffect = effect as RestoreManaPercentageFieldEffect;

            if (undoDebuff != null)
            {
                //TODO: create an IStatusable class that both Fighter and Team implement?
                ClearDebuffs(targettedTeam);
            }
            if (magicAttack != null)
            {
                foreach (IFighter fighter in targettedTeam.Fighters.Where(f => f.IsAlive()))
                {
                    int magicalDamage = CalculateMagicalStrength(magicAttack.Owner, magicAttack.MagicType,
                        magicAttack.Power);

                    fighter.MagicalDamage(magicalDamage, magicAttack.MagicType);
                }
            }
            if (shieldEffect != null)
            {
                foreach (IFighter fighter in targettedTeam.Fighters)
                {
                    fighter.SetBattleShield(shieldEffect.BattleShield as BattleShield);
                }
            }
            if (restoreHealthEffect != null)
            {
                foreach (IFighter fighter in targettedTeam.Fighters)
                {
                    double percentage = ((double)restoreHealthEffect.Percentage) / 100.0;
                    int healAmount = (int) (fighter.MaxHealth*percentage);
                    fighter.Heal(healAmount);
                }
            }
            if (restoreManaEffect != null)
            {
                foreach (IFighter fighter in targettedTeam.Fighters)
                {
                    double percentage = ((double) restoreManaEffect.Percentage)/100.0;
                    int restoreAmount = (int) (fighter.MaxMana*percentage);
                    fighter.RestoreMana(restoreAmount);
                }

            }
        }
        
        protected void ExecuteDanceEffect(DanceEffectCounter danceEffect)
        {
            danceEffect.DecrementTurnsLeft();
        }

        protected void ExecuteStatus(Status status, IFighter target)
        {
            UndoDebuffsStatus undoDebuff = status as UndoDebuffsStatus;

            if (undoDebuff != null)
            {
                target.RemoveStatuses(s => 
                s is StatMultiplierStatus 
                || s is CriticalChanceMultiplierStatus
                || s is MagicMultiplierStatus
                || s is MagicResistanceMultiplierStatus
                || s is SpellCostMultiplierStatus, 
                false);

                //TODO: need a test for event raised when fighter debuffed
            }
            else
            {
                target.AddStatus(status);
            }
        }

        protected bool IsEffectConditionMet(BattleMoveEffect effect, BattleMoveWithTarget battleMove)
        {
            return IsEffectConditionMet(effect, battleMove.Owner, battleMove.Target);
        }

        protected bool IsEffectConditionMet(BattleMoveEffect effect, IFighter moveExecutor, IFighter moveTarget)
        {
            bool ret = true;

            if (effect.BattleCondition != null)
            {
                DanceBattleCondition danceCondition = effect.BattleCondition as DanceBattleCondition;
                NotEvadedBattleCondition notEvadedCondition = effect.BattleCondition as NotEvadedBattleCondition;

                if (danceCondition != null)
                {
                    ret = IsDanceConditionMet(danceCondition.EffectType, IsFighterHuman(moveExecutor));
                }
                else if (notEvadedCondition != null)
                {
                    ret = IsNotEvadedConditionMet(moveTarget);
                }
            }

            return ret;
        }

        protected bool IsDanceConditionMet(DanceEffectType effectType, bool isHumanTeamEffect)
        {
            List<DanceEffectCounter> danceCounters = Config.GetDanceEffects(isHumanTeamEffect);

            return danceCounters.Exists(dec => dec.Effect == effectType);
        }

        protected bool IsNotEvadedConditionMet(IFighter defender)
        {
            //note: there.... may be a case of a specific type of AutoEvadeStatus that would only evade 2 attacks per round or some such.
            //if that type of logic is ever executed, this logic will need to be updated to somehow check that this particular attack was evaded
            return !defender.Statuses.OfType<AutoEvadeStatus>().Any();
        }

        //TODO: This should be fired by an EventHandler that handles "OnAttackHit" event or similar
        protected void ExecuteOnSuccessfulAttackBattleMoveEffect(BattleMoveEffect effect, BattleMoveWithTarget battleMove)
        {
            IFighter owner = battleMove.Owner;

            if (!IsEffectConditionMet(effect, battleMove))
            {
                return;
            }

            RestorationBattleMoveEffect restorationEffect = effect as RestorationBattleMoveEffect;

            if (restorationEffect != null)
            { 
                //TODO: we do this calculation in a few places, maybe create a "CalculateHealAmount()" helper method
                double percentage = restorationEffect.Percentage / 100.0;

                int restoreAmount;

                switch (restorationEffect.RestorationType)
                {
                    case RestorationType.Health:
                        restoreAmount = (int)(owner.MaxHealth * percentage);
                        owner.Heal(restoreAmount);
                        break;
                    case RestorationType.Mana:
                        restoreAmount = (int) (owner.MaxMana*percentage);
                        owner.RestoreMana(restoreAmount);
                        break;
                }
            }
        }

        protected void ExecuteShieldMove(ShieldMove move, IFighter target)
        {
            target.SetBattleShield(move.Shield as BattleShield);
        }

        protected void ExecuteShieldFortifyingMove(ShieldFortifyingMove move, IFighter target)
        {
            BattleShield battleShield = target.BattleShield;

            if (battleShield == null)
            {
                _output.WriteLine($"But it failed because {target.DisplayName} did not have a battleShield equipped!");
                return;
            }

            int shieldFortifyingAmount = move.FortifyingAmount;

            switch (move.FortifyingType)
            {
                case ShieldFortifyingType.Defense:
                    battleShield.FortifyDefense(shieldFortifyingAmount);
                    break;
                case ShieldFortifyingType.Health:
                    battleShield.IncrementHealth(shieldFortifyingAmount);
                    break;
            }
        }

        protected void ExecuteShieldBusterMove(ShieldBusterMove move, IFighter moveExecutor, IFighter target)
        {
            SpecialMoveFailedEventArgs failedEventArgs;

            if (target.BattleShield == null)
            {
                _output.WriteLine($"But it failed because {target.DisplayName} did not have a battleShield equipped!");

                failedEventArgs = new SpecialMoveFailedEventArgs(moveExecutor, target, move, SpecialMoveFailedReasonType.TargetHadNoShield);
                moveExecutor.OnSpecialMoveFailed(failedEventArgs);

                return;
            }

            if (target.BattleShield.ShieldBusterDefense > move.ShieldBusterStrength)
            {
                _output.WriteLine("But the shield proved too powerful to be busted!");

                failedEventArgs = new SpecialMoveFailedEventArgs(moveExecutor, target, move, SpecialMoveFailedReasonType.ShieldBusterDefenseHigherThanShieldBusterPower);
                moveExecutor.OnSpecialMoveFailed(failedEventArgs);

                return;
            }

            target.BattleShield.OnShieldDestroyed(new ShieldDestroyedEventArgs());
            target.RemoveBattleShield();
        }

        protected List<FieldEffectCounter> FindEffects(FieldEffect effect, IFighter owner)
        {
            var isOwnerHuman = IsFighterHuman(owner);

            var ret = new List<FieldEffectCounter>();

            ret.AddRange(Config.FieldEffectCounters.Where(fec =>
            {
                var ret2 = fec.Effect.AreEqual(effect);

                switch (effect.TargetType)
                {
                    case TargetType.OwnTeam:
                        ret2 = ret2 && fec.IsHumanEffect == isOwnerHuman;
                        break;
                    case TargetType.EnemyTeam:
                        ret2 = ret2 && fec.IsHumanEffect != isOwnerHuman;
                        break;
                }

                return ret2;
            }));

            return ret;
        }

        protected IEnumerable<T> FindEffectsByType<T>(bool isHumanEffect) where T : FieldEffect
        {
            IEnumerable<FieldEffectCounter> countersWithCorrectEffectType =
                Config.FieldEffectCounters.Where(fec => fec.Effect is T && fec.IsHumanEffect == isHumanEffect);

            IEnumerable<FieldEffect> fieldEffects = countersWithCorrectEffectType.Select(fec => fec.Effect);

            return fieldEffects.Cast<T>();
        }

        protected void ImplementEffect(FieldEffect effect, int duration, params IFighter[] owners)
        {
            if (owners.Length == 0)
            {
                //TODO: this should be an error thrown here
            }

            OnFieldEffectExecuted(new FieldEffectExecutedEventArgs(effect, owners));

            if (effect.IsImmediatelyExecuted)
            {
                IFighter owner = owners[0];
                bool isOwnerHuman = _humanTeam.Contains(owner);
                Team targettedTeam = null;

                switch (effect.TargetType)
                {
                    case TargetType.EnemyTeam:
                        targettedTeam = isOwnerHuman ? _enemyTeam : _humanTeam;
                        break;
                    case TargetType.OwnTeam:
                        targettedTeam = isOwnerHuman ? _humanTeam : _enemyTeam;
                        break;
                }

                if (targettedTeam == null)
                {
                    throw new ArgumentException($"Battlemanager tried to implement field effect {effect}, but it had an invalid TargetType");
                }

                ExecuteFieldEffect(effect, targettedTeam, owners);
            }
            else
            {
                bool isOwnerHuman = IsFighterHuman(owners[0]);

                StatusFieldEffect statusEffect = effect as StatusFieldEffect;
                if (statusEffect != null)
                {
                    switch (effect.TargetType)
                    {
                        case TargetType.OwnTeam:
                            if (isOwnerHuman)
                            {
                                _humanTeam.AddStatus(statusEffect.Status);
                            }
                            else
                            {
                                _enemyTeam.AddStatus(statusEffect.Status);
                            }

                            break;
                        case TargetType.EnemyTeam:
                            if (isOwnerHuman)
                            {
                                _enemyTeam.AddStatus(statusEffect.Status);
                            }
                            else
                            {
                                //TODO: not currently setting status duration
                                //TODO: Should be an AddOrExtend() method for ensuring statuses stack when appropriate
                                _humanTeam.AddStatus(statusEffect.Status);
                            }

                            break;
                        case TargetType.Field:
                            _enemyTeam.AddStatus(statusEffect.Status);
                            _humanTeam.AddStatus(statusEffect.Status);
                            break;
                    }
                }
                else
                {
                    var find = FindEffects(effect, owners[0]);
                    var ownerList = owners.ToList();

                    if (find.Count > 0)
                    {
                        find.ForEach(fec => fec.SetTurnsLeft(duration));
                    }
                    else
                    {
                        switch (effect.TargetType)
                        {
                            case TargetType.OwnTeam:
                                Config.FieldEffectCounters.Add(new FieldEffectCounter(ownerList, duration, effect, isOwnerHuman));
                                break;
                            case TargetType.EnemyTeam:
                                Config.FieldEffectCounters.Add(new FieldEffectCounter(ownerList, duration, effect,
                                    !isOwnerHuman));
                                break;
                            case TargetType.Field:
                                Config.FieldEffectCounters.Add(new FieldEffectCounter(ownerList, duration, effect, true));
                                Config.FieldEffectCounters.Add(new FieldEffectCounter(ownerList, duration, effect, false));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        protected void BeforeBattle()
        {
            SetEvents();

            _humanTeam.InitializeForBattle(_enemyTeam, _input, _output);

            if (Config.ShowIntroAndOutroMessages)
            {
                _output.WriteLine("Looks like it's time for a battle!");

                foreach (var fighter in _enemyTeam.Fighters)
                {
                    _output.WriteLine("Encountered " + fighter.DisplayName + "!");
                }

                _input.WaitAndClear(_output);

                if (TerrainInteractables.Count > 0)
                {
                    foreach (TerrainInteractable interactable in TerrainInteractables)
                    {
                        _output.WriteLine(interactable.GetFullDisplayString());
                    }

                    _input.WaitAndClear(_output);
                }
            }

            List<IBossEnemy> bosses = GetBossEnemies();

            bosses.ForEach(boss =>
            {
                boss.PreBattleSetup(_enemyTeam, _humanTeam, _output, Config.SpecialBattleFlag);
                BattleMoveWithTarget zeroTurnMove = boss.GetZeroTurnMove(_enemyTeam, _humanTeam);
                ExecuteMove(zeroTurnMove);
                
                //TODO: the wait and clear should be part of the ExecuteMove() code
                _input.WaitAndClear(_output);
            });
        }

        protected List<IBossEnemy> GetBossEnemies()
        {
            List<IBossEnemy> bosses = _enemyTeam.Fighters.OfType<IBossEnemy>().ToList();

            //could be a HUman Controlled Boss Enemy, check for that
            if (!bosses.Any())
            {
                IEnumerable<HumanControlledEnemyFighter> humanControlledEnemies =
                    _enemyTeam.Fighters.OfType<HumanControlledEnemyFighter>().ToList();

                if (humanControlledEnemies.Any())
                {
                    bosses =
                        humanControlledEnemies.Where(hce => hce.Fighter is IBossEnemy)
                            .Select(hce => hce.Fighter as IBossEnemy)
                            .ToList();
                }
            }

            return bosses;
        }

        protected void SetEvents()
        {
            SetTeamEvents(_humanTeam);
            SetTeamEvents(_enemyTeam);
        }

        protected void SetTeamEvents(Team team)
        {
            if (team == _humanTeam)
            {
                team.TeamDefeated += OnHumanTeamDefeated;
                team.Ran += OnHumanTeamRan;

                if (Config != null && Config.SpecialBattleFlag == BattleConfigurationSpecialFlag.FirstBarbarianBattle)
                {
                    FailedShieldBusterCount = 0;
                    team.Fighters.ForEach(f => f.SpecialMoveFailed += RecordFailedShieldBusterEffect);
                }
            }
            else
            {
                team.TeamDefeated += OnEnemyTeamDefeated;
                team.Ran += OnEnemyTeamRan;
            }

            team.FighterAdded += OnFighterAdded;

            foreach (var fighter in team.Fighters)
            {
                SetFighterEvents(fighter);
            }
        }

        protected void SetFighterEvents(IFighter fighter)
        {
            fighter.Killed += OnFighterDefeated;
            fighter.Killed += PrintKilledMessage;
            fighter.MagicalDamageTaken += PrintMagicalDamageTakenMessage;
            fighter.DamageTaken += PrintPhysicalDamageMessage;
            fighter.Healed += PrintHealedMessage;
            fighter.CriticalAttack += PrintCriticalHitMessage;
            fighter.AttackMissed += PrintAttackMissedMessage;
            fighter.AutoEvaded += PrintAutoEvadedMessage;
            fighter.EnemyAttackCountered += PrintCounterAttackMessage;
            fighter.StatusAdded += PrintStatusAddedMessage;
            fighter.SpecialMoveExecuted += PrintSpecialMoveExecutedMessage;
            fighter.ShieldAdded += OnFighterShieldAdded;
            fighter.ShieldAdded += PrintShieldAddedMessage;
            fighter.StatRaised += PrintStatRaisedMessage;

            if (fighter.BattleShield != null)
            {
                BattleShield battleShield = fighter.BattleShield;

                battleShield.DamageTaken += PrintPhysicalDamageMessage;
                battleShield.ShieldDestroyed += PrintShieldDestroyedMessage;
                battleShield.ShieldDestroyed += OnShieldDestroyed;
            }

            HumanFighter humanFighter = fighter as HumanFighter;

            if(humanFighter != null)
            {
                humanFighter.ExpGained += OnExpGained;
                humanFighter.LeveledUp += OnLeveledUp;
                humanFighter.SpellsLearned += OnSpellsLearned;
            }

            Shade shadeFighter = fighter as Shade;

            if (shadeFighter != null)
            {
                shadeFighter.FighterSealed += OnFighterSealed;
                shadeFighter.ShadeAbsorbed += PrintShadeAbsorbedMessage;
                shadeFighter.ShadeTransformed += PrintFighterTransformedMessage;
            }
        }

        protected void PrintVictoryMessage()
        {
            if (Config.ShowIntroAndOutroMessages)
            {
                _output.WriteLine("And on this day, victory was won by you, the player!");
                _input.WaitAndClear(_output);
            }
        }

        protected void AfterBattle()
        {
            if (_enemyTeam.IsTeamDefeated())
            {
                Config.EndStatus = BattleEndStatus.Victory;
                PrintVictoryMessage();

                IEnumerable<HumanFighter> humanFighters = _humanTeam.GetHumanFighters();
                foreach (HumanFighter fighter in humanFighters)
                {
                    fighter.GainExp(Config.ExpGainedOnVictory);
                }
            }
            else if (_humanTeam.IsTeamDefeated())
            {
                Config.EndStatus = BattleEndStatus.Defeat;
                if (Config.ShowIntroAndOutroMessages)
                {
                    _output.WriteLine("what a tragedy on this day, to see the heros fall...");
                    _input.WaitAndClear(_output);
                }
            }
            else if (Config.EnemyTeamRan)
            {
                Config.EndStatus = BattleEndStatus.Victory;

                PrintVictoryMessage();
            }
            else
            {
                Config.EndStatus = BattleEndStatus.Ran;
                if (Config.ShowIntroAndOutroMessages)
                {
                    _output.WriteLine("Ran away safely!");
                    _input.WaitAndClear(_output);
                }
            }

            ClearEvents();
            Config.ClearAllCounters();

            OnBattleEnded(new BattleEndedEventArgs());
        }

        protected void OnRoundEnd()
        {
            var allDanceEffects = Config.GetAllDanceEffects();
            //TODO: this technically means all Field Effects kick in on Round End. But for some (e.g. restoreHealth), these should probably be implemented on Fighter's turn end
            Config.FieldEffectCounters.ForEach(ExecuteFieldEffect);
            allDanceEffects.ForEach(ExecuteDanceEffect);

            Config.FieldEffectCounters.RemoveAll(ets => ets.TurnsLeft == 0);
            Config.HumanDanceEffects.RemoveAll(hde => hde.TurnsLeft == 0);
            Config.EnemyDanceEffects.RemoveAll(ede => ede.TurnsLeft == 0);

            _humanTeam.OnRoundEnded(new RoundEndedEventArgs(_humanTeam));
            _enemyTeam.OnRoundEnded(new RoundEndedEventArgs(_enemyTeam));
        }

        protected void ClearEvents()
        {
            ClearTeamEvents(_humanTeam);
            ClearTeamEvents(_enemyTeam);
        }

        protected void ClearTeamEvents(Team team)
        {
            if (team == _humanTeam)
            {
                team.TeamDefeated -= OnHumanTeamDefeated;
            }
            else
            {
                team.TeamDefeated -= OnEnemyTeamDefeated;
            }
            team.Ran -= OnHumanTeamRan;

            foreach (var fighter in team.Fighters)
            {
                fighter.Killed -= OnFighterDefeated;
                fighter.Killed -= PrintKilledMessage;
                fighter.MagicalDamageTaken -= PrintMagicalDamageTakenMessage;
                fighter.DamageTaken -= PrintPhysicalDamageMessage;
                fighter.Healed -= PrintHealedMessage;
                fighter.CriticalAttack -= PrintCriticalHitMessage;
                fighter.AttackMissed -= PrintAttackMissedMessage;
                fighter.AutoEvaded -= PrintAutoEvadedMessage;
                fighter.EnemyAttackCountered -= PrintCounterAttackMessage;
                fighter.StatusAdded -= PrintStatusAddedMessage;
                fighter.SpecialMoveExecuted -= PrintSpecialMoveExecutedMessage;
                fighter.ShieldAdded -= OnFighterShieldAdded;
                fighter.ShieldAdded -= PrintShieldAddedMessage;
                fighter.StatRaised -= PrintStatRaisedMessage;
            }

            IEnumerable<HumanFighter> humanFighters = _humanTeam.GetHumanFighters();

            foreach (HumanFighter fighter in humanFighters)
            {
                fighter.ExpGained -= OnExpGained;
                fighter.LeveledUp -= OnLeveledUp;
                fighter.SpellsLearned -= OnSpellsLearned;
            }

            foreach (Shade shade in _humanTeam.Fighters.OfType<Shade>())
            {
                shade.FighterSealed -= OnFighterSealed;
            }
        }

        protected void ClearDebuffs(IFighter fighter)
        {
            var isHuman = IsFighterHuman(fighter);

            Config.FieldEffectCounters.RemoveAll(fec =>
            {
                var shouldRemove = fec.IsHumanEffect == isHuman;

                var statMultiplier = fec.Effect as StatMultiplierFieldEffect;
                if (statMultiplier != null)
                {
                    shouldRemove = shouldRemove && statMultiplier.Percentage < 1;
                }
                else
                {
                    shouldRemove = false;
                }

                return shouldRemove;
            });
        }

        protected void ClearDebuffs(Team team)
        {
            team.Fighters.ForEach(f =>
            {
                f.RemoveStatuses(s => s is StatMultiplierStatus, true);
            });
        }

        protected void SelectNewTarget(BattleMoveWithTarget move)
        {
            IFighter newTarget;
            IFighter owner = move.Owner;

            IEnumerable<IFighter> availableFighters = new List<IFighter>();

            bool isOwnerHuman = IsFighterHuman(owner);

            switch (move.Move.TargetType)
            {
                case TargetType.SingleEnemy:
                    availableFighters = isOwnerHuman ? _enemyTeam.Fighters : _humanTeam.Fighters;
                    break;
                case TargetType.SingleAlly:
                    availableFighters = isOwnerHuman ? _humanTeam.Fighters : _enemyTeam.Fighters;
                    availableFighters = availableFighters.Where(f => f != owner);
                    break;
                case TargetType.SingleAllyOrSelf:
                    availableFighters = isOwnerHuman ? _humanTeam.Fighters : _enemyTeam.Fighters;
                    break;
                
            }

            newTarget = availableFighters.FirstOrDefault(f => f.IsAlive());

            move.Retarget(newTarget);
        }

        protected bool DoesDefenderDodge(IFighter defender, AttackBattleMove attackMove, out bool shouldAlsoCounter)
        {
            List<AutoEvadeStatus> autoEvadeStatuses = defender.Statuses.OfType<AutoEvadeStatus>().ToList();

            shouldAlsoCounter = autoEvadeStatuses.Any(s => s.ShouldCounterAttack);

            bool shouldDodge = autoEvadeStatuses.Any();

            if (shouldDodge && attackMove.BattleMoveEffects.FirstOrDefault(e => e is CannotBeEvadedBattleMoveEffect) != null)
            {
                shouldDodge = false;
            }

            return shouldDodge;
        }

        protected bool DoesAttackHit(AttackBattleMove move, IFighter attacker, IFighter target)
        {
            if (move.BattleMoveEffects.OfType<NeverMissBattleMoveEffect>().Any(e => IsEffectConditionMet(e, attacker, target)))
            {
                return true;
            }

            var realAccuracy = ((double)move.Accuracy - target.Evade) / (100.0);

            if (attacker.Statuses.FirstOrDefault(s => s is BlindStatus) != null)
            {
                realAccuracy /= 3;
            }

            return _chanceService.EventOccurs(realAccuracy);
        }

        protected bool IsCriticalHit(AttackBattleMove move, IFighter attacker)
        {
            var critChance = ((double)move.CritChance + attacker.Luck) / 100.0;

            critChance *= CalculateCritMultiplier(attacker);

            return _chanceService.EventOccurs(critChance);
        }

        protected bool DoesDefenderCounter(IFighter defender)
        {
            return defender.Statuses.Any(s => s is CounterAttackStatus);
        }

        protected bool IsFighterHuman(IFighter fighter)
        {
            return _humanTeam.Contains(fighter);
        }

        #region event listeners

        protected void OnFighterDefeated(object sender, KilledEventArgs e)
        {
            Config.FieldEffectCounters.RemoveAll(fec => fec.Owners.Contains(sender));
            Config.HumanDanceEffects.RemoveAll(hde => hde.Owner == sender);
            Config.EnemyDanceEffects.RemoveAll(ede => ede.Owner == sender);

            EnemyFighter senderAsEnemyFighter = sender as EnemyFighter;

            if (senderAsEnemyFighter != null && _enemyTeam.Contains(senderAsEnemyFighter))
            {
                Config.ExpGainedOnVictory += senderAsEnemyFighter.ExpGivenOnDefeat;
            }
        }

        protected void OnFighterSealed(object sender, FighterSealedEventArgs e)
        {
            Config.FieldEffectCounters.RemoveAll(fec => fec.Owners.Contains(sender));
            Config.HumanDanceEffects.RemoveAll(hde => hde.Owner == sender);
            Config.EnemyDanceEffects.RemoveAll(ede => ede.Owner == sender);

            EnemyFighter senderAsEnemyFighter = sender as EnemyFighter;

            if (senderAsEnemyFighter != null && _enemyTeam.Contains(senderAsEnemyFighter))
            {
                Config.ExpGainedOnVictory += senderAsEnemyFighter.ExpGivenOnDefeat;
            }
        }

        protected void OnHumanTeamDefeated(object sender, TeamDefeatedEventArgs e)
        {
            Config.ContinueBattling = false;
        }

        protected void OnEnemyTeamDefeated(object sender, TeamDefeatedEventArgs e)
        {
            Config.ContinueBattling = false;
        }

        protected void OnHumanTeamRan(object sender, RanEventArgs e)
        {
            Config.HumanTeamRan = true;
            Config.ContinueBattling = false;
        }

        protected void OnEnemyTeamRan(object sender, RanEventArgs e)
        {
            Config.EnemyTeamRan = true;
            Config.ContinueBattling = false;
        }

        protected void OnEnemyDefeated(object sender, KilledEventArgs e)
        {
            var fighter = sender as EnemyFighter;
            if (fighter == null)
            {
                throw new InvalidOperationException(
                    $"BattleManager incorrectly assigned OnEnemyDefeated event to something that was not an EnemyFighter! sender: {sender}, sender.Type: {sender.GetType()}");
            }
            Config.ExpGainedOnVictory += fighter.ExpGivenOnDefeat;
        }

        protected void OnFighterAdded(object sender, FighterAddedEventArgs e)
        {
            SetFighterEvents(e.Fighter);
        }

        protected void OnExpGained(object sender, ExpGainedEventArgs e)
        {
            var fighter = sender as HumanFighter;
            if (fighter == null)
            {
                throw new InvalidOperationException(
                    $"BattleManager incorrectly assigned OnExpGained event to something that was not a HumanFighter! sender: {sender}, sender.Type: {sender.GetType()}");
            }

            string experiencePointOrPoints = "experience point" + ((e.AmountGained == 1) ? "!" : "s!");

            if (Config.ShowExpAndLevelUpMessages)
            {
                var output = $"{fighter.DisplayName} gained {e.AmountGained} {experiencePointOrPoints}";
                _output.WriteLine(output);
                _input.WaitAndClear(_output);
            }
        }

        protected void OnSpellsLearned(object sender, SpellsLearnedEventArgs e)
        {
            var fighter = sender as HumanFighter;
            if (fighter == null)
            {
                throw new InvalidOperationException(
                    $"BattleManager incorrectly assigned OnSpellLearned event to something that was not a HumanFighter! sender: {sender}, sender.Type: {sender.GetType()}");
            }

            List<Spell> spellsLearned = e.SpellsLearned;

            foreach (Spell spell in spellsLearned)
            {
                _output.WriteLine($"{fighter.DisplayName} learned the '{spell.Description}' spell!");
            }

            _input.WaitAndClear(_output);
        }

        protected void OnLeveledUp(object sender, LeveledUpEventArgs e)
        {
            var fighter = sender as HumanFighter;
            if (fighter == null)
            {
                throw new InvalidOperationException(
                    $"BattleManager incorrectly assigned OnLeveledUp event to something that was not a HumanFighter! sender: {sender}, sender.Type: {sender.GetType()}");
            }

            if (Config.ShowExpAndLevelUpMessages)
            {
                var output = $"{fighter.DisplayName} grew to level {e.NewLevel}!";
                _output.WriteLine(output);

                _output.WriteLine($"+{e.HealthBoost} Health!");
                _output.WriteLine($"+{e.ManaBoost} Mana!");
                _output.WriteLine($"+{e.StrengthBoost} Strength!");
                _output.WriteLine($"+{e.DefenseBoost} Defense!");
                _output.WriteLine($"+{e.SpeedBoost} Speed!");

                _input.WaitAndClear(_output);

                _output.WriteLine($"Max Health: {fighter.MaxHealth}");
                _output.WriteLine($"Max Mana: {fighter.MaxMana}");
                _output.WriteLine($"Strength: {fighter.Strength}");
                _output.WriteLine($"Defense: {fighter.Defense}");
                _output.WriteLine($"Speed: {fighter.Speed}");

                _input.WaitAndClear(_output);
            }
        }

        protected void OnFighterShieldAdded(object sender, ShieldAddedEventArgs e)
        {
            e.BattleShield.DamageTaken += PrintPhysicalDamageMessage;
            e.BattleShield.ShieldDestroyed += OnShieldDestroyed;
            e.BattleShield.ShieldDestroyed += PrintShieldDestroyedMessage;
        }

        protected void OnShieldDestroyed(object sender, ShieldDestroyedEventArgs e)
        {
            BattleShield senderAsShield = sender as BattleShield;

            if (senderAsShield != null)
            {
                senderAsShield.DamageTaken -= PrintPhysicalDamageMessage;
                senderAsShield.ShieldDestroyed -= OnShieldDestroyed;
            }
        }

        protected void PrintExecutedFieldEffect(object sender, FieldEffectExecutedEventArgs e)
        {
            FieldEffect effect = e.Effect;

            List<IFighter> owners = e.EffectOwners;

            if (owners.Count == 0)
            {
                throw new ArgumentException("BattleManager.OnFieldEffectImplemented() should be given an event args with at least one owner, but the list was empty!");
            }

            IFighter primaryOwner = e.EffectOwners[0];

            PrintFieldEffectImplementedMessage(primaryOwner, effect);
        }

        protected int FailedShieldBusterCount;

        protected void RecordFailedShieldBusterEffect(object sender, SpecialMoveFailedEventArgs e)
        {
            if (e.Move is ShieldBusterMove)
            {
                FailedShieldBusterCount++;

                if (FailedShieldBusterCount == 4)
                {
                    _output.WriteLine("The old gods have smiled upon your efforts!");
                    _output.WriteLine("\nIn a flash of light, your have been bestowed with a powerful gift.");
                    _output.Write("Unlocked the ");
                    _output.Write("Super shield buster", ConsoleColor.Cyan);
                    _output.WriteLine("!");

                    BattleMove superShieldBuster = MoveFactory.Get(BattleMoveType.ShieldBuster, "Super shield buster");

                    _humanTeam.Fighters.OfType<HumanFighter>().ToList().ForEach(f => f.AddMove(superShieldBuster));
                }
            }
        }

        #endregion
    }
}
