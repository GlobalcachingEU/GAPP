using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Munzee
{
    public class DataSetup : Utils.BasePlugin.Plugin
    {
        public override Framework.PluginType PluginType
        {
            get
            {
                //return Framework.PluginType.General;
                return Framework.PluginType.ExportData; //so settings will be on the right place
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
                return "Munzee";
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

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SETTYPE));

            if (result)
            {
                addCacheType(95342, "Munzee", Properties.Settings.Default.GPXTagMunzee);
                addCacheType(95343, "Virtual Munzee", Properties.Settings.Default.GPXTagVirtual);
                addCacheType(95344, "Maintenance Munzee", Properties.Settings.Default.GPXTagMaintenance);
                addCacheType(95345, "Business Munzee", Properties.Settings.Default.GPXTagBusiness);
                addCacheType(95346, "Mystery Munzee", Properties.Settings.Default.GPXTagMystery);
                addCacheType(95347, "NFC Munzee", Properties.Settings.Default.GPXTagNFC);
                addCacheType(95348, "Premium Munzee", Properties.Settings.Default.GPXTagPremium);
            }

            core.GeocachingAccountNames.SetAccountName("MZ", Properties.Settings.Default.AccountName);
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
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes,95342).GPXTag = Properties.Settings.Default.GPXTagMunzee;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95343).GPXTag = Properties.Settings.Default.GPXTagVirtual;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95344).GPXTag = Properties.Settings.Default.GPXTagMaintenance;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95345).GPXTag = Properties.Settings.Default.GPXTagBusiness;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95346).GPXTag = Properties.Settings.Default.GPXTagMystery;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95347).GPXTag = Properties.Settings.Default.GPXTagNFC;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95348).GPXTag = Properties.Settings.Default.GPXTagPremium;
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

    }
}
