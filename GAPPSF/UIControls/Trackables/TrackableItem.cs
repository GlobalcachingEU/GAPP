using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.Trackables
{
    public class TrackableItem
    {
        public bool? AllowedToBeCollected { get; set; }
        public bool Archived { get; set; }
        public long BugTypeID { get; set; }
        public string Code { get; set; }
        public string CurrentGeocacheCode { get; set; }
        public string CurrentGoal { get; set; }
        public DateTime DateCreated { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public long Id { get; set; }
        public bool InCollection { get; set; }
        public string Name { get; set; }
        public string TBTypeName { get; set; }
        public string Url { get; set; }
        public long WptTypeID { get; set; }
        public string Owner { get; set; }

        public byte[] IconData { get; set; }

        //calculated
        public int HopCount { get; set; }
        public int DiscoverCount { get; set; }
        public int InCacheCount { get; set; }
        public double DistanceKm { get; set; }
        public double? Lon { get; set; }
        public double? Lat { get; set; }
    }
}
