using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {

        public Core.Logger.Level DebugLogViewLevel
        {
            get { return (Core.Logger.Level)Enum.Parse(typeof(Core.Logger.Level), GetProperty("Error")); }
            set { SetProperty(value.ToString()); }
        }

        public int DebugLogViewWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int DebugLogViewWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int DebugLogViewWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int DebugLogViewWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }


    }
}
