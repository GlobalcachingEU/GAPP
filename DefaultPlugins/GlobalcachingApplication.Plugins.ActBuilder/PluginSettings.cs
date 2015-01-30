using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public class PluginSettings
    {
        public static PluginSettings _uniqueInstance = null;
        private ICore _core = null;

        public PluginSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static PluginSettings Instance
        {
            get { return _uniqueInstance; }
        }

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("ActBuilder.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("ActBuilder.WindowPos", value); }
        }

        public int LeftPanelWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ActBuilder.LeftPanelWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("ActBuilder.LeftPanelWidth", value); }
        }

        public int RightPanelWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ActBuilder.RightPanelWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("ActBuilder.RightPanelWidth", value); }
        }

        public bool ShowFlowCompletedMessage
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ActBuilder.ShowFlowCompletedMessage", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ActBuilder.ShowFlowCompletedMessage", value); }
        }

        public bool ShowConnectionLabel
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ActBuilder.ShowConnectionLabel", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ActBuilder.ShowConnectionLabel", value); }
        }
    }
}
