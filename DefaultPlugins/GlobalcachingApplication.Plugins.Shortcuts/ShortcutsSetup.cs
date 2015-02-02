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

        public class ShortCutPoco
        {
            public string plugintype { get; set; }
            public string action { get; set; }
            public string subaction { get; set; }
            public int keys { get; set; }
            public string keystring { get; set; }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            bool result = await base.InitializeAsync(core);
            if (result)
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_CLEAR));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INFO));

                lock (core.SettingsProvider)
                {
                    if (core.SettingsProvider.TableExists(core.SettingsProvider.GetFullTableName("shortcut")))
                    {
                        List<ShortCutPoco> pocos = core.SettingsProvider.Database.Fetch<ShortCutPoco>(string.Format("select * from {0}", core.SettingsProvider.GetFullTableName("shortcut")));
                        foreach (var poco in pocos)
                        {
                            core.ShortcutInfo.Add(new Framework.Data.ShortcutInfo() { PluginType = poco.plugintype, PluginAction = poco.action, PluginSubAction = poco.subaction, ShortcutKeys = (Keys)poco.keys, ShortcutKeyString = poco.keystring });
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
            }
            return result;
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            try
            {
                SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
                panel.ApplyShortcuts(Core);
                lock (Core.SettingsProvider)
                {
                    if (!Core.SettingsProvider.TableExists(Core.SettingsProvider.GetFullTableName("shortcut")))
                    {
                        Core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (plugintype text, action text, subaction text, keys integer, keystring text)", Core.SettingsProvider.GetFullTableName("shortcut")));
                    }
                    Core.SettingsProvider.Database.Execute(string.Format("delete from {0}", Core.SettingsProvider.GetFullTableName("shortcut")));
                    foreach (Framework.Data.ShortcutInfo scuts in Core.ShortcutInfo)
                    {
                        Core.SettingsProvider.Database.Execute(string.Format("insert into {5} (plugintype, action, subaction, keys, keystring) values ('{0}', '{1}', '{2}', {3}, '{4}')", scuts.PluginType, scuts.PluginAction.Replace("'", "''"), scuts.PluginSubAction.Replace("'", "''"), (int)scuts.ShortcutKeys, scuts.ShortcutKeyString.Replace("'", "''"), Core.SettingsProvider.GetFullTableName("shortcut")));
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
