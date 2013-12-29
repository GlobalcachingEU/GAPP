using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string GDAKTargetFolder
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public int GDAKMaxLogCount
        {
            get { return int.Parse(GetProperty("5")); }
            set { SetProperty(value.ToString()); }
        }

        public int GDAKMaxImagesInFolder
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

        public bool GDAKExportOfflineImages
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
