using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string FindLogsOfUserLogTypes
        {
            get { return GetProperty("2"); }
            set { SetProperty(value); }
        }

        public string FindLogsOfUserUsers
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public bool FindLogsOfUserImportMissing
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public bool FindLogsOfUserBetweenDates
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public DateTime FindLogsOfUserMinDate
        {
            get { return DateTime.Parse(GetProperty(DateTime.Now.Date.ToString("s"))); }
            set { SetProperty(value.ToString("s")); }
        }

        public DateTime FindLogsOfUserMaxDate
        {
            get { return DateTime.Parse(GetProperty(DateTime.Now.Date.ToString("s"))); }
            set { SetProperty(value.ToString("s")); }
        }

    }
}
