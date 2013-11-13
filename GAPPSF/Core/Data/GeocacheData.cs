using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class GeocacheData: IGeocacheData
    {
        public string Code { get; set; } 
        public string Name { get; set; } 
        public DateTime DataFromDate { get; set; } 
        public DateTime PublishedTime { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double? CustomLat { get; set; }
        public double? CustomLon { get; set; }
        public bool Available { get; set; }
        public bool Archived { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Municipality { get; set; }
        public string City { get; set; }
        public GeocacheType GeocacheType { get; set; }
        public string PlacedBy { get; set; }
        public string Owner { get; set; }
        public string OwnerId { get; set; }
        public GeocacheContainer Container { get; set; }
        public double Terrain { get; set; }
        public double Difficulty { get; set; }
        public string ShortDescription { get; set; }
        public bool ShortDescriptionInHtml { get; set; }
        public string LongDescription { get; set; }
        public bool LongDescriptionInHtml { get; set; }
        public string EncodedHints { get; set; }
        public string Url { get; set; }
        public bool MemberOnly { get; set; }
        public List<int> AttributeIds { get; set; }
        public int Favorites { get; set; }
        public string PersonalNote { get; set; }
        public string Notes { get; set; }
        public bool Flagged { get; set; }
        public bool Found { get; set; }
        public bool Locked { get; set; }

        public static void Copy(IGeocacheData src, IGeocacheData dest)
        {
            dest.Code = src.Code;
            dest.Name = src.Name;
            dest.DataFromDate = src.DataFromDate;
            dest.PublishedTime = src.PublishedTime;
            dest.Lat = src.Lat;
            dest.Lon = src.Lon;
            dest.CustomLat = src.CustomLat;
            dest.CustomLon = src.CustomLon;
            dest.Available = src.Available;
            dest.Archived = src.Archived;
            dest.Country = src.Country;
            dest.State = src.State;
            dest.Municipality = src.Municipality;
            dest.City = src.City;
            dest.GeocacheType = src.GeocacheType;
            dest.PlacedBy = src.PlacedBy;
            dest.Owner = src.Owner;
            dest.OwnerId = src.OwnerId;
            dest.Container = src.Container;
            dest.Terrain = src.Terrain;
            dest.Difficulty = src.Difficulty;
            dest.ShortDescription = src.ShortDescription;
            dest.ShortDescriptionInHtml = src.ShortDescriptionInHtml;
            dest.LongDescription = src.LongDescription;
            dest.LongDescriptionInHtml = src.LongDescriptionInHtml;
            dest.EncodedHints = src.EncodedHints;
            dest.Url = src.Url;
            dest.MemberOnly = src.MemberOnly;
            if (src.AttributeIds == null)
            {
                dest.AttributeIds = null;
            }
            else
            {
                dest.AttributeIds = (from a in src.AttributeIds select a).ToList();
            }
            dest.Favorites = src.Favorites;
            dest.PersonalNote = src.PersonalNote;
            dest.Notes = src.Notes;
            dest.Flagged = src.Flagged;
            dest.Found = src.Found;
            dest.Locked = src.Locked;
        }
    }
}
