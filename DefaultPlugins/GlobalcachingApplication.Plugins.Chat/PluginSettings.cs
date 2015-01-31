using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Chat
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
            get { return _core.SettingsProvider.GetSettingsValueRectangle("Chat.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("Chat.WindowPos", value); }
        }

        public int BottomPanelHeight
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("CAR.BottomPanelHeight", 120); }
            set { _core.SettingsProvider.SetSettingsValueInt("CAR.BottomPanelHeight", value); }
        }

        public int RightPanelWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("CAR.RightPanelWidth", 120); }
            set { _core.SettingsProvider.SetSettingsValueInt("CAR.RightPanelWidth", value); }
        }

    }
}
