using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Enums;

namespace SimpleRPG.Regions
{
    /// <summary>
    /// Represents a collection of <see cref="MapGroupingItem{TArea, TAreaId}"/>, used to represent all areas that come off of a particular Region or SubRegion
    /// </summary>
    /// <typeparam name="TArea">Should only ever be <see cref="Region"/> or <seealso cref="SubRegion"/></typeparam>
    /// <typeparam name="TAreaId">Should only ever be <see cref="WorldRegion"/> or <seealso cref="WorldSubRegion"/></typeparam>
    public class MapGrouping<TArea, TAreaId> where TArea : Area<TAreaId>
    {
        public int GroupingId { get; }

        public AreaMap<TArea, TAreaId> Parent { get; private set; }

        public List<MapGroupingItem<TArea, TAreaId>> Values { get; }

        public MapGrouping(int groupingId, params TArea[] values)
        {
            GroupingId = groupingId;
            Values = new List<MapGroupingItem<TArea, TAreaId>>();

            foreach (TArea value in values)
            {
                Values.Add(new MapGroupingItem<TArea, TAreaId>(value));
            }
        }

        public void SetParent(AreaMap<TArea, TAreaId> parent)
        {
            if (Parent != null)
            {
                throw new InvalidOperationException("A mapGrouping's parent should only be set once!");
            }

            Parent = parent;
        }

        public IEnumerable<TArea> GetAvaialableAreas()
        {
            return Values.Where(v => !v.IsLocked).Select(v => v.Item);
        }

        public void Lock(Func<TArea, bool> lockSelector)
        {
            IEnumerable<MapGroupingItem<TArea, TAreaId>> selectedItems = Values.Where(v => lockSelector(v.Item));

            foreach (MapGroupingItem<TArea, TAreaId> selectedItem in selectedItems)
            {
                selectedItem.Lock();
            }
        }

        public void Unlock(Func<TArea, bool> unlockSelector)
        {
            IEnumerable<MapGroupingItem<TArea, TAreaId>> selectedItems = Values.Where(v => unlockSelector(v.Item));

            foreach (MapGroupingItem<TArea, TAreaId> selectedItem in selectedItems)
            {
                selectedItem.Unlock();
            }
        }
    }
}