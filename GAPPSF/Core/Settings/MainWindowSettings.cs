using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string MainWindowWindowFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowLeftPanelFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowRightPanelFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowTopPanelFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowBottomLeftPanelFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowBottomRightPanelFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowExpandedPanelFeature
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MainWindowMiximizedPanelName
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public int MainWindowHeight
        {
            get { return int.Parse(GetProperty("800")); }
            set { SetProperty(value.ToString()); }
        }

        public int MainWindowWidth
        {
            get { return int.Parse(GetProperty("800")); }
            set { SetProperty(value.ToString()); }
        }

        public int MainWindowTop
        {
            get { return int.Parse(GetProperty("10")); }
            set { SetProperty(value.ToString()); }
        }

        public int MainWindowLeft
        {
            get { return int.Parse(GetProperty("10")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
