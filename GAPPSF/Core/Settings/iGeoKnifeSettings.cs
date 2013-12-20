using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int IGeoKnifeMaxLogs
        {
            get { return int.Parse(GetProperty("5")); }
            set { SetProperty(value.ToString()); }
        }

        public string IGeoKnifeFolderName
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

    }
}
