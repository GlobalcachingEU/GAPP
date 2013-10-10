using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Threading;

namespace GlobalcachingApplication.Plugins.Startup
{
    public class AutoStart : Utils.BasePlugin.Plugin
    {
        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SORT));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override string FriendlyName
        {
            get
            {
                return "Startup";
            }
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(Core));
            return pnls;
        }


        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
            panel.Apply();
            return true;
        }


        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();

            //execute after all application initialized methods have been executed
            SynchronizationContext context = null;
            context = SynchronizationContext.Current;
            if (context == null)
            {
                context = new SynchronizationContext();
            }

            if (Properties.Settings.Default.Startup != null && Properties.Settings.Default.Startup.Count>0)
            {
                context.Post(new SendOrPostCallback(delegate(object state)
                {
                    foreach (string s in Properties.Settings.Default.Startup)
                    {
                        string[] parts = s.Split(new char[] { '@' }, 2);
                        Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, parts[0]);
                        if (p != null)
                        {
                            try
                            {
                                p.Action(parts[1].Replace('@', '|'));
                            }
                            catch
                            {
                            }
                        }
                    }
                }), null);
            }
        }
    }
}
