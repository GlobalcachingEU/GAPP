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
        public bool GeocacheFilterLocationKm
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }
        public double GeocacheFilterLocationRadius
        {
            get { return double.Parse(GetProperty("30.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(((double)value).ToString(CultureInfo.InvariantCulture)); }
        }

    }
}
