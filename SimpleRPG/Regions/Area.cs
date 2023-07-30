using SimpleRPG.Screens;

namespace SimpleRPG.Regions
{
    public abstract class Area<T>
    {
        public T AreaId { get; }

        /// <summary>
        /// the string to be printed upon entering the region.
        /// </summary>
        public ColorString RegionIntro { get; protected set; }

        protected Area(T areaId, ColorString regionIntro = null)
        {
            AreaId = areaId;
            RegionIntro = regionIntro;
        }
    }
}
