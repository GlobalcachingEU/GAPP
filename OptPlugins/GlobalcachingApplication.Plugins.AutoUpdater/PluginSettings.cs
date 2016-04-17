using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoUpdater
{
    public class PluginSettings
    {
        public static PluginSettings _uniqueInstance = null;
        private ICore _core = null;
        private List<AreaItemInfo> _areaInfoItems = null;

        public PluginSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static PluginSettings Instance
        {
            get { return _uniqueInstance; }
        }

        public List<AreaItemInfo> AreaInfoItems
        {
            get
            {
                if (_areaInfoItems == null)
                {
                    lock (this)
                    {
                        if (_areaInfoItems == null)
                        {
                            _areaInfoItems = new List<AreaItemInfo>();
                            try
                            {
                                using (System.Net.WebClient wc = new System.Net.WebClient())
                                {
                                    string doc = wc.DownloadString("http://www.globalcaching.eu/Service/GeocacheCodesExFilter.aspx");
                                    if (doc != null)
                                    {
                                        string[] lines = doc.Replace("\r", "").Split(new char[] { '\n' },  StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var l in lines)
                                        {
                                            _areaInfoItems.Add(new AreaItemInfo(l));
                                        }
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                return _areaInfoItems;
            }
        }

        public int[] GetSelectedCountries()
        {
            return (from a in AreaInfoItems where a.Level == "Land" && GetUpdateAreaInfo(a) select a.Code).ToArray();
        }

        public int[] GetSelectedStates()
        {
            return (from a in AreaInfoItems where a.Level == "Provincie" && GetUpdateAreaInfo(a) select a.Code).ToArray();
        }

        public bool ShowSettingsDialog
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.ShowSettingsDialog2", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.ShowSettingsDialog2", value); }
        }

        public bool AutomaticDownloadGeocaches
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.AutomaticDownloadGeocaches", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.AutomaticDownloadGeocaches", value); }
        }

        public bool GetUpdateAreaInfo(AreaItemInfo ai)
        {
            return _core.SettingsProvider.GetSettingsValueBool(string.Format("AutoUpdater.Update{0}{1}", ai.Level, ai.Code), false);
        }

        public void SetUpdateAreaInfo(AreaItemInfo ai, bool val)
        {
            _core.SettingsProvider.SetSettingsValueBool(string.Format("AutoUpdater.Update{0}{1}", ai.Level, ai.Code), val);
        }

        public string FromLocation
        {
            get { return _core.SettingsProvider.GetSettingsValue("AutoUpdater.FromLocation", ""); }
            set { _core.SettingsProvider.SetSettingsValue("AutoUpdater.FromLocation", value ?? ""); }
        }

        public int FromLocationRadius
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("AutoUpdater.FromLocationRadius", 10); }
            set { _core.SettingsProvider.SetSettingsValueInt("AutoUpdater.FromLocationRadius", value); }
        }
    }
}
