namespace SimpleRPG.Regions
{
    /// <summary>
    /// Represents how to go from one Region to the next, or one SubRegion to the next.
    /// Because the player will only ever be in ONE Region and SubRegion, the "From" property can be a single option.
    /// The "To" property is a MapGrouping, however, since the player may have potentially many next areas
    /// </summary>
    /// <typeparam name="TArea"></typeparam>
    /// <typeparam name="TAreaId"></typeparam>
    public class MapPath<TArea, TAreaId> where TArea : Area<TAreaId>
    {
        public TArea From { get; }

        public MapGrouping<TArea, TAreaId> To { get; }

        public MapPath(TArea from, MapGrouping<TArea, TAreaId> to)
        {
            From = from;
            To = to;
        }

        public MapPath(TArea from, TArea to)
        {
            From = from;
            //TODO: how to get correct groupingID?
            To = new MapGrouping<TArea, TAreaId>(0, to);
        }

        public MapPath(TArea from)
        {
            From = from;
            //TODO: how to get correct groupingID?
            To = new MapGrouping<TArea, TAreaId>(0);
        }
    }
}