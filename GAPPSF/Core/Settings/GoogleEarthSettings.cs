using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int GoogleEarthWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleEarthWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleEarthWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int GoogleEarthWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public double GoogleEarthFlyToSpeed
        {
            get { return double.Parse(GetProperty("0.5")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GoogleEarthFixedView
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public int GoogleEarthTiltView
        {
            get { return int.Parse(GetProperty("45")); }
            set { SetProperty(value.ToString()); }
        }

        public int GoogleEarthAltitudeView
        {
            get { return int.Parse(GetProperty("500")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
