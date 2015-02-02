using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ScrPresets
{
    public class Presets: Utils.BasePlugin.Plugin
    {
        public const string ACTION_SAVECURRENT = "Presets|Save";
        public const string ACTION_SEP = "Presets|-";
        public const string ACTION_SPLITSCREEN = "Presets|Splitscreen";

        private string _presetDatabaseFile = null;
        private List<string> _presets = new List<string>();

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SAVECURRENT);
            AddAction(ACTION_SEP);
            AddAction(ACTION_SPLITSCREEN);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetNameForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetNameForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetNameForm.STR_TITLE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_PRESETS));

            try
            {
                _presetDatabaseFile = System.IO.Path.Combine(new string[] { core.PluginDataPath, "Presets.db3" });
            }
            catch
            {
            }

            try
            {
                if (System.IO.File.Exists(_presetDatabaseFile))
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_presetDatabaseFile))
                    {
                        initDatabase(dbcon);
                        DbDataReader dr = dbcon.ExecuteReader("select name from preset");
                        while (dr.Read())
                        {
                            _presets.Add((string)dr["name"]);
                            AddAction(string.Concat("Presets|", (string)dr["name"]));
                        }
                    }
                }
            }
            catch
            {
            }


            return await base.InitializeAsync(core);
        }

        private void initDatabase(Utils.DBCon dbcon)
        {
            object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='preset'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'preset' (name text)");
            }
            o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='forms'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'forms' (preset text, plugintype text, x integer, y integer, w integer, h integer, visible integer, customtag text)");
            }
            else if (!dbcon.ColumnExists("forms", "customtag"))
            {
                dbcon.ExecuteNonQuery("alter table 'forms' add customtag text");
            }
        }


        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            try
            {
                SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
                List<string> presets = panel.PresetNames;
                if (presets.Count != _presets.Count)
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_presetDatabaseFile))
                    {
                        initDatabase(dbcon);
                        List<string> tobeRemoved = new List<string>();
                        foreach (string p in _presets)
                        {
                            if (!presets.Contains(p))
                            {
                                dbcon.ExecuteNonQuery(string.Format("delete from forms where preset='{0}'", p.Replace("'","''")));
                                dbcon.ExecuteNonQuery(string.Format("delete from preset where name='{0}'", p.Replace("'", "''")));

                                RemoveAction(string.Concat("Presets|", p));
                                tobeRemoved.Add(p);
                            }
                        }
                        foreach (string p in tobeRemoved)
                        {
                            _presets.Remove(p);
                        }
                    }
                }
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
            pnls.Add(new SettingsPanel(_presets));
            return pnls;
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SPLITSCREEN;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GenericWindow;
            }
        }

        public override string FriendlyName
        {
            get
            {
                return "Window presets";
            }
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();
            if (PluginSettings.Instance.FirstUse)
            {
                PluginSettings.Instance.FirstUse = false;

                //Form main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a.MainForm).FirstOrDefault();
                //if (main != null)
                //{
                //    main.WindowState = FormWindowState.Maximized;
                //    await splitScreen();
                //}
            }
        }


        private async Task splitScreen()
        {
            Form main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a.MainForm).FirstOrDefault();
            if (main != null)
            {
                //get the 3 screens: cache list, geocache viewer and map
                Framework.Interfaces.IPluginUIChildWindow gclistPlugin = (from Framework.Interfaces.IPluginUIChildWindow a in Core.GetPlugin(Framework.PluginType.UIChildWindow) where a.GetType().ToString() == "GlobalcachingApplication.Plugins.SimpleCacheList.SimpleCacheList" select a).FirstOrDefault();
                Framework.Interfaces.IPluginUIChildWindow viewerPlugin = (from Framework.Interfaces.IPluginUIChildWindow a in Core.GetPlugin(Framework.PluginType.UIChildWindow) where a.GetType().ToString() == "GlobalcachingApplication.Plugins.GCView.GeocacheViewer" select a).FirstOrDefault();
                Framework.Interfaces.IPluginUIChildWindow mapPlugin = (from Framework.Interfaces.IPluginUIChildWindow a in Core.GetPlugin(Framework.PluginType.Map) where a.GetType().ToString() == "GlobalcachingApplication.Plugins.GMap.GMap" select a).FirstOrDefault();
                if (gclistPlugin != null && viewerPlugin != null && mapPlugin != null)
                {
                    await gclistPlugin.ActionAsync(gclistPlugin.DefaultAction);
                    await viewerPlugin.ActionAsync(viewerPlugin.DefaultAction);
                    await mapPlugin.ActionAsync(mapPlugin.DefaultAction);
                    if (gclistPlugin.ChildForm != null && viewerPlugin.ChildForm != null && mapPlugin.ChildForm != null)
                    {
                        System.Drawing.Size clntSize = main.ClientSize;
                        clntSize.Height -= 80;
                        clntSize.Width -= 20;
                        gclistPlugin.ChildForm.SetBounds(0, 0, clntSize.Width, clntSize.Height / 2);
                        viewerPlugin.ChildForm.SetBounds(0, clntSize.Height / 2, clntSize.Width /2, clntSize.Height / 2);
                        mapPlugin.ChildForm.SetBounds(clntSize.Width / 2, clntSize.Height / 2, clntSize.Width / 2, clntSize.Height / 2);
                    }
                }
            }
        }


        private void newPreset()
        {
            //check if saved preset
            string presetname = null;
            using (PresetNameForm dlg = new PresetNameForm(_presets))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    presetname = dlg.PresetName;
                }
            }
            if (!string.IsNullOrEmpty(presetname))
            {
                try
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_presetDatabaseFile))
                    {
                        initDatabase(dbcon);
                        if (!_presets.Contains(presetname))
                        {
                            //we need to add the action
                            AddAction(string.Concat("Presets|",presetname));
                            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                            main.AddAction(this, "Presets", presetname);

                            _presets.Add(presetname);

                        }
                        dbcon.ExecuteNonQuery(string.Format("delete from forms where preset='{0}'", presetname));
                        dbcon.ExecuteNonQuery(string.Format("delete from preset where name='{0}'", presetname));

                        dbcon.ExecuteNonQuery(string.Format("insert into preset (name) values ('{0}')", presetname));
                        List<Framework.Interfaces.IPlugin> pins = Core.GetPlugins();
                        foreach (Framework.Interfaces.IPlugin pin in pins)
                        {
                            Framework.Interfaces.IPluginUIChildWindow p = pin as Framework.Interfaces.IPluginUIChildWindow;
                            if (p != null)
                            {
                                if (p.ChildForm != null && p.ChildForm.Visible)
                                {
                                    dbcon.ExecuteNonQuery(string.Format("insert into forms (preset, plugintype, x, y, w, h, visible) values ('{0}', '{1}', {2}, {3}, {4}, {5}, 1)", presetname, p.GetType().ToString(), p.ChildForm.Left, p.ChildForm.Top, p.ChildForm.Width, p.ChildForm.Height));
                                }
                                else
                                {
                                    dbcon.ExecuteNonQuery(string.Format("insert into forms (preset, plugintype, x, y, w, h, visible) values ('{0}', '{1}', 0, 0, 100, 100, 0)", presetname, p.GetType().ToString()));
                                }
                            }
                            else if (pin.GetType().ToString() == "GlobalcachingApplication.Plugins.Maps.MapsPlugin")
                            {
                                //special onw
                                try
                                {
                                    MethodInfo mi = pin.GetType().GetMethod("GetWindowStateText");
                                    if (mi != null)
                                    {
                                        string s = mi.Invoke(pin, null) as string;
                                        if (s != null)
                                        {
                                            dbcon.ExecuteNonQuery(string.Format("insert into forms (preset, plugintype, x, y, w, h, visible, customtag) values ('{0}', '{1}', 0, 0, 100, 100, 0, '{2}')", presetname, pin.GetType().ToString(), s.Replace("'", "''")));
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private async Task openPreset(string presetname)
        {
            try
            {
                int pos = presetname.IndexOf('|');
                if (pos > 0)
                {
                    presetname = presetname.Substring(pos + 1);
                    if (_presets.Contains(presetname))
                    {
                        using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_presetDatabaseFile))
                        {
                            initDatabase(dbcon);
                            List<Framework.Interfaces.IPlugin> pins = Core.GetPlugins();
                            DbDataReader dr = dbcon.ExecuteReader(string.Format("select * from forms where preset='{0}'", presetname));
                            while (dr.Read())
                            {
                                Framework.Interfaces.IPluginUIChildWindow p = (from Framework.Interfaces.IPlugin a in pins where a.GetType().ToString() == (string)dr["plugintype"] select a).FirstOrDefault() as Framework.Interfaces.IPluginUIChildWindow;
                                if (p != null)
                                {
                                    if ((int)dr["visible"] == 0)
                                    {
                                        if (p.ChildForm != null && p.ChildForm.Visible)
                                        {
                                            p.ChildForm.Hide();
                                        }
                                    }
                                    else
                                    {
                                        if (p.ChildForm == null || !p.ChildForm.Visible)
                                        {
                                            await p.ActionAsync(p.DefaultAction);
                                        }
                                        if (p.ChildForm != null)
                                        {
                                            p.ChildForm.SetBounds((int)dr["x"], (int)dr["y"], (int)dr["w"], (int)dr["h"]);
                                        }
                                    }
                                }
                                else if ((string)dr["plugintype"]=="GlobalcachingApplication.Plugins.Maps.MapsPlugin")
                                {
                                    try
                                    {
                                        Framework.Interfaces.IPlugin mp = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.Maps.MapsPlugin");
                                        if (mp != null)
                                        {
                                            MethodInfo mi = mp.GetType().GetMethod("SetWindowStates");
                                            if (mi != null)
                                            {
                                                mi.Invoke(mp, new object[] { dr["customtag"] as string });
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SPLITSCREEN)
                {
                    await splitScreen();
                }
                else if (action == ACTION_SAVECURRENT)
                {
                    newPreset();
                }
                else
                {
                    await openPreset(action);
                }
            }
            return result;
        }

    }
}
