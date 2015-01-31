using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GPSRSerial
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

        public int BaudRate
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GPSRSerial.BaudRate", 9600); }
            set { _core.SettingsProvider.SetSettingsValueInt("GPSRSerial.BaudRate", value); }
        }

        public int Databits
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GPSRSerial.Databits", 8); }
            set { _core.SettingsProvider.SetSettingsValueInt("GPSRSerial.Databits", value); }
        }

        public string ComPort
        {
            get { return _core.SettingsProvider.GetSettingsValue("GPSRSerial.ComPort", "COM1"); }
            set { _core.SettingsProvider.SetSettingsValue("GPSRSerial.ComPort", value); }
        }

        public string Parity
        {
            get { return _core.SettingsProvider.GetSettingsValue("GPSRSerial.Parity", "None"); }
            set { _core.SettingsProvider.SetSettingsValue("GPSRSerial.Parity", value); }
        }

        public string StopBits
        {
            get { return _core.SettingsProvider.GetSettingsValue("GPSRSerial.StopBits", "One"); }
            set { _core.SettingsProvider.SetSettingsValue("GPSRSerial.StopBits", value); }
        }

    }
}
