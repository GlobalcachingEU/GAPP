using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class DataSetup : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SETTINGS = "Settings";

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.OKAPI; //so settings will be on the right place
            }
        }
        public override bool Prerequisite
        {
            get
            {
                return true;
            }
        }
        public override string FriendlyName
        {
            get
            {
                return "OKAPI";
            }
        }
        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            bool result = base.Initialize(core);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            AddAction(ACTION_SETTINGS);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_RETRIEVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SITES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_USERID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_USERNAME));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SiteInfoGermany.STR_INFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_TITLE));

            addCacheType(96001, "OC Traditional Cache", "Traditional Cache");
            addCacheType(96002, "OC Multi-cache", "Multi-cache");
            addCacheType(96003, "OC Virtual Cache", "Virtual Cache");
            addCacheType(96004, "OC Event Cache", "Event Cache");
            addCacheType(96005, "OC Unknown (Mystery) Cache", "Unknown Cache");
            addCacheType(96006, "OC Webcam Cache", "Webcam Cache");
            addCacheType(96007, "OC Moving Cache", "Locationless (Reverse) Cache");
            addCacheType(96008, "OC Quiz Cache", "Unknown Cache");
            addCacheType(96009, "OC Drive-in Cache", "Traditional Cache");

            List<SiteInfo> sites = SiteManager.Instance.AvailableSites;
            foreach (SiteInfo si in sites)
            {
                core.GeocachingAccountNames.SetAccountName(si.GeocodePrefix, si.Username);
            }

            //delelop only: be sure a valid site is active!
            //OKAPIService.develop_CreateAttributesList(SiteManager.Instance.ActiveSite);

            return result;
        }

        protected void addCacheType(int id, string name)
        {
            Framework.Data.GeocacheType ct = new Framework.Data.GeocacheType();
            ct.ID = id;
            ct.Name = name;
            Core.GeocacheTypes.Add(ct);
        }

        protected void addCacheType(int id, string name, string gpxTag)
        {
            Framework.Data.GeocacheType ct = new Framework.Data.GeocacheType(gpxTag);
            ct.ID = id;
            ct.Name = name;
            Core.GeocacheTypes.Add(ct);
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
            pnls.Add(new SettingsPanel(Core));
            return pnls;
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SETTINGS)
                {
                    using (SettingsForm dlg = new SettingsForm(Core))
                    {
                        dlg.ShowDialog();
                    }
                }
            }
            return result;
        }
    }
}
