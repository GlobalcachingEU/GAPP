using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string FavoritesGCCodes
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
    }
}
