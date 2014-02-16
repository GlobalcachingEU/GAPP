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
        public string LiveAPILogGeocachesLogs
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public double LiveAPILogGeocachesMaxImageSizeMB
        {
            get { return double.Parse(GetProperty("2"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }

        public int LiveAPILogGeocachesMaxImageWidth
        {
            get { return int.Parse(GetProperty("600")); }
            set { SetProperty(value.ToString()); }
        }

        public int LiveAPILogGeocachesMaxImageHeight
        {
            get { return int.Parse(GetProperty("600")); }
            set { SetProperty(value.ToString()); }
        }

        public int LiveAPILogGeocachesImageQuality
        {
            get { return int.Parse(GetProperty("75")); }
            set { SetProperty(value.ToString()); }
        }
    }
}
