using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int WebBrowserWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int WebBrowserWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int WebBrowserWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int WebBrowserWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public string WebBrowserHomePage
        {
            get { return GetProperty("http://www.geocaching.com"); }
            set { SetProperty(value); }
        }
        public string WebBrowserBookmarks
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
    }
}
