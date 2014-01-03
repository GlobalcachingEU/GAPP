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
        public string LiveAPIGetGCCacheTypes
        {
            get { return GetProperty("2\r3\r4\r5\r6\r8\r9\r11\r12\r13\r137\r453\r605\r1304\r1858\r3653\r3773\r3774\r4738"); }
            set { SetProperty(value); }
        }

        public string LiveAPIGetGCCacheContainers
        {
            get { return GetProperty("1\r2\r3\r4\r5\r6\r8"); }
            set { SetProperty(value); }
        }

        public string LiveAPIGetGCLocation
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public double LiveAPIGetGCRadius
        {
            get { return double.Parse(GetProperty("10.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }

        public UIControls.GeocacheFilter.BooleanEnum LiveAPIGetGCLocationKm
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public string LiveAPIGetGCCodes
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string LiveAPIGetGCName
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string LiveAPIGetGCHiddenBy
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string LiveAPIGetGCExcludeFoundBy
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string LiveAPIGetGCExcludeHiddenBy
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public int LiveAPIGetGCFavMin
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }
        public int LiveAPIGetGCFavMax
        {
            get { return int.Parse(GetProperty("9999")); }
            set { SetProperty(value.ToString()); }
        }

        public double LiveAPIGetGCDifMin
        {
            get { return double.Parse(GetProperty("0.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }

        public double LiveAPIGetGCDifMax
        {
            get { return double.Parse(GetProperty("5.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }

        public double LiveAPIGetGCTerMin
        {
            get { return double.Parse(GetProperty("0.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }

        public double LiveAPIGetGCTerMax
        {
            get { return double.Parse(GetProperty("5.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }

        public int LiveAPIGetGCTrackMin
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }
        public int LiveAPIGetGCTrackMax
        {
            get { return int.Parse(GetProperty("9999")); }
            set { SetProperty(value.ToString()); }
        }

        public bool LiveAPIGetGCBetweenDates
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public DateTime LiveAPIGetGCMinDate
        {
            get { return DateTime.Parse(GetProperty(DateTime.Now.Date.ToString("s"))); }
            set { SetProperty(value.ToString("s")); }
        }

        public DateTime LiveAPIGetGCMaxDate
        {
            get { return DateTime.Parse(GetProperty(DateTime.Now.Date.ToString("s"))); }
            set { SetProperty(value.ToString("s")); }
        }

        public bool? LiveAPIGetGCExcludeArchived
        {
            get 
            {
                string s = GetProperty(null);
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                else
                {
                    return bool.Parse(s);
                }
            }
            set { SetProperty(value==null?null:value.ToString()); }
        }

        public bool? LiveAPIGetGCExcludeAvailable
        {
            get
            {
                string s = GetProperty(null);
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                else
                {
                    return bool.Parse(s);
                }
            }
            set { SetProperty(value == null ? null : value.ToString()); }
        }

        public bool? LiveAPIGetGCExcludePMO
        {
            get
            {
                string s = GetProperty(null);
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                else
                {
                    return bool.Parse(s);
                }
            }
            set { SetProperty(value == null ? null : value.ToString()); }
        }

        public int LiveAPIGetGCTotalMaximum
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public int LiveAPIGetGCMaximumLogs
        {
            get { return int.Parse(GetProperty("5")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
