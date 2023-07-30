using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Battle.FieldEffects
{
    public interface IHasOwnerFieldEffect
    {
        IFighter Owner { get; }

        void SetOwner(IFighter owner);
    }
}
