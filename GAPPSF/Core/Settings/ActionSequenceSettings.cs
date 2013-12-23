using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string ActionSequenceXml
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public long ActionSequenceID
        {
            get { return long.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }
    }
}
