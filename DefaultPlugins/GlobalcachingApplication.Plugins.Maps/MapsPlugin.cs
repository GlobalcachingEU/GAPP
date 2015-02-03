using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Maps
{
    public class MapsPlugin : Utils.BasePlugin.Plugin
    {
        public const string ACTION_OSMONLINE = "Open street map - online";
        public const string ACTION_OSMOFFLINE = "Open street map - offline";
        public const string ACTION_GOOGLEONLINE = "Google road map - online";

        private MapForm frmOsmMapOnline = null;
        private MapForm frmOsmMapOffline = null;
        private MapForm frmGoogleOnline = null;
        private TileLocalServer _tileLocalServer = null;

        public string GetWindowStateText()
        {
            StringBuilder sb = new StringBuilder();
            if (frmOsmMapOnline == null || !frmOsmMapOnline.Visible)
            {
                sb.Append("frmOsmMapOnline|0|0|100|100|false");
            }
            else
            {
                sb.Append(string.Format("frmOsmMapOnline|{0}|{1}|{2}|{3}|true", frmOsmMapOnline.Left, frmOsmMapOnline.Top, frmOsmMapOnline.Width, frmOsmMapOnline.Height));
            }
            sb.Append("|");
            if (frmOsmMapOffline == null || !frmOsmMapOffline.Visible)
            {
                sb.Append("frmOsmMapOffline|0|0|100|100|false");
            }
            else
            {
                sb.Append(string.Format("frmOsmMapOffline|{0}|{1}|{2}|{3}|true", frmOsmMapOffline.Left, frmOsmMapOffline.Top, frmOsmMapOffline.Width, frmOsmMapOffline.Height));
            }
            sb.Append("|");
            if (frmGoogleOnline == null || !frmGoogleOnline.Visible)
            {
                sb.Append("frmGoogleOnline|0|0|100|100|false");
            }
            else
            {
                sb.Append(string.Format("frmGoogleOnline|{0}|{1}|{2}|{3}|true", frmGoogleOnline.Left, frmGoogleOnline.Top, frmGoogleOnline.Width, frmGoogleOnline.Height));
            }
            return sb.ToString();
        }

        public void SetWindowStates(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                string[] parts = s.Split(new char[] { '|' });
                if (parts.Length == 3 * 6)
                {
                    try
                    {
                        if (bool.Parse(parts[5]))
                        {
                            Action(ACTION_OSMONLINE);
                            frmOsmMapOnline.Bounds = new System.Drawing.Rectangle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]));
                        }
                        else if (frmOsmMapOnline!=null)
                        {
                            frmOsmMapOnline.Hide();
                        }

                        if (bool.Parse(parts[5+6]))
                        {
                            Action(ACTION_OSMOFFLINE);
                            frmOsmMapOffline.Bounds = new System.Drawing.Rectangle(int.Parse(parts[1 + 6]), int.Parse(parts[2 + 6]), int.Parse(parts[3 + 6]), int.Parse(parts[4 + 6]));
                        }
                        else if (frmOsmMapOffline != null)
                        {
                            frmOsmMapOffline.Hide();
                        }

                        if (bool.Parse(parts[5 + 12]))
                        {
                            Action(ACTION_GOOGLEONLINE);
                            frmGoogleOnline.Bounds = new System.Drawing.Rectangle(int.Parse(parts[1 + 12]), int.Parse(parts[2 + 12]), int.Parse(parts[3 + 12]), int.Parse(parts[4 + 12]));
                        }
                        else if (frmGoogleOnline != null)
                        {
                            frmGoogleOnline.Hide();
                        }

                    }
                    catch
                    {
                    }
                }
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Map;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_OSMONLINE;
            }
        }

        public override string FriendlyName
        {
            get
            {
                return "Open Street Maps";
            }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var sett = new PluginSettings(core);

            AddAction(ACTION_OSMONLINE);
            AddAction(ACTION_OSMOFFLINE);
            AddAction(ACTION_GOOGLEONLINE);

            try
            {
                if (string.IsNullOrEmpty(PluginSettings.Instance.OSMOfflineMapFolder))
                {
                    string p = core.PluginDataPath;
                    if (!System.IO.Directory.Exists(p))
                    {
                        System.IO.Directory.CreateDirectory(p);
                    }
                    p = System.IO.Path.Combine(new string[] { p, "OSMOfflineMaps" });
                    if (!System.IO.Directory.Exists(p))
                    {
                        System.IO.Directory.CreateDirectory(p);
                    }
                    PluginSettings.Instance.OSMOfflineMapFolder = p;
                }
            }
            catch
            {
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_ACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_ALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_SEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_SHOWGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_DECOUPLE_WINDOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_DOCK_WINDOW));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetMapsForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetMapsForm.STR_DOWNLOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetMapsForm.STR_DOWNLOADINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetMapsForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetMapsForm.STR_RETRIEVINGLIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetMapsForm.STR_SIZE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_CLEARCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GETMORE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAPS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_OFFLINEOSMMAPS));

            _tileLocalServer = new TileLocalServer();
            _tileLocalServer.Start(core);

            return await base.InitializeAsync(core);
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(this));
            return pnls;
        }


        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
            panel.Apply();
            if (frmOsmMapOnline != null)
            {
                frmOsmMapOnline.SettingsChanged();
            }
            if (frmOsmMapOffline != null)
            {
                frmOsmMapOffline.SettingsChanged();
            }
            if (frmGoogleOnline != null)
            {
                frmGoogleOnline.SettingsChanged();
            }
            if (_tileLocalServer != null)
            {
                _tileLocalServer.SettingsChanged();
            }
            return true;
        }

        public void ClearCache()
        {
            //note: paths assumed, since the factories might not have been created
            string p = System.IO.Path.Combine(new string[] { Core.PluginDataPath, "OSMOnlineCache" });
            if (System.IO.Directory.Exists(p))
            {
                clearFolder(p);
            }
            p = System.IO.Path.Combine(new string[] { Core.PluginDataPath, "OSMOfflineCache" });
            if (System.IO.Directory.Exists(p))
            {
                clearFolder(p);
            }
            p = System.IO.Path.Combine(new string[] { Core.PluginDataPath, "GoogleCache" });
            if (System.IO.Directory.Exists(p))
            {
                clearFolder(p);
            }
        }

        private void clearFolder(string folder)
        {
            string[] fls = System.IO.Directory.GetFiles(folder, "*.png");
            foreach (string f in fls)
            {
                System.IO.File.Delete(f);
            }
            string[] dirs = System.IO.Directory.GetDirectories(folder);
            foreach (string d in dirs)
            {
                clearFolder(d);
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_OSMONLINE)
                {
                    if (frmOsmMapOnline == null)
                    {
                        MapControl.MapCanvas.MapControlFactoryToUse = new MapProviders.OSMOnline.MapControlFactoryOSMOnline(Core);
                        MapControl.MapCanvas.MapControlFactoryToUse.Init();
                        frmOsmMapOnline = new MapForm(Core, action);
                        Framework.Interfaces.IPluginUIMainWindow mainPlugin = (from Framework.Interfaces.IPlugin a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                        if (mainPlugin != null)
                        {
                            if (PluginSettings.Instance.DecoupledChildWindows == null || !PluginSettings.Instance.DecoupledChildWindows.Contains(action))
                            {
                                frmOsmMapOnline.MdiParent = mainPlugin.MainForm;
                            }
                        }
                        if (PluginSettings.Instance.TopMostWindows != null && PluginSettings.Instance.TopMostWindows.Contains(action))
                        {
                            frmOsmMapOnline.TopMost = true;
                        }
                    }
                    if (!frmOsmMapOnline.Visible)
                    {
                        frmOsmMapOnline.Show();
                        frmOsmMapOnline.UpdateView();
                    }
                    if (frmOsmMapOnline.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    {
                        frmOsmMapOnline.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    }
                    frmOsmMapOnline.BringToFront();
                }
                else if (action == ACTION_OSMOFFLINE)
                {
                    if (frmOsmMapOffline == null)
                    {
                        MapControl.MapCanvas.MapControlFactoryToUse = new MapProviders.OSMOffline.MapControlFactoryOSMOffline(Core);
                        MapControl.MapCanvas.MapControlFactoryToUse.Init();
                        frmOsmMapOffline = new MapForm(Core, action);
                        Framework.Interfaces.IPluginUIMainWindow mainPlugin = (from Framework.Interfaces.IPlugin a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                        if (mainPlugin != null)
                        {
                            if (PluginSettings.Instance.DecoupledChildWindows == null || !PluginSettings.Instance.DecoupledChildWindows.Contains(action))
                            {
                                frmOsmMapOffline.MdiParent = mainPlugin.MainForm;
                            }
                        }
                        if (PluginSettings.Instance.TopMostWindows != null && PluginSettings.Instance.TopMostWindows.Contains(action))
                        {
                            frmOsmMapOffline.TopMost = true;
                        }
                    }
                    if (!frmOsmMapOffline.Visible)
                    {
                        frmOsmMapOffline.Show();
                        frmOsmMapOffline.UpdateView();
                    }
                    if (frmOsmMapOffline.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    {
                        frmOsmMapOffline.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    }
                    frmOsmMapOffline.BringToFront();
                }
                else if (action == ACTION_GOOGLEONLINE)
                {
                    if (frmGoogleOnline == null)
                    {
                        MapControl.MapCanvas.MapControlFactoryToUse = new MapProviders.Google.MapControlFactoryGoogle(Core);
                        MapControl.MapCanvas.MapControlFactoryToUse.Init();
                        frmGoogleOnline = new MapForm(Core, action);
                        Framework.Interfaces.IPluginUIMainWindow mainPlugin = (from Framework.Interfaces.IPlugin a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                        if (mainPlugin != null)
                        {
                            if (PluginSettings.Instance.DecoupledChildWindows == null || !PluginSettings.Instance.DecoupledChildWindows.Contains(action))
                            {
                                frmGoogleOnline.MdiParent = mainPlugin.MainForm;
                            }
                        }
                        if (PluginSettings.Instance.TopMostWindows != null && PluginSettings.Instance.TopMostWindows.Contains(action))
                        {
                            frmGoogleOnline.TopMost = true;
                        }
                    }
                    if (!frmGoogleOnline.Visible)
                    {
                        frmGoogleOnline.Show();
                        frmGoogleOnline.UpdateView();
                    }
                    if (frmGoogleOnline.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    {
                        frmGoogleOnline.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    }
                    frmGoogleOnline.BringToFront();
                }

            }
            return result;
        }

    }
}
