using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int DatabaseBackupMaxBackups
        {
            get { return int.Parse(GetProperty("10")); }
            set { SetProperty(value.ToString()); }
        }

        public bool DatabaseBackupAutomatic
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public TimeSpan DatabaseBackupAutomaticInterval
        {
            get { return TimeSpan.Parse(GetProperty("12:00:00")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
