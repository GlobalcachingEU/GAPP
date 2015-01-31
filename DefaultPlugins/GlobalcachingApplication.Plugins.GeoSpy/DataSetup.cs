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
        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            bool result = await base.InitializeAsync(core);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SETTYPE));

            if (result)
            {
                addCacheType(97001, "Civil", PluginSettings.Instance.GPXTagCivil);
                addCacheType(97002, "Historic and religious", PluginSettings.Instance.GPXTagHistoricAndReligious);
                addCacheType(97003, "Natural", PluginSettings.Instance.GPXTagNatural);
                addCacheType(97004, "Technical", PluginSettings.Instance.GPXTagTechnical);
                addCacheType(97005, "Military", PluginSettings.Instance.GPXTagMilitary);
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
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97001).GPXTag = PluginSettings.Instance.GPXTagCivil;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97002).GPXTag = PluginSettings.Instance.GPXTagHistoricAndReligious;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97003).GPXTag = PluginSettings.Instance.GPXTagNatural;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97004).GPXTag = PluginSettings.Instance.GPXTagTechnical;
                    Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 97005).GPXTag = PluginSettings.Instance.GPXTagMilitary;
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
