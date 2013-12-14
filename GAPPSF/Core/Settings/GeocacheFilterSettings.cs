using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int GeocacheFilterWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GeocacheFilterWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GeocacheFilterWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int GeocacheFilterWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GeocacheFilterStatusExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public UIControls.GeocacheFilter.GeocacheStatus GeocacheFilterGeocacheStatus
        {
            get { return (UIControls.GeocacheFilter.GeocacheStatus)Enum.Parse(typeof(UIControls.GeocacheFilter.GeocacheStatus), GetProperty("Enabled")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GeocacheFilterOwnExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public UIControls.GeocacheFilter.BooleanEnum GeocacheFilterOwn
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GeocacheFilterFoundExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public UIControls.GeocacheFilter.BooleanEnum GeocacheFilterFound
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GeocacheFilterFoundByExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public UIControls.GeocacheFilter.BooleanEnum GeocacheFilterFoundByAll
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterFoundBy
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GeocacheFilterNotFoundByExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public UIControls.GeocacheFilter.BooleanEnum GeocacheFilterNotFoundByAny
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterNotFoundBy
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GeocacheFilterLocationExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterLocation
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }
        public UIControls.GeocacheFilter.BooleanEnum GeocacheFilterLocationKm
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }
        public double GeocacheFilterLocationRadius
        {
            get { return double.Parse(GetProperty("30.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }

        public bool GeocacheFilterCountryStateExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterCountry
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }
        public string GeocacheFilterState
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GeocacheFilterMunicipalityCityExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterMunicipality
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }
        public string GeocacheFilterCity
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GeocacheFilterGeocacheTypesExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterGeocacheTypes
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GeocacheFilterGeocacheContainersExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterGeocacheContainers
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GeocacheFilterFavExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public double GeocacheFilterMinFav
        {
            get { return double.Parse(GetProperty("0.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }
        public double GeocacheFilterMaxFav
        {
            get { return double.Parse(GetProperty("99999"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }

        public bool GeocacheFilterTerrainExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public double GeocacheFilterMinTerrain
        {
            get { return double.Parse(GetProperty("0.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }
        public double GeocacheFilterMaxTerrain
        {
            get { return double.Parse(GetProperty("5"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }

        public bool GeocacheFilterDifficultyExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public double GeocacheFilterMinDifficulty
        {
            get { return double.Parse(GetProperty("0.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }
        public double GeocacheFilterMaxDifficulty
        {
            get { return double.Parse(GetProperty("5"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }

        public bool GeocacheFilterHiddenDateExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public DateTime GeocacheFilterMinHiddenDate
        {
            get { return DateTime.Parse(GetProperty(DateTime.Now.Date.ToString("s"))); }
            set { SetProperty(value.ToString("s")); }
        }
        public DateTime GeocacheFilterMaxHiddenDate
        {
            get { return DateTime.Parse(GetProperty(DateTime.Now.Date.ToString("s"))); }
            set { SetProperty(value.ToString("s")); }
        }

        public bool GeocacheFilterGeocacheAttributesExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }
        public UIControls.GeocacheFilter.AttributeFilter GeocacheFilterAttributeFilter
        {
            get { return (UIControls.GeocacheFilter.AttributeFilter)Enum.Parse(typeof(UIControls.GeocacheFilter.AttributeFilter), GetProperty("ContainsAll")); }
            set { SetProperty(value.ToString()); }
        }
        public string GeocacheFilterGeocacheAttributes
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

    }
}
