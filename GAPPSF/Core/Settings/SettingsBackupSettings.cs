using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int SettingsBackupMaxBackups
        {
            get { return int.Parse(GetProperty("20")); }
            set { SetProperty(value.ToString()); }
        }

        public bool SettingsBackupAtStartup
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }
    }
}
