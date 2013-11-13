using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public interface IGeocacheData
    {
        string Code { get; set; }
        string Name { get; set; }
        DateTime DataFromDate { get; set; }
        DateTime PublishedTime { get; set; }
        double Lat { get; set; }
        double Lon { get; set; }
        double? CustomLat { get; set; }
        double? CustomLon { get; set; }
        bool Available { get; set; }
        bool Archived { get; set; }
        string Country { get; set; }
        string State { get; set; }
        string Municipality { get; set; }
        string City { get; set; }
        GeocacheType GeocacheType { get; set; }
        string PlacedBy { get; set; }
        string Owner { get; set; }
        string OwnerId { get; set; }
        GeocacheContainer Container { get; set; }
        double Terrain { get; set; }
        double Difficulty { get; set; }
        string ShortDescription { get; set; }
        bool ShortDescriptionInHtml { get; set; }
        string LongDescription { get; set; }
        bool LongDescriptionInHtml { get; set; }
        string EncodedHints { get; set; }
        string Url { get; set; }
        bool MemberOnly { get; set; }
        List<int> AttributeIds { get; set; }
        int Favorites { get; set; }
        string PersonalNote { get; set; }
        string Notes { get; set; }
        bool Flagged { get; set; }
        bool Found { get; set; }
        bool Locked { get; set; }
    }
}
