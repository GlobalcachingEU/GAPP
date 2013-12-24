using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string LiveAPIToken
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string LiveAPITokenStaging
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string LiveAPIMemberType
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public int LiveAPIMemberTypeId
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

        public bool LiveAPIDeselectAfterUpdate
        {
            get { return bool.Parse(GetProperty("True")); }
            set { SetProperty(value.ToString()); }
        }

        public int LiveAPIDelaySearchForGeocaches
        {
            get { return int.Parse(GetProperty("2000")); }
            set { SetProperty(value.ToString()); }
        }

        public int LiveAPIImportGeocachesBatchSize
        {
            get { return int.Parse(GetProperty("30")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
