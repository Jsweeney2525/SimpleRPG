using System;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Battle.BattleManager
{
    public partial class BattleManager
    {
        protected void PrintAttackMissedMessage(object sender, AttackMissedEventArgs e)
        {
            if (Config.ShowAttackMessages)
            {
                _output.WriteLine("But it missed!");
            }
        }

        protected void PrintAutoEvadedMessage(object sender, AutoEvadedEventArgs e)
        {
            string displayName = e.Evader.DisplayName;
            string counterText = e.AlsoCountered ? " and countered" : "";

            string output = $"{displayName} evaded the attack{counterText}!";
            _output.WriteLine(output);
        }

        protected void PrintCounterAttackMessage(object sender, EnemyAttackCounteredEventArgs e)
        {
            _output.WriteLine($"{e.Counterer.DisplayName} counter attacks!");
        }

        protected void PrintCriticalHitMessage(object sender, CriticalAttackEventArgs e)
        {
            if (Config.ShowAttackMessages)
            {
                _output.WriteLine("Critical hit!");
            }
        }

        protected void PrintPhysicalDamageMessage(object sender, PhysicalDamageTakenEventArgs e)
        {
            int damage = e.Damage;
            if (Config.ShowPhysicalDamageMessages)
            {
                IBattleShield senderAsShield = sender as IBattleShield;

                string output;

                if (senderAsShield != null)
                {
                    output = $"{senderAsShield.Owner.DisplayName}'s {senderAsShield.GetDisplayText(false)} took {damage} damage!";
                }
                else
                {
                    output = $"It did {damage} damage!";
                }

                _output.WriteLine(output);
            }
        }

        protected void PrintHealedMessage(object sender, FighterHealedEventArgs e)
        {
            IFighter senderAsFighter = sender as IFighter;

            if (senderAsFighter == null)
            {
                throw new ArgumentException("BattleManagerScreenOutputs.PrintHealedMessage should subscribe to IFighters and nothing else");
            }

            var output = e.IsFullHeal ? 
                $"{senderAsFighter.DisplayName}'s HP was fully restored!" : 
                $"{senderAsFighter.DisplayName} was healed for {e.HealAmount} HP!";

            _output.WriteLine(output);
        }

        protected void PrintMagicalDamageTakenMessage(object sender, MagicalDamageTakenEventArgs e)
        {
            if (!Config.ShowMagicalDamageMessages)
            {
                return;
            }

            int damageTaken = e.Damage;
            IFighter senderAsFighter = sender as IFighter;

            if (senderAsFighter == null)
            {
                throw new ArgumentException("Something other than an IFighter fired a 'magicalDamageTaken' event!");
            }

            string output = $"{senderAsFighter.DisplayName} took {damageTaken} damage!";

            _output.WriteLine(output);
        }

        protected void PrintKilledMessage(object sender, KilledEventArgs e)
        {
            if (Config.ShowDeathMessages)
            {
                IFighter senderAsFighter = sender as IFighter;

                if (senderAsFighter == null)
                {
                    throw new InvalidOperationException("PrintKilledMessage subscribed to something that wasn't an IFighter");
                }

                _output.WriteLine($"{senderAsFighter.DisplayName} has been defeated!");
            }
        }

        protected void PrintSpecialMoveExecutedMessage(object sender, SpecialMoveExecutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Move.ExecutionText))
            {
                IFighter target = e.MoveTarget;
                string executionText = string.Copy(e.Move.ExecutionText);
                executionText = executionText.Replace(Globals.TargetReplaceText, target.DisplayName);
                _output.WriteLine($"{e.Executor.DisplayName} {executionText}!");
            }
        }

        protected void PrintShieldAddedMessage(object sender, ShieldAddedEventArgs e)
        {
            if (Config.ShowShieldAddedMessage)
            {
                IFighter senderAsFighter = sender as IFighter;

                if (senderAsFighter == null)
                {
                    throw new InvalidOperationException("Battlemanager.PrintShieldAddedMessage() subscribed to something that was not an IFighter, that's odd");
                }

                string shieldDisplayText = e.BattleShield.GetDisplayText();
                string output = $"{senderAsFighter.DisplayName} was equipped with {shieldDisplayText}!";
                _output.WriteLine(output);
            }
        }

        protected void PrintShieldDestroyedMessage(object sender, ShieldDestroyedEventArgs e)
        {
            BattleShield senderAsShield = sender as BattleShield;

            if (senderAsShield == null)
            {
                throw new InvalidOperationException($"Something other than a BattleShield fired an ShieldDestroyed event. sender: {sender}, typeof sender: {sender.GetType()}");
            }

            _output.WriteLine($"{senderAsShield.Owner.DisplayName}'s shield was destroyed!");
        }

        protected void PrintStatRaisedMessage(object sender, StatRaisedEventArgs e)
        {
            IFighter senderAsFighter = sender as IFighter;

            if (senderAsFighter == null)
            {
                throw new ArgumentException("BattleManagerScreenOutputs.PrintStatRaisedMessage should subscribe to IFighters and nothing else");
            }

            StatType raisedStat = e.RaisedStat;

            string statTypeString = raisedStat == StatType.Evade ? "evasion" : raisedStat.ToString().ToLower();

            string expectedMessage = $"{senderAsFighter.DisplayName}'s {statTypeString} was raised by {e.BoostAmount}!";
            _output.WriteLine(expectedMessage);
        }

        protected void PrintFighterTransformedMessage(object sender, FighterTransformedEventArgs e)
        {
            _output.WriteLine($"{e.PreTransformDisplayName} has transformed to become {e.PostTransformDisplayName}!");
        }

        #region status print functions

        protected string GetDurationString(Status status)
        {
            string turnOrTurns = status.TurnCounter == 1 ? "turn" : "turns";
            return $"{status.TurnCounter} {turnOrTurns}";
        }

        protected void PrintStatusAddedMessage(IFighter fighter, CriticalChanceMultiplierStatus status)
        {
            string increaseOrDecrease = (status.Multiplier > 1.0) ? "an increased" : "a decreased";

            string output = $"{fighter.DisplayName} has {increaseOrDecrease} chance for scoring critical hits for {GetDurationString(status)}!";

            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, MagicMultiplierStatus status)
        {
            string strengthenedOrWeakened = (status.Multiplier > 1.0) ? "strengthened" : "weakened";
            MagicType magicType = status.MagicType;

            string output = $"{fighter.DisplayName} has {strengthenedOrWeakened}";

            if (magicType != MagicType.All)
            {
                string magicTypeString = magicType.ToString().ToLower();
                output = $"{output} {magicTypeString}";
            }

            output = $"{output} magic for {GetDurationString(status)}!";

            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, MagicResistanceMultiplierStatus status)
        {
            string strengthenedOrWeakened = (status.Multiplier > 1.0) ? "strengthened" : "weakened";
            MagicType magicType = status.MagicType;

            string output = $"{fighter.DisplayName} has {strengthenedOrWeakened}";

            if (magicType != MagicType.All)
            {
                string magicTypeString = magicType.ToString().ToLower();
                output = $"{output} {magicTypeString}";
            }

            output = $"{output} resistance for {GetDurationString(status)}!";
            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, ReflectStatus status)
        {
            MagicType reflectType = status.MagicType;

            string output = $"{fighter.DisplayName} has gained reflect status";

            if (reflectType != MagicType.All)
            {
                output = $"{output} against {reflectType.ToString().ToLower()} magic";
            }

            output = $"{output} for {GetDurationString(status)}!";

            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, SpellCostMultiplierStatus status)
        {
            string increaseOrDecrease = status.Multiplier < 1 ? "decreased" : "increased";
            string output = $"{fighter.DisplayName} has {increaseOrDecrease} cost for magic spells for {GetDurationString(status)}!";
            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, StatMultiplierStatus status)
        {
            StatType stat = status.StatType;
            string increaseOrDecrease = (status.Multiplier > 1.0) ? "raised" : "lowered";

            string output = $"{fighter.DisplayName} had their {stat.ToString().ToLower()} {increaseOrDecrease} for {GetDurationString(status)}!";
            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, AutoEvadeStatus status)
        {
            string andCounterString = status.ShouldCounterAttack ? " and counter" : "";
            string output = $"{fighter.DisplayName} will evade{andCounterString} all attacks for {GetDurationString(status)}!";
            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, CounterAttackStatus status)
        {
            string output = $"{fighter.DisplayName} will counter any attack for {GetDurationString(status)}!";
            _output.WriteLine(output);
        }

        protected void PrintStatusAddedMessage(IFighter fighter, BlindStatus status)
        {
            _output.WriteLine($"{fighter.DisplayName} has been afflicted with blindness for {GetDurationString(status)}!");
        }

        protected void PrintStatusAddedMessage(object sender, StatusAddedEventArgs e)
        {
            Status status = e.Status;

            IFighter senderAsFighter = sender as IFighter;

            if (senderAsFighter == null)
            {
                throw new InvalidOperationException("BattleManager.PrintStatusAddedMessage() should only subscribe to instances of IFighter");
            }

            CriticalChanceMultiplierStatus criticalChanceStatus = status as CriticalChanceMultiplierStatus;
            StatMultiplierStatus statMultiplierStatus = status as StatMultiplierStatus;
            MagicMultiplierStatus magicMultiplierStatus = status as MagicMultiplierStatus;
            MagicResistanceMultiplierStatus resistanceMultiplierStatus = status as MagicResistanceMultiplierStatus;
            ReflectStatus reflectStatus = status as ReflectStatus;
            SpellCostMultiplierStatus spellCostMultiplierStatus = status as SpellCostMultiplierStatus;
            AutoEvadeStatus autoEvadeStatus = status as AutoEvadeStatus;
            CounterAttackStatus counterAttackStatus = status as CounterAttackStatus;
            BlindStatus blindStatus = status as BlindStatus;
            
            if (criticalChanceStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, criticalChanceStatus);
            }
            if (statMultiplierStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, statMultiplierStatus);
            }
            if (magicMultiplierStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, magicMultiplierStatus);
            }
            if (resistanceMultiplierStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, resistanceMultiplierStatus);
            }
            if (reflectStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, reflectStatus);
            }
            if (spellCostMultiplierStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, spellCostMultiplierStatus);
            }
            if (autoEvadeStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, autoEvadeStatus);
            }
            if (counterAttackStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, counterAttackStatus);
            }
            if (blindStatus != null)
            {
                PrintStatusAddedMessage(senderAsFighter, blindStatus);
            }
        }

        #endregion

        #region field effect print functions

        protected string GetFieldEffectTeamString(IFighter effectExecutor, FieldEffect effect)
        {
            bool isOwnerHuman = _humanTeam.Contains(effectExecutor);
            string ret = "";

            switch (effect.TargetType)
            {
                case TargetType.EnemyTeam:
                    ret = isOwnerHuman ? "Enemy team" : "Your team";
                    break;
                case TargetType.OwnTeam:
                    ret = isOwnerHuman ? "Your team" : "Enemy team";
                    break;
                case TargetType.Field:
                    throw new NotImplementedException("GetFieldEffectTeamString method has not yet been coded to handle TargetType.Field");
            }

            return ret;
        }

        protected string GetDurationString(FieldEffect effect)
        {
            string turnOrTurns = effect.EffectDuration == 1 ? "turn" : "turns";
            return $"{effect.EffectDuration} {turnOrTurns}";
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, MagicAttackFieldEffect effect)
        {
            string teamString = GetFieldEffectTeamString(effectExecutor, effect);

            string output = $"{teamString} has been attacked with {effect.MagicType.ToString().ToLower()}!";

            _output.WriteLine(output);
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, ShieldFieldEffect effect)
        {
            string teamString = GetFieldEffectTeamString(effectExecutor, effect);

            IBattleShield battleShield = effect.BattleShield;
            ElementalBattleShield elementalShield = battleShield as ElementalBattleShield;

            string shieldTypeString = "";

            if (elementalShield != null)
            {
                shieldTypeString = $"{elementalShield.ElementalType.ToString().ToLower()} elemental shields";
            }

            string output = $"{teamString} has gained {shieldTypeString}!";

            _output.WriteLine(output);
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, UndoDebuffsFieldEffect effect)
        {
            string teamString = GetFieldEffectTeamString(effectExecutor, effect);

            string output = $"{teamString} has had all stat changes removed!";

            _output.WriteLine(output);
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, RestoreHealthPercentageFieldEffect effect)
        {
            string teamString = GetFieldEffectTeamString(effectExecutor, effect);

            string output = effect.IsImmediatelyExecuted ? 
                $"{teamString} has had health restored!" :
                $"{teamString} has gained regen status for {GetDurationString(effect)}!";

            _output.WriteLine(output);
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, RestoreManaPercentageFieldEffect effect)
        {
            string teamString = GetFieldEffectTeamString(effectExecutor, effect);
            string output;

            output = effect.IsImmediatelyExecuted ? 
                $"{teamString} has had mana restored!" : 
                $"{teamString} has gained mana regen status for {GetDurationString(effect)}!";

            _output.WriteLine(output);
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, StatMultiplierFieldEffect effect)
        {
            string teamString = GetFieldEffectTeamString(effectExecutor, effect);
            string statString = effect.Stat.ToString().ToLower();
            double percentage = effect.Percentage;
            string raisedOrLowered = percentage < 1 ? "lowered" : "raised";
            int effectDuration = effect.EffectDuration ?? 0;
            string turnOrTurns = effectDuration == 1 ? "turn" : "turns";

            string output = $"{teamString} has had its {statString} {raisedOrLowered} for {effectDuration} {turnOrTurns}";

            _output.WriteLine(output);
        }

        protected void PrintFieldEffectImplementedMessage(IFighter effectExecutor, FieldEffect effect)
        {
            MagicAttackFieldEffect magicAttackFieldEffect = effect as MagicAttackFieldEffect;
            ShieldFieldEffect shieldFieldEffect = effect as ShieldFieldEffect;
            UndoDebuffsFieldEffect undoDebuffsFieldEffect = effect as UndoDebuffsFieldEffect;
            RestoreHealthPercentageFieldEffect restoreHealthEffect = effect as RestoreHealthPercentageFieldEffect;
            RestoreManaPercentageFieldEffect restoreManaEffect = effect as RestoreManaPercentageFieldEffect;
            StatMultiplierFieldEffect statMultiplierFieldEffect = effect as StatMultiplierFieldEffect;
            SpellCostMultiplierFieldEffect costMultiplierEffect = effect as SpellCostMultiplierFieldEffect;
            CriticalChanceMultiplierFieldEffect critChanceEffect = effect as CriticalChanceMultiplierFieldEffect;
            MagicMultiplierFieldEffect magicMultiplierEffect = effect as MagicMultiplierFieldEffect;
            ReflectFieldEffect magicReflectEffect = effect as ReflectFieldEffect;
            StatusFieldEffect statusFieldEffect = effect as StatusFieldEffect;

            if (magicAttackFieldEffect != null)
            {
                PrintFieldEffectImplementedMessage(effectExecutor, magicAttackFieldEffect);
            }
            else if (shieldFieldEffect != null)
            {
                PrintFieldEffectImplementedMessage(effectExecutor, shieldFieldEffect);
            }
            else if (undoDebuffsFieldEffect != null)
            {
                PrintFieldEffectImplementedMessage(effectExecutor, undoDebuffsFieldEffect);
            }
            else if (restoreHealthEffect != null)
            {
                PrintFieldEffectImplementedMessage(effectExecutor, restoreHealthEffect);
            }
            else if (statMultiplierFieldEffect != null)
            {
                PrintFieldEffectImplementedMessage(effectExecutor, statMultiplierFieldEffect);
            }
            else if (restoreManaEffect != null)
            {
                PrintFieldEffectImplementedMessage(effectExecutor, restoreManaEffect);
            }
            else if (critChanceEffect != null)
            {
                throw new NotImplementedException("this didn't get an updated PrintFieldEffectImplementedMessage when the code was moved over");
                /*if (critChanceEffect.Percentage < 1.0)
                {
                    output += "a decreased ";
                }
                else
                {
                    output += "an increased ";
                }

                output += "chance for critical hits";*/
            }
            else if (magicMultiplierEffect != null)
            {
                throw new NotImplementedException("this didn't get an updated PrintFieldEffectImplementedMessage when the code was moved over");
                /*if (magicMultiplierEffect.Percentage < 1.0)
                {
                    output += "weakened ";
                }
                else
                {
                    output += "strengthened ";
                }

                if (magicMultiplierEffect.MagicType != MagicType.All)
                {
                    output += $"{magicMultiplierEffect.MagicType.ToString().ToLower()} ";
                }
                output += "magic";*/
            }
            else if (costMultiplierEffect != null)
            {
                throw new NotImplementedException("this didn't get an updated PrintFieldEffectImplementedMessage when the code was moved over");
                /*output += (costMultiplierEffect.Multiplier < 1.0) ? "decreased " : "increased ";
                output += "cost for magic spells";*/
            }
            else if (magicReflectEffect != null)
            {
                throw new NotImplementedException("this didn't get an updated PrintFieldEffectImplementedMessage when the code was moved over");
                /*var magicTypeString = magicReflectEffect.MagicType.ToString().ToLower();
                output += $"gained reflect status against {magicTypeString} magic";*/
            }
            else if (statusFieldEffect != null)
            {
                //Don't need StatusFieldEffect print message- the statuses themselves have their own print methods
            }
            else
            {
                throw new ArgumentException(
                    "It's time to implement a new version of BattleManager.PrintFieldEffectImplementedMessage()");
            }
        }

        #endregion field effect print functions

        #region Shade messages

        protected void PrintShadeAbsorbedMessage(object sender, ShadeAbsorbedEventArgs e)
        {
            Shade senderAsShade = sender as Shade;

            if (senderAsShade == null)
            {
                throw new InvalidCastException("BattleManagerScreenoutputs.PrintShadeAbsorbedMessage was expecting the sender argument to be castable to type Shade");
            }
            _output.WriteLine($"{e.AbsorbedShade.DisplayName}'s essence was absorbed by {senderAsShade.DisplayName}!");
        }

        #endregion
    }
}
