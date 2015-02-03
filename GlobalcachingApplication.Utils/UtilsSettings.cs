using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Utils
{
    public class UtilsSettings
    {
        public static UtilsSettings _uniqueInstance = null;
        private ICore _core = null;

        public UtilsSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static UtilsSettings Instance
        {
            get { return _uniqueInstance; }
        }

        public int TopMostOpaque
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("Utils.TopMostOpaque", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("Utils.TopMostOpaque", value); }
        }

        public InterceptedStringCollection DecoupledChildWindows
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Utils.DecoupledChildWindows", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Utils.DecoupledChildWindows", value); }
        }

        public InterceptedStringCollection TopMostWindows
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Utils.TopMostWindows", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Utils.TopMostWindows", value); }
        }

    }
}
