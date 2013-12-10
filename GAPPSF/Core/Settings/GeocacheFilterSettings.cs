using System;
using System.Collections.Generic;
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

    }
}
