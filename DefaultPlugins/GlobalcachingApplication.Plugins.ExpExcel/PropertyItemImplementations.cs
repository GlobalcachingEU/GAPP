using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.ExpExcel
{
    public class PropertyName
    {
        public const string STR_NAME = "Name";
        public const string STR_CODE = "Code";
        public const string STR_PUBLISHED = "Published";
        public const string STR_LAT = "Latitude";
        public const string STR_LON = "Longitude";
        public const string STR_COORDINATE = "Coordinate";
        public const string STR_YES = "Yes";
        public const string STR_NO = "No";
        public const string STR_AVAILABLE = "Available";
        public const string STR_ARCHIVED = "Archived";
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_MUNICIPALITY = "Municipality";
        public const string STR_CITY = "City";
        public const string STR_TYPE = "Type";
        public const string STR_PLACEDBY = "Placed by";
        public const string STR_OWNER = "Owner";
        public const string STR_CONTAINER = "Container";
        public const string STR_TERRAIN = "Terrain";
        public const string STR_DIFFICULTY = "Difficulty";
        public const string STR_DESCRIPTIONTEXT = "Description Text";
        public const string STR_DESCRIPTIONHTML = "Description HTML";
        public const string STR_URL = "Url";
        public const string STR_MEMBERONLY = "Member Only";
        public const string STR_CUSTOMLAT = "Custom Latitude";
        public const string STR_CUSTOMLON = "Custom Longitude";
        public const string STR_FAVORITES = "Favorites";
        public const string STR_PERSONALNOTES = "Personal notes";
        public const string STR_FLAGGED = "Flagged";
        public const string STR_FOUND = "Found";
        public const string STR_FOUNDDATE = "Found date";
        public const string STR_HINTS = "Hints";
        public const string STR_CUSTOMCOORD = "Custom coord.";
        public const string STR_AUTOCOORD = "Auto coord.";
        public const string STR_GCVOTE = "GCVote";
        public const string STR_RDX = "RD x";
        public const string STR_RDY = "RD y";
        public const string STR_ENVELOPEAREA_OTHER = "In region envelop - other";
        public const string STR_INAREA_OTHER = "In region - other";
        public const string STR_GLOBALCACHINGURL = "Globalcaching URL";
    }

    public class PropertyItemGlobalcachingUrl : PropertyItem
    {
        public PropertyItemGlobalcachingUrl(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_GLOBALCACHINGURL)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return string.Concat("https://www.4geocaching.eu/gmap/DefaultV3.aspx?wp={0}", gc.Code);
        }
    }

    public class PropertyItemEnvelopAreaOther : PropertyItem
    {
        public PropertyItemEnvelopAreaOther(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_ENVELOPEAREA_OTHER)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            StringBuilder result = new StringBuilder();
            List<Framework.Data.AreaInfo> al = Utils.GeometrySupport.Instance.GetEnvelopAreasOfLocation(new Framework.Data.Location(gc.Lat, gc.Lon), Utils.GeometrySupport.Instance.GetAreasByLevel(Framework.Data.AreaType.Other));
            foreach (Framework.Data.AreaInfo a in al)
            {
                result.AppendLine(a.Name);
            }
            return result.ToString();
        }
    }

    public class PropertyItemInAreaOther : PropertyItem
    {
        public PropertyItemInAreaOther(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_INAREA_OTHER)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            StringBuilder result = new StringBuilder();
            List<Framework.Data.AreaInfo> al = Utils.GeometrySupport.Instance.GetAreasOfLocation(new Framework.Data.Location(gc.Lat, gc.Lon), Utils.GeometrySupport.Instance.GetAreasByLevel(Framework.Data.AreaType.Other));
            foreach (Framework.Data.AreaInfo a in al)
            {
                result.AppendLine(a.Name);
            }
            return result.ToString();
        }
    }

    public class PropertyItemName : PropertyItem
    {
        public PropertyItemName(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_NAME)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Name;
        }
    }
    public class PropertyItemCode : PropertyItem
    {
        public PropertyItemCode(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_CODE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Code;
        }
    }
    public class PropertyItemPublished : PropertyItem
    {
        public PropertyItemPublished(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_PUBLISHED)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.PublishedTime.ToString("d");
        }
    }
    public class PropertyItemLat : PropertyItem
    {
        public PropertyItemLat(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_LAT)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Lat.ToString();
        }
    }
    public class PropertyItemLon : PropertyItem
    {
        public PropertyItemLon(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_LON)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Lon.ToString();
        }
    }
    public class PropertyItemRDx : PropertyItem
    {
        public PropertyItemRDx(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_RDX)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            double x, y;
            if (Utils.Calculus.RDFromLatLong(gc.Lat, gc.Lon, out x, out y))
            {
                return x.ToString();
            }
            else
            {
                return "";
            }
        }
    }
    public class PropertyItemRDy : PropertyItem
    {
        public PropertyItemRDy(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_RDY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            double x, y;
            if (Utils.Calculus.RDFromLatLong(gc.Lat, gc.Lon, out x, out y))
            {
                return y.ToString();
            }
            else
            {
                return "";
            }
        }
    }
    public class PropertyItemCoordinate : PropertyItem
    {
        public PropertyItemCoordinate(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_COORDINATE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon);
        }
    }
    public class PropertyItemAvailable : PropertyItem
    {
        public PropertyItemAvailable(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_AVAILABLE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Available ? Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_YES) : Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_NO);
        }
    }
    public class PropertyItemArchived : PropertyItem
    {
        public PropertyItemArchived(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_ARCHIVED)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Archived ? Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_YES) : Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_NO);
        }
    }
    public class PropertyItemCountry : PropertyItem
    {
        public PropertyItemCountry(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_COUNTRY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Country ?? "";
        }
    }
    public class PropertyItemState : PropertyItem
    {
        public PropertyItemState(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_STATE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.State ?? "";
        }
    }
    public class PropertyItemMunicipality : PropertyItem
    {
        public PropertyItemMunicipality(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_MUNICIPALITY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Municipality ?? "";
        }
    }
    public class PropertyItemCity : PropertyItem
    {
        public PropertyItemCity(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_CITY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.City ?? "";
        }
    }
    public class PropertyItemType : PropertyItem
    {
        public PropertyItemType(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_TYPE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return Utils.LanguageSupport.Instance.GetTranslation(gc.GeocacheType.Name);
        }
    }
    public class PropertyItemPlacedBy : PropertyItem
    {
        public PropertyItemPlacedBy(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_PLACEDBY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.PlacedBy ?? "";
        }
    }
    public class PropertyItemOwner : PropertyItem
    {
        public PropertyItemOwner(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_OWNER)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Owner ?? "";
        }
    }
    public class PropertyItemContainer : PropertyItem
    {
        public PropertyItemContainer(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_CONTAINER)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return Utils.LanguageSupport.Instance.GetTranslation(gc.Container.Name);
        }
    }
    public class PropertyItemTerrain : PropertyItem
    {
        public PropertyItemTerrain(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_TERRAIN)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Terrain.ToString("0.#");
        }
    }
    public class PropertyItemDifficulty : PropertyItem
    {
        public PropertyItemDifficulty(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_DIFFICULTY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Difficulty.ToString("0.#");
        }
    }
    public class PropertyItemDescriptionText : PropertyItem
    {
        public PropertyItemDescriptionText(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_DESCRIPTIONTEXT)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(gc.ShortDescription))
            {
                if (gc.ShortDescriptionInHtml)
                {
                    sb.AppendLine(Utils.Conversion.StripHtmlTags(gc.ShortDescription));
                }
                else
                {
                    sb.AppendLine(gc.ShortDescription);
                }
            }
            if (!string.IsNullOrEmpty(gc.LongDescription))
            {
                if (gc.LongDescriptionInHtml)
                {
                    sb.AppendLine(Utils.Conversion.StripHtmlTags(gc.LongDescription));
                }
                else
                {
                    sb.AppendLine(gc.LongDescription);
                }
            }
            return sb.ToString();
        }
    }
    public class PropertyItemDescriptionHTML : PropertyItem
    {
        public PropertyItemDescriptionHTML(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_DESCRIPTIONHTML)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(gc.ShortDescription))
            {
                if (!gc.ShortDescriptionInHtml)
                {
                    string s = gc.ShortDescription.Replace("\r\n", "cvcvcvcvcvc");
                    s = System.Web.HttpUtility.HtmlEncode(s);
                    s = s.Replace("cvcvcvcvcvc", "<br />");
                    sb.AppendLine(s);
                }
                else
                {
                    sb.AppendLine(gc.ShortDescription);
                }
                sb.AppendLine("<br />");
            }
            if (!string.IsNullOrEmpty(gc.LongDescription))
            {
                if (!gc.LongDescriptionInHtml)
                {
                    string s = gc.LongDescription.Replace("\r\n", "cvcvcvcvcvc");
                    s = System.Web.HttpUtility.HtmlEncode(s);
                    s = s.Replace("cvcvcvcvcvc", "<br />");
                    sb.AppendLine(s);
                }
                else
                {
                    sb.AppendLine(gc.LongDescription);
                }
            }
            return sb.ToString();
        }
    }
    public class PropertyItemUrl : PropertyItem
    {
        public PropertyItemUrl(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_URL)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Url ?? "";
        }
    }
    public class PropertyItemMemberOnly : PropertyItem
    {
        public PropertyItemMemberOnly(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_MEMBERONLY)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.MemberOnly ? Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_YES) : Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_NO);
        }
    }
    public class PropertyItemCustomLat : PropertyItem
    {
        public PropertyItemCustomLat(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_CUSTOMLAT)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.ContainsCustomLatLon ? ((double)gc.CustomLat).ToString() : "";
        }
    }
    public class PropertyItemCustomLon : PropertyItem
    {
        public PropertyItemCustomLon(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_CUSTOMLON)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.ContainsCustomLatLon ? ((double)gc.CustomLon).ToString() : "";
        }
    }
    public class PropertyItemCustomCoordinate : PropertyItem
    {
        public PropertyItemCustomCoordinate(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_CUSTOMCOORD)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.ContainsCustomLatLon ? Utils.Conversion.GetCoordinatesPresentation((double)gc.CustomLat, (double)gc.CustomLon) : "";
        }
    }
    public class PropertyItemAutoCoordinate : PropertyItem
    {
        public PropertyItemAutoCoordinate(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_AUTOCOORD)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.ContainsCustomLatLon ? Utils.Conversion.GetCoordinatesPresentation((double)gc.CustomLat, (double)gc.CustomLon) : Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon);
        }
    }

    public class PropertyItemFavorites : PropertyItem
    {
        public PropertyItemFavorites(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_FAVORITES)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Favorites;
        }
    }
    public class PropertyItemPersonalNotes : PropertyItem
    {
        public PropertyItemPersonalNotes(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_PERSONALNOTES)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.PersonaleNote ?? "";
        }
    }
    public class PropertyItemHints : PropertyItem
    {
        public PropertyItemHints(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_HINTS)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.EncodedHints ?? "";
        }
    }
    public class PropertyItemFlagged : PropertyItem
    {
        public PropertyItemFlagged(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_FLAGGED)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Flagged ? Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_YES) : Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_NO);
        }
    }
    public class PropertyItemFound : PropertyItem
    {
        public PropertyItemFound(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_FOUND)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.Found ? Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_YES) : Utils.LanguageSupport.Instance.GetTranslation(PropertyName.STR_NO);
        }
    }
    public class PropertyItemFoundDate : PropertyItem
    {
        public PropertyItemFoundDate(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_FOUNDDATE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            return gc.FoundDate == null ? "" : ((DateTime)gc.FoundDate).ToString("d");
        }
    }

    public class PropertyItemGCVote : PropertyItem
    {
        public PropertyItemGCVote(Framework.Interfaces.ICore core)
            : base(core, PropertyName.STR_GCVOTE)
        {
        }
        public override object GetValue(Framework.Data.Geocache gc)
        {
            string gcvote = gc.GetCustomAttribute("GCVote") as string;
            return gcvote ?? "";
        }
    }

}
