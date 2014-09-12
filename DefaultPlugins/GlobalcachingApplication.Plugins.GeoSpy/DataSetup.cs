using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GeoSpy
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
                return "GeoSpy";
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
                addCacheType(97001, "Civil", Properties.Settings.Default.GPXTagCivil);
                addCacheType(97002, "Historic and religious", Properties.Settings.Default.GPXTagHistoricAndReligious);
                addCacheType(97003, "Natural", Properties.Settings.Default.GPXTagNatural);
                addCacheType(97004, "Technical", Properties.Settings.Default.GPXTagTechnical);
                addCacheType(97005, "Military", Properties.Settings.Default.GPXTagMilitary);
            }

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
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97001).GPXTag = Properties.Settings.Default.GPXTagCivil;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97002).GPXTag = Properties.Settings.Default.GPXTagHistoricAndReligious;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97003).GPXTag = Properties.Settings.Default.GPXTagNatural;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97004).GPXTag = Properties.Settings.Default.GPXTagTechnical;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97005).GPXTag = Properties.Settings.Default.GPXTagMilitary;
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
