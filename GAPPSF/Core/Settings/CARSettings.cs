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
        public int CARWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int CARWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int CARWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int CARWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public UIControls.GeocacheFilter.BooleanEnum CARKm
        {
            get { return (UIControls.GeocacheFilter.BooleanEnum)Enum.Parse(typeof(UIControls.GeocacheFilter.BooleanEnum), GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public double CARRadius
        {
            get { return double.Parse(GetProperty("3.0"), CultureInfo.InvariantCulture); }
            set { SetProperty(value.ToString(CultureInfo.InvariantCulture)); }
        }
    }
}
