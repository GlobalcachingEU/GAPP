using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        public GridLength MainWindowLeftPanelWidth
        {
            get { return new GridLength(double.Parse(GetProperty("300"), CultureInfo.InvariantCulture)); }
            set { SetProperty(value.Value.ToString(CultureInfo.InvariantCulture)); }
        }

        public bool MainWindowLeftPanelIsExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public GridLength MainWindowRightPanelWidth
        {
            get { return new GridLength(double.Parse(GetProperty("300"), CultureInfo.InvariantCulture)); }
            set { SetProperty(value.Value.ToString(CultureInfo.InvariantCulture)); }
        }

        public GridLength MainWindowBottomLeftPanelWidth
        {
            get { return new GridLength(double.Parse(GetProperty("300"), CultureInfo.InvariantCulture)); }
            set { SetProperty(value.Value.ToString(CultureInfo.InvariantCulture)); }
        }

        public GridLength MainWindowBottomPanelHeight
        {
            get { return new GridLength(double.Parse(GetProperty("300"), CultureInfo.InvariantCulture)); }
            set { SetProperty(value.Value.ToString(CultureInfo.InvariantCulture)); }
        }

        public bool MainWindowRightPanelIsExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool MainWindowBottomPanelIsExpanded
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public string MainWindowShortCutKeyAssignment
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

    }
}
