using System.Collections.Generic;
using System.Linq;

namespace SimpleRPG.Enums
{
    public enum MagicType
    {
        None,
        All,
        Fire,
        Water,
        Wind,
        Earth,
        Lightning,
        Ice
    }

    public class MagicTypes
    {
        public static IEnumerable<MagicType> GetBasicMagicTypes()
        {
            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();
            return magicTypes.Where(mt => mt != MagicType.All && mt != MagicType.None);
        }
    }
}