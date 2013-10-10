using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.APIGetC
{
    public class PresetSettings
    {
        public string Name { get; set; }

        public string Location { get; set; }
        public double Radius { get; set; }
        public bool Km { get; set; }
        public string Codes { get; set; }
        public string HiddenBy { get; set; }
        public string GeocacheName { get; set; }
        public double FavMin { get; set; }
        public double FavMax { get; set; }
        public double DifMin { get; set; }
        public double DifMax { get; set; }
        public double TerMin { get; set; }
        public double TerMax { get; set; }
        public double TrackMin { get; set; }
        public double TrackMax { get; set; }
        public int[] CacheTypes { get; set; }
        public int[] Containers { get; set; }
        public bool? ExclArchived { get; set; }
        public bool? ExclAvailable { get; set; }
        public bool? ExclPMO { get; set; }
        public string ExclFoundBy { get; set; }
        public string ExclHiddenBy { get; set; }
        public int LimitMaxTotal { get; set; }
        public int LimitMaxRequest { get; set; }
        public int LimitMaxLogs { get; set; }
        public bool BetweenPublishedDates { get; set; }
        public DateTime FromPublishedDate { get; set; }
        public DateTime ToPublishedDate { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
