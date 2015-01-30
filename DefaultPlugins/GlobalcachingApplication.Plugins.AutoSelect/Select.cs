using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoSelect
{
    public class Select : Utils.BasePlugin.Plugin
    {
        private Hashtable _prevList;
        private SynchronizationContext _context = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_AUTO_SELECTNEW));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            return await base.InitializeAsync(core);
        }

        public override string FriendlyName
        {
            get { return "Automatic selection"; }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();

            _prevList = new Hashtable();
            foreach (Framework.Data.Geocache g in Core.Geocaches)
            {
                _prevList.Add(g.Code, g);
            }

            Core.Geocaches.GeocacheAdded += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
            Core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
            Core.Geocaches.GeocacheRemoved += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheRemoved);
        }

        void checkGeocachesAdded()
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                if (Core.Geocaches.Count > 0)
                {
                    using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                    {
                        Hashtable ht = new Hashtable();
                        foreach (Framework.Data.Geocache g in Core.Geocaches)
                        {
                            if (_prevList[g.Code] == null && !g.Saved)
                            {
                                g.Selected = true;
                            }
                            ht.Add(g.Code, g);
                        }
                        _prevList = ht;
                    }
                }
            }), null);
        }

        void Geocaches_GeocacheRemoved(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Properties.Settings.Default.AutoSelectNewGeocaches)
            {
                if (_prevList[e.Geocache.Code] != null)
                {
                    _prevList.Remove(e.Geocache.Code);
                }
            }
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutoSelectNewGeocaches)
            {
                if (_prevList.Count != Core.Geocaches.Count)
                {
                    checkGeocachesAdded();
                }
            }
        }

        void Geocaches_GeocacheAdded(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Properties.Settings.Default.AutoSelectNewGeocaches)
            {
                checkGeocachesAdded();
            }
        }

    }
}
