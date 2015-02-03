using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Attach
{
    public class SelectWithAttachements : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SELECT = "Select geocaches with attachements";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_SELECT);
            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SELECT)
                {
                    Core.Geocaches.BeginUpdate();
                    try
                    {
                        lock (Core.SettingsProvider)
                        {
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                            List<string> codes = Core.SettingsProvider.Database.Fetch<string>(string.Format("select distinct code from {0}", Core.SettingsProvider.GetFullTableName("attachements")));
                            foreach (string code in codes)
                            {
                                Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, code);
                                if (gc != null)
                                {
                                    gc.Selected = true;
                                }
                            }
                        }

                    }
                    catch
                    {
                    }
                    Core.Geocaches.EndUpdate();
                }
            }
            return result;
        }
    }
}
