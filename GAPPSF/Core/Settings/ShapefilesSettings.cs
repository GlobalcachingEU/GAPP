using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string ShapeFiles
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
    }
}
