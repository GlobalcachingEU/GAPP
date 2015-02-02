using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Startup
{
    public class AutoStart : Utils.BasePlugin.Plugin
    {
        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SORT));

            return await base.InitializeAsync(core);
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


        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();

            //execute after all application initialized methods have been executed
            SynchronizationContext context = null;
            context = SynchronizationContext.Current;
            if (context == null)
            {
                context = new SynchronizationContext();
            }

            if (PluginSettings.Instance.Startup != null && PluginSettings.Instance.Startup.Count>0)
            {
                context.Post(new SendOrPostCallback(async delegate(object state)
                {
                    foreach (string s in PluginSettings.Instance.Startup)
                    {
                        string[] parts = s.Split(new char[] { '@' }, 2);
                        Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, parts[0]);
                        if (p != null)
                        {
                            try
                            {
                                await p.ActionAsync(parts[1].Replace('@', '|'));
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
