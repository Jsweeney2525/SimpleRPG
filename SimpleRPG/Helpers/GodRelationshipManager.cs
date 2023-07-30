using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;

namespace SimpleRPG.Helpers
{

    public class GodRelationshipManager
    {
        private class FighterGodRelationship
        {
            public HumanFighter Fighter { get; }

            private readonly Dictionary<GodEnum, int> _values;

            public FighterGodRelationship(HumanFighter fighter)
            {
                Fighter = fighter;
                _values = new Dictionary<GodEnum, int>();

                IEnumerable<GodEnum> godValues = EnumHelperMethods.GetAllValuesForEnum<GodEnum>();

                foreach (GodEnum god in godValues)
                {
                    _values[god] = 0;
                }
            }

            public int GetRelationshipValue(GodEnum god)
            {
                return _values[god];
            }

            public void UpdateRelationshipValue(GodEnum god, int value)
            {
                _values[god] += value;
            }
        }

        private List<FighterGodRelationship> FighterGodRelationships { get; } = new List<FighterGodRelationship>();

        public void InitializeForFighter(HumanFighter fighter)
        {
            if (!FighterGodRelationships.Exists(fgr => fgr.Fighter == fighter))
            {
                FighterGodRelationships.Add(new FighterGodRelationship(fighter));   
            }
        }

        public int GetFighterRelationshipValue(HumanFighter fighter, GodEnum god)
        {
            FighterGodRelationship relationship = GetRelationship(fighter);

            return relationship.GetRelationshipValue(god);
        }

        public void UpdateRelationship(HumanFighter fighter, GodEnum god, int value)
        {
            FighterGodRelationship relationship = GetRelationship(fighter);

            relationship.UpdateRelationshipValue(god, value);
        }

        private FighterGodRelationship GetRelationship(IFighter fighter, bool throwIfNotFound = true)
        {
            FighterGodRelationship relationship = FighterGodRelationships.FirstOrDefault(fgr => fgr.Fighter == fighter);

            if (relationship == null)
            {
                throw new KeyNotFoundException($"GodRelationshipManager has not been initialized for fighter {fighter.DisplayName}");
            }

            return relationship;
        }
    }
}
