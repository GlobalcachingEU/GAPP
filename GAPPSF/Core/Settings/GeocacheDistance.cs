using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public double? GetGeocacheDistance(string gcCode)
        {
            return _settingsStorage.GetGeocacheDistance(gcCode);
        }
        public void SetGeocacheDistance(string gcCode, double? dist)
        {
            _settingsStorage.SetGeocacheDistance(gcCode, dist);
        }
    }
}
