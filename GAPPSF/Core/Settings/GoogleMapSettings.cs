using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int GoogleMapWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleMapWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleMapWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleMapWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public int GoogleMapClusterMinimumCountGeocaches
        {
            get { return int.Parse(GetProperty("1000")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleMapClusterMaximumZoomLevel
        {
            get { return int.Parse(GetProperty("13")); }
            set { SetProperty(value.ToString()); }
        }

        public int GoogleMapClusterGridSize
        {
            get { return int.Parse(GetProperty("40")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
