using System;
using System.Collections.Generic;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class CombinedFieldEffect
    {
        public List<FieldEffect> Effects { get; }

        public string Description { get; }

        public CombinedFieldEffect(string description, params FieldEffect[] effects)
        {
            Description = description;
            Effects = new List<FieldEffect>(effects);
        }
    }

    //TODO: most of this code is out of date, it's using the old FieldEffects rather than the statuses
    public class FieldEffectCombiner
    {
        public virtual CombinedFieldEffect Combine(DanceEffectType effect1, DanceEffectType effect2)
        {
            CombinedFieldEffect ret = null;
            string combinedName;
            var first = (DanceEffectType) Math.Min((int) effect1, (int) effect2);
            var second = (DanceEffectType) Math.Max((int) effect1, (int) effect2);

            switch (first)
            {
                case DanceEffectType.Fire:
                    switch (second)
                    {
                        case DanceEffectType.Wind:
                            combinedName = "lightning dance";
                            ret = new CombinedFieldEffect(combinedName,
                                new CriticalChanceMultiplierFieldEffect(TargetType.OwnTeam, combinedName, 2.0),
                                new CriticalChanceMultiplierFieldEffect(TargetType.EnemyTeam, combinedName, 0.5));
                            break;
                        case DanceEffectType.Soul:
                            combinedName = "courage dance";
                            ret = new CombinedFieldEffect(combinedName,
                                new StatMultiplierFieldEffect(TargetType.OwnTeam, combinedName, StatType.Strength, 1.5),
                                new MagicMultiplierFieldEffect(TargetType.OwnTeam, combinedName, MagicType.Fire, 1.25));
                            break;
                        case DanceEffectType.Heart:
                            combinedName = "passion tempo";
                            ret = new CombinedFieldEffect(combinedName,
                                new CriticalChanceMultiplierFieldEffect(TargetType.OwnTeam, combinedName, 3),
                                new SpellCostMultiplierFieldEffect(TargetType.OwnTeam, combinedName, 2),
                                new MagicMultiplierFieldEffect(TargetType.OwnTeam, combinedName, MagicType.All, 2));
                            break;
                        case DanceEffectType.Mind:
                            combinedName = "burning ballet";
                            ret = new CombinedFieldEffect(combinedName,
                                new SpellCostMultiplierFieldEffect(TargetType.OwnTeam, combinedName, 2),
                                new MagicMultiplierFieldEffect(TargetType.OwnTeam, combinedName, MagicType.All, 2));
                            break;
                        case DanceEffectType.Danger:
                            combinedName = "eruption festival";
                            ret = new CombinedFieldEffect(combinedName,
                                new MagicAttackFieldEffect(TargetType.EnemyTeam, combinedName, MagicType.Fire, 2, 1, true));
                            break;
                    }
                    break;
                case DanceEffectType.Water:
                    switch (second)
                    {
                        case DanceEffectType.Soul:
                            combinedName = "river rumba";
                            ret = new CombinedFieldEffect(combinedName,
                                new StatMultiplierFieldEffect(TargetType.OwnTeam, combinedName, StatType.Speed, 2));
                            break;
                        case DanceEffectType.Heart:
                            combinedName = "cleansing calypso";
                            ret = new CombinedFieldEffect(combinedName,
                                new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, combinedName, 25, immediatelyExecuted: true),
                                new UndoDebuffsFieldEffect(TargetType.OwnTeam, combinedName)
                                );
                            break;
                        case DanceEffectType.Mind:
                            combinedName = "clearsong conga";
                            ret = new CombinedFieldEffect(combinedName,
                                new RestoreManaPercentageFieldEffect(TargetType.OwnTeam, combinedName, 25, immediatelyExecuted: true));
                            break;
                        case DanceEffectType.Danger:
                            combinedName = "whirlpool fesitval";
                            ret = new CombinedFieldEffect(combinedName,
                                new MagicAttackFieldEffect(TargetType.EnemyTeam, combinedName, MagicType.Water, 2, 1, true));
                            break;
                    }
                    break;
                case DanceEffectType.Wind:
                    switch (second)
                    {
                        case DanceEffectType.Soul:
                            combinedName = "breeze form";
                            ret = new CombinedFieldEffect(combinedName,
                                new StatMultiplierFieldEffect(TargetType.OwnTeam, combinedName, StatType.Evade, 1.4));
                            break;
                        case DanceEffectType.Heart:
                            combinedName = "carefree waltz";
                            ret = new CombinedFieldEffect(combinedName,
                                new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, combinedName, 10));
                            break;
                        case DanceEffectType.Mind:
                            combinedName = "gusty mamba";
                            ret = new CombinedFieldEffect(combinedName,
                                new ReflectFieldEffect(TargetType.OwnTeam, combinedName, MagicType.All));
                            break;
                        case DanceEffectType.Danger:
                            combinedName = "tornado festival";
                            ret = new CombinedFieldEffect(combinedName,
                                new MagicAttackFieldEffect(TargetType.EnemyTeam, combinedName, MagicType.Wind, 2, 1, true));
                            break;
                    }
                    break;
                case DanceEffectType.Earth:
                    switch (second)
                    {
                        case DanceEffectType.Soul:
                            combinedName = "desert swing";
                            ret = new CombinedFieldEffect(combinedName,
                                new StatMultiplierFieldEffect(TargetType.OwnTeam, combinedName, StatType.Defense, 2));
                            break;
                        case DanceEffectType.Heart:
                            combinedName = "steadfast stance";
                            var earthShield = new ElementalBattleShield(5, 0, 0, MagicType.Earth);
                            ret = new CombinedFieldEffect(combinedName,
                                new ShieldFieldEffect(TargetType.OwnTeam, combinedName, earthShield));
                            break;
                        case DanceEffectType.Mind:
                            combinedName = "gaia galla";
                            ret = new CombinedFieldEffect(combinedName,
                                new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, combinedName, 10));
                            break;
                        case DanceEffectType.Danger:
                            combinedName = "earthquake fesitval";
                            ret = new CombinedFieldEffect(combinedName,
                                new MagicAttackFieldEffect(TargetType.EnemyTeam, combinedName, MagicType.Earth, 2, 1, true));
                            break;
                    }
                    break;
            }

            return ret;
        }
    }
}
