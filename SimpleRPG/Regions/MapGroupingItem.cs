namespace SimpleRPG.Regions
{
    public class MapGroupingItem<TArea, TAreaId> where TArea : Area<TAreaId>
    {
        public bool IsLocked { get; private set; }

        public TArea Item { get; }

        public MapGroupingItem(TArea item, bool isLocked = false)
        {
            Item = item;
            IsLocked = isLocked;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }
    }
}