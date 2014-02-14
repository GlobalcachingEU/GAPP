using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string GarminPOIExportPath
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GarminPOIClearExportDirectory
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GarminPOIExportGeocachePOIs
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GarminPOIExportWaypointPOIs
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public int GarminPOINameLengthLimit
        {
            get { return int.Parse(GetProperty("22")); }
            set { SetProperty(value.ToString()); }
        }

        public int GarminPOIDescriptionLengthLimit
        {
            get { return int.Parse(GetProperty("84")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GarminPOIRunPOILoader
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public string GarminPOIPOILoaderFilename
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GarminPOIPassDirectoryToPOILoader
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GarminPOIRunPOILoaderSilently
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public UIControls.GeocacheFilter.BooleanEnum GarminPOIPOINameTypeName
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }
    }
}
