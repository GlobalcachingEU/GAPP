using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string ExcelTargetPath
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public string ExcelSheets
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }
    }
}
