using System;
using System.Collections.Generic;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Magic
{
    /// <summary>
    /// A class that groups a set of values where each value corresponds to a <see cref="MagicType"/>
    /// </summary>
    /// <typeparam name="T">The type of the values to be paired with the Magic Type</typeparam>
    public class MagicSet<T>
    {
        private Dictionary<MagicType, T> _values;

        public MagicSet()
        {
            _values = new Dictionary<MagicType, T>();

            var magicTypes = Enum.GetValues(typeof(MagicType));

            var tType = typeof(T);
            foreach (MagicType magicType in magicTypes)
            {
                _values[magicType] = (T)(tType.IsValueType ? Activator.CreateInstance(tType) : null);
            }
        }

        public T this[MagicType index]    // Indexer declaration
        {
            get { return _values[index]; }
            set { _values[index] = value; }
        }
    }
}
