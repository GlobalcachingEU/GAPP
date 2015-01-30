using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Shortcuts
{
    public class ShortcutsSetup : Utils.BasePlugin.Plugin
    {
        private string _shortcutDatabaseFile = null;

        public override string FriendlyName
        {
            get
            {
                return "Shortcut assignment";
            }
        }
        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.General;
            }
        }
        public override bool Prerequisite
        {
            get
            {
                return true;
            }
        }
        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            bool result = await base.InitializeAsync(core);
            if (result)
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_CLEAR));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INFO));

                try
                {
                    _shortcutDatabaseFile = System.IO.Path.Combine(core.PluginDataPath, "Shortcuts.db3" );
                }
                catch
                {
                }

                if (System.IO.File.Exists(_shortcutDatabaseFile))
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_shortcutDatabaseFile))
                    {
                        initDatabase(dbcon);
                        DbDataReader dr = dbcon.ExecuteReader("select * from shortcut");
                        while (dr.Read())
                        {
                            core.ShortcutInfo.Add(new Framework.Data.ShortcutInfo() { PluginType = (string)dr["plugintype"], PluginAction = (string)dr["action"], PluginSubAction = (string)dr["subaction"], ShortcutKeys = (Keys)(int)dr["keys"], ShortcutKeyString = (string)dr["keystring"] });
                        }
                    }
                }
                else
                {
                    core.ShortcutInfo.AddRange(new Framework.Data.ShortcutInfo[]
                            {
                                new Framework.Data.ShortcutInfo(){PluginType="GlobalcachingApplication.Plugins.GCView.GeocacheViewer", PluginAction="View Geocache", PluginSubAction="", ShortcutKeys = Keys.F3, ShortcutKeyString = "F3" },
                                new Framework.Data.ShortcutInfo(){PluginType="GlobalcachingApplication.Plugins.GMap.GMap", PluginAction="Google Map", PluginSubAction="", ShortcutKeys = Keys.Control | Keys.M, ShortcutKeyString = "Ctrl+M" },
                                new Framework.Data.ShortcutInfo(){PluginType="GlobalcachingApplication.Plugins.FilterEx.GeocacheSearch", PluginAction="Search geocache", PluginSubAction="", ShortcutKeys = Keys.Control | Keys.F, ShortcutKeyString = "Ctrl+F" },
                                new Framework.Data.ShortcutInfo(){PluginType="GlobalcachingApplication.Plugins.TxtSearch.GeocacheTextSearch", PluginAction="Search geocache by text", PluginSubAction="", ShortcutKeys = Keys.Control | Keys.T, ShortcutKeyString = "Ctrl+T" },
                            }
                    );
                }
            }
            return result;
        }

        private void initDatabase(Utils.DBCon dbcon)
        {
            object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='shortcut'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'shortcut' (plugintype text, action text, subaction text, keys integer, keystring text)");
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            try
            {
                SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
                panel.ApplyShortcuts(Core);
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_shortcutDatabaseFile))
                {
                    initDatabase(dbcon);
                    dbcon.ExecuteNonQuery("delete from shortcut");
                    foreach (Framework.Data.ShortcutInfo scuts in Core.ShortcutInfo)
                    {
                        dbcon.ExecuteNonQuery(string.Format("insert into shortcut (plugintype, action, subaction, keys, keystring) values ('{0}', '{1}', '{2}', {3}, '{4}')", scuts.PluginType, scuts.PluginAction.Replace("'", "''"), scuts.PluginSubAction.Replace("'", "''"), (int)scuts.ShortcutKeys, scuts.ShortcutKeyString.Replace("'", "''")));
                    }
                }
                Core.OnShortcutInfoChanged();
            }
            catch
            {
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
