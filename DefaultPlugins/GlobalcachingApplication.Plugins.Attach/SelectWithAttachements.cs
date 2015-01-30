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
                        string databaseFile = System.IO.Path.Combine(new string[] { Core.PluginDataPath, "attachements.db3" });
                        if (System.IO.File.Exists(databaseFile))
                        {
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                            Utils.DBCon dbcon = new Utils.DBConComSqlite(databaseFile);
                            DbDataReader dr = dbcon.ExecuteReader("select distinct code from attachements");
                            while (dr.Read())
                            {
                                string code = dr["code"] as string;
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
