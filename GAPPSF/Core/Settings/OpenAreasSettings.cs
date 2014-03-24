using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int OpenAreasWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int OpenAreasWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int OpenAreasWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int OpenAreasWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public int OpenAreasRadius
        {
            get { return int.Parse(GetProperty("161")); }
            set { SetProperty(value.ToString()); }
        }

        public bool OpenAreasSelectedGeocaches
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool OpenAreasMysteryOnlyIfCorrected
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool OpenAreasAddWaypoints
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool OpenAreasCustomWaypoints
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public int OpenAreasFillOpacity
        {
            get { return int.Parse(GetProperty("50")); }
            set { SetProperty(value.ToString()); }
        }

        public int OpenAreasStrokeOpacity
        {
            get { return int.Parse(GetProperty("50")); }
            set { SetProperty(value.ToString()); }
        }

        public string OpenAreasGeocacheColor
        {
            get { return GetProperty("Red"); }
            set { SetProperty(value); }
        }

        public string OpenAreasWaypointColor
        {
            get { return GetProperty("Red"); }
            set { SetProperty(value); }
        }

        public string OpenAreasCustomColor
        {
            get { return GetProperty("Green"); }
            set { SetProperty(value); }
        }

        public string OpenAreasCustomLocations
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
    }
}
