using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Magic
{
    public static class MagicRelationshipCalculator
    {
        public static MagicRelationshipType GetRelationship(MagicType attackingElement, MagicType defendingElement)
        {
            var ret = MagicRelationshipType.None;

            if (attackingElement == MagicType.Fire)
            {
                if (defendingElement == MagicType.Water)
                {
                    ret = MagicRelationshipType.Weak;
                }
                else if (defendingElement == MagicType.Wind)
                {
                    ret = MagicRelationshipType.Strong;
                }
            }
            else if (attackingElement == MagicType.Water)
            {
                if (defendingElement == MagicType.Earth)
                {
                    ret = MagicRelationshipType.Weak;
                }
                else if (defendingElement == MagicType.Fire)
                {
                    ret = MagicRelationshipType.Strong;
                }
            }
            else if (attackingElement == MagicType.Wind)
            {
                if (defendingElement == MagicType.Fire)
                {
                    ret = MagicRelationshipType.Weak;
                }
                else if (defendingElement == MagicType.Earth)
                {
                    ret = MagicRelationshipType.Strong;
                }
            }
            else if (attackingElement == MagicType.Earth)
            {
                if (defendingElement == MagicType.Wind)
                {
                    ret = MagicRelationshipType.Weak;
                }
                else if (defendingElement == MagicType.Water)
                {
                    ret = MagicRelationshipType.Strong;
                }
            }

            return ret;
        }
    }
}
