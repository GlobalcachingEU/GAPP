using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string GPXFileName
        {
            get { return GetProperty("geocaches"); }
            set { SetProperty(value); }
        }

        public string GPXTargetFolder
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public GPX.TargetDevice GPXTargetDevice
        {
            get { return (GPX.TargetDevice)Enum.Parse(typeof(GPX.TargetDevice), GetProperty("Folder")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GPXExportGGZ
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GPXAddChildWaypoints
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GPXUseHintsForDescription
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GPXAddAdditionWaypointsToDescription
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GPXAddFieldnotesToDescription
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public int GPXMaxLogCount
        {
            get { return int.Parse(GetProperty("5")); }
            set { SetProperty(value.ToString()); }
        }

        public int GPXMaxNameLength
        {
            get { return int.Parse(GetProperty("255")); }
            set { SetProperty(value.ToString()); }
        }

        public int GPXMinStartOfname
        {
            get { return int.Parse(GetProperty("255")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GPXUseNameForGCCode
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public string GPXExtraCoordPrefix
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool GPXAddExtraInfo
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public string GPXVersion
        {
            get { return GetProperty("1.0.1"); }
            set { SetProperty(value); }
        }

    }
}
