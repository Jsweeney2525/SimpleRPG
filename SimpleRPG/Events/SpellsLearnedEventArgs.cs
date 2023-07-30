using System;
using System.Collections.Generic;
using SimpleRPG.Battle.Magic;

namespace SimpleRPG.Events
{
    public class SpellsLearnedEventArgs : EventArgs
    {
        public List<Spell> SpellsLearned { get; private set; }

        public SpellsLearnedEventArgs(params Spell[] spellsLearned)
        {
            SpellsLearned = new List<Spell>(spellsLearned);
        }

        public SpellsLearnedEventArgs(IEnumerable<Spell> spellsLearned)
        {
            SpellsLearned = new List<Spell>(spellsLearned);
        }
    }
}
