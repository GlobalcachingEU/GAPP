using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int LocusMaxLogs
        {
            get { return int.Parse(GetProperty("5")); }
            set { SetProperty(value.ToString()); }
        }

        public string LocusFolderName
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public int LocusMaxImagesInFolder
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

        public bool LocusExportOfflineImages
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
