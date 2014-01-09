using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int PurgeLogsKeepLogCount
        {
            get { return int.Parse(GetProperty("5")); }
            set { SetProperty(value.ToString()); }
        }

        public int PurgeLogsOlderThanDays
        {
            get { return int.Parse(GetProperty("30")); }
            set { SetProperty(value.ToString()); }
        }

        public bool PurgeLogsKeepOfOwnedCaches
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool PurgeLogsKeepOwnLogs
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public string PurgeLogsKeepLogsOfUsers
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string PurgeLogsRemoveAllLogsOfUsers
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

    }
}
