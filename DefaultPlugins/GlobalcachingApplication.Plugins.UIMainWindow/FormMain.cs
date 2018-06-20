using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GlobalcachingApplication.Framework.Interfaces;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    public partial class FormMain : Utils.BasePlugin.BaseUIMainWindowForm
    {
        public const string STR_FILE = "File";
        public const string STR_IMPORT = "Import";
        public const string STR_EXPORT = "Export";
        public const string STR_SEARCH = "Search";
        public const string STR_ACTION = "Action";
        public const string STR_SCRIPTS = "Scripts";
        public const string STR_LIVEAPI = "LiveAPI";
        public const string STR_OKAPI = "OKAPI";
        public const string STR_LANGUAGE = "Language";
        public const string STR_ORIGINALTEXT = "Original text";
        public const string STR_PLUGINS = "Plugins";
        public const string STR_WINDOW = "Window";
        public const string STR_HELP = "Help";
        public const string STR_ABOUT = "About";
        public const string STR_SETTINGS = "Settings...";
        public const string STR_FORUM = "Forum...";
        public const string STR_ASKSAVEDATA = "The data has been changed. Do you want to save the data first?";
        public const string STR_WARNING = "Warning";
        public const string STR_HOME = "Home";
        public const string STR_CENTER = "Center";
        public const string STR_NOCACHESELECTED = "No active geocache";
        public const string STR_MAPS = "Maps";
        public const string STR_TOOLBAR = "toolbar";
        public const string STR_EXECUTE = "Execute";
        public const string STR_CUSTOMTOOLBAR = "Custom toolbar...";
        public const string STR_INSTRUCTIONVIDEOS = "Instruction videos";
        public const string STR_CHANGEHISTORY = "Change history";
        public const string STR_EXIT = "Exit";
        public const string STR_ACTIONSEQUNCER = "Action sequence";

        public class ToolbarActionProperties
        {
            public Framework.PluginType PluginType { get; private set; }
            public string Action { get; private set; }
            public string SubAction { get; private set; }
            public Image ButtonImage { get; private set; }
            public ToolStrip DefaultToolStrip { get; private set; }
            public object Tag { get; set; }

            public ToolbarActionProperties(Framework.PluginType ptype, string act, string subact, Image img, ToolStrip defStrip)
            {
                PluginType = ptype;
                Action = act;
                SubAction = subact;
                ButtonImage = img;
                DefaultToolStrip = defStrip;
                Tag = null;
            }
        }

        public class ActionBuilderPluginAction : PluginAction
        {
            public override string ToString()
            {
                return this.subaction;
            }
        }
        public class ActionSequencePluginAction : PluginAction
        {
            public override string ToString()
            {
                return this.subaction;
            }
        }

        private FormProgressHandler _frmProgress = null;
        private NotificationContainer _notificationContainer = null;
        private List<ToolbarActionProperties> _toolbarActionProperties = new List<ToolbarActionProperties>();
        private List<ToolStripButton> _toolbarItems = new List<ToolStripButton>();
        private bool _sizeInitialized = false;
        private ToolStripMenuItem _exitMeniItem = null;
        private int _selCount = 0;
        private bool _allowedToClose = false;

        public FormMain()
        {
            InitializeComponent();
        }

        public FormMain(Utils.BasePlugin.BaseUIMainWindow ownerPlugin, Framework.Interfaces.ICore core)
            : base(ownerPlugin, core)
        {
            var sett = new PluginSettings(core);

            InitializeComponent();

            core.LanguageItems.AddRange(FormSettings.LanguageItems);
            core.LanguageItems.AddRange(FormSettingsTreeView.LanguageItems);
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_FILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ACTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ACTIONSEQUNCER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SCRIPTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LIVEAPI));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_OKAPI));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LANGUAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ORIGINALTEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_PLUGINS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WINDOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_HELP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ABOUT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SETTINGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ASKSAVEDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_HOME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CENTER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_FORUM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_MAPS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TOOLBAR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXECUTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CUSTOMTOOLBAR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_INSTRUCTIONVIDEOS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CHANGEHISTORY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXIT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CustomToolbarSettingForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CustomToolbarSettingForm.STR_SHOWTOOLBAR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CustomToolbarSettingForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NotificationContainer.STR_TITLE));

            //file toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.InternalStorage, "Save", "", Properties.Resources.database_save, toolStripFile));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.InternalStorage, "New", "", Properties.Resources.database_add, toolStripFile));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.InternalStorage, "Open", "", Properties.Resources.database, toolStripFile));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.ExportData, "Export Garmin POI", "All", Properties.Resources.exportpoi, toolStripFile));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.ExportData, "Export Garmin POI", "Selected", Properties.Resources.exportpoisel, toolStripFile));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.ImportData, "Import GCVote", "All", Properties.Resources.gcvote, toolStripFile));

            //search toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GeocacheSelectFilter, "Search geocache", "", Properties.Resources.magnifier__plus, toolStripSearch));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GeocacheSelectFilter, "Search geocache by text", "", Properties.Resources.magnifier__pencil, toolStripSearch));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GeocacheSelectFilter, "Quick Select", "Clear selection", Properties.Resources.deselect_all, toolStripSearch));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GeocacheSelectFilter, "Quick Select", "Select All", Properties.Resources.select_all, toolStripSearch));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GeocacheSelectFilter, "Select geocaches by area", "", Properties.Resources.selectarea, toolStripSearch));

            //script toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.PluginManager, "Package manager", "", Properties.Resources.download, toolStripScripts));

            //windows toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Chat", "", Properties.Resources.chat, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Generate statistics", "", Properties.Resources.statistics, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Geocache Editor", "", Properties.Resources.gcedit, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Waypoint Editor", "", Properties.Resources.wpedit, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "View Geocache", "", Properties.Resources.view, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Simple cache list", "", Properties.Resources.list, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Solver", "", Properties.Resources.solver, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Web browser", "", Properties.Resources.browser, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GenericWindow, "Presets", "Splitscreen", Properties.Resources.Icon_Split, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "GCVote dashboard", "", Properties.Resources.gcvote, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.GeocacheCollection, "Geocache collections", "", Properties.Resources.categories, toolStripWindows));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.UIChildWindow, "Formula Solver", "", Properties.Resources.mathematics, toolStripWindows));

            //maps toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "Google Map", "", Properties.Resources.google, toolStripMaps));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "OpenLayers Map", "", Properties.Resources.openlayers, toolStripMaps));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "Open street map - online", "", Properties.Resources.osm, toolStripMaps));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "Open street map - offline", "", Properties.Resources.maps, toolStripMaps));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "Google road map - online", "", Properties.Resources.google2, toolStripMaps));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "Open areas", "", Properties.Resources.holes, toolStripMaps));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Map, "Google Earth", "", Properties.Resources.googleearth, toolStripMaps));

            //Live API toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Import Pocket Queries", "", Properties.Resources.dbimport, toolStripLiveAPI));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Import geocaches", "", Properties.Resources.Importgc, toolStripLiveAPI));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Log geocache", "Single", Properties.Resources.found, toolStripLiveAPI));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Log geocache", "Selected", Properties.Resources.foundmore, toolStripLiveAPI));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Image Gallery", "", Properties.Resources.image, toolStripLiveAPI));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Trackable groups", "", Properties.Resources.travelbug, toolStripLiveAPI));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.LiveAPI, "Log trackables", "", Properties.Resources._48, toolStripLiveAPI));

            //actions toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Action, "Action builder", "Editor", Properties.Resources.Flowchart, toolStripActions));
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Action, "Action builder", "Download and publish", Properties.Resources.server, toolStripActions));

            //actions sequence toolbar
            _toolbarActionProperties.Add(new ToolbarActionProperties(Framework.PluginType.Action, "Action sequence", "Edit", Properties.Resources.sequence, toolStripActionSequence));

            _notificationContainer = new NotificationContainer(this);
            _notificationContainer.Visible = false;
            this.Controls.Add(_notificationContainer);
            _frmProgress = FormProgressHandler.Create(core, this);
            UpdateLocations();
            UpdateCountStatus();

            this.Icon = Properties.Resources.Globalcaching;
            bool maximize = PluginSettings.Instance.WindowMaximized;
            if (PluginSettings.Instance.WindowPos != null && !PluginSettings.Instance.WindowPos.IsEmpty)
            {
                this.Bounds = PluginSettings.Instance.WindowPos;
            }
            if (maximize)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);

            List<Framework.Interfaces.IPlugin> pl = core.GetPlugins();
            foreach (Framework.Interfaces.IPlugin p in pl)
            {
                p.Notification += new Framework.EventArguments.NotificationEventHandler(p_Notification);
            }
            core.PluginAdded += new Framework.EventArguments.PluginEventHandler(core_PluginAdded);


            core.ShortcutInfoChanged += new EventHandler(core_ShortcutInfoChanged);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);

            core_ActiveGeocacheChanged(this, null);

            foreach( ToolStripMenuItem p in toolStripSplitButtonTranslate.DropDownItems)
            {
                if (p.Tag != null && p.Tag.ToString() == PluginSettings.Instance.SelectedTranslater)
                {
                    toolStripSplitButtonTranslate.Image = p.Image;
                }
            }
            
        }

        public void ApplicationInitialized()
        {
            List<ToolStrip> tsl = new List<ToolStrip>();
            tsl.AddRange(new ToolStrip[] { toolStripFile, toolStripSearch, toolStripScripts, toolStripWindows, toolStripLiveAPI, toolStripMaps, toolStripCustom, toolStripActions, toolStripActionSequence });

            toolStripContainer1.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();

            foreach (ToolStrip ts in tsl)
            {
                toolStripContainer1.TopToolStripPanel.Controls.Remove(ts);
            }

            toolStripScripts.Visible = false;
            toolStripActionSequence.Visible = false;

            toolStripFile.Location = PluginSettings.Instance.ToolbarFileLocation;
            toolStripSearch.Location = PluginSettings.Instance.ToolbarSearchLocation;
            toolStripScripts.Location = PluginSettings.Instance.ToolbarScriptsLocation;
            toolStripWindows.Location = PluginSettings.Instance.ToolbarWindowsLocation;
            toolStripLiveAPI.Location = PluginSettings.Instance.ToolbarLiveAPIlocation;
            toolStripMaps.Location = PluginSettings.Instance.ToolbarMapsLocation;
            toolStripCustom.Location = PluginSettings.Instance.ToolbarCustomLocation;
            toolStripActions.Location = PluginSettings.Instance.ToolbarActionsLocation;
            toolStripActionSequence.Location = PluginSettings.Instance.ToolbarActionSequenceLocation;

            _sizeInitialized = true;

            fileToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarFile;
            searchToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarSearch;
            scriptsToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarScripts;
            toolStripScripts.Visible = PluginSettings.Instance.ToolbarScripts; //by default visible
            windowsToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarWindows;
            mapsToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarMaps;
            liveAPIToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarLiveAPI;
            toolStripCustom.Visible = PluginSettings.Instance.ToolbarCustom && toolStripCustom.Items.Count > 0;
            actionToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarActions;
            actionSequencerToolbarToolStripMenuItem.Checked = PluginSettings.Instance.ToolbarActionSequence;
            toolStripActions.Visible = PluginSettings.Instance.ToolbarActions; //by default visible
            toolStripActionSequence.Visible = PluginSettings.Instance.ToolbarActionSequence;

            foreach (ToolStrip ts in tsl)
            {
                toolStripContainer1.TopToolStripPanel.Controls.Add(ts);
            }

            toolStripContainer1.TopToolStripPanel.ResumeLayout(true);
            toolStripContainer1.ResumeLayout(true);

            fileToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            _exitMeniItem = new ToolStripMenuItem();
            _exitMeniItem.ShortcutKeys = Keys.Alt | Keys.F4;
            _exitMeniItem.ShortcutKeyDisplayString = "Alt+F4";
            _exitMeniItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXIT);
            fileToolStripMenuItem.DropDownItems.Add(_exitMeniItem);
            _exitMeniItem.Click += new EventHandler(_exitMeniItem_Click);

            oKAPIToolStripMenuItem.Visible = PluginSettings.Instance.ShowOKAPIMenu;

            FillHelpPluginMenu();
        }

        private void FillHelpPluginMenu()
        {
            menuItemPlugins.Enabled = false;
            menuItemPlugins.DropDownItems.Clear();

            foreach (IPlugin plugin in Core.GetPlugins())
            {
                if (plugin.IsHelpAvailable)
                {
                    menuItemPlugins.Enabled = true;
                    ToolStripItem item = new ToolStripMenuItem();
                    item.Text = Utils.LanguageSupport.Instance.GetTranslation(plugin.FriendlyName);
                    item.Enabled = true;
                    item.Tag = plugin;
                    item.Click += new EventHandler(helpItem_Click);
                    menuItemPlugins.DropDownItems.Add(item);
                }
            }
        }

        void helpItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;
            IPlugin plugin = (IPlugin)clicked.Tag;
            plugin.ShowHelp();
        }

        async void _exitMeniItem_Click(object sender, EventArgs e)
        {
            bool cancel = false;
            if (Core.AutoSaveOnClose)
            {
                Framework.Interfaces.IPluginInternalStorage p = (from Framework.Interfaces.IPluginInternalStorage ip in Core.GetPlugin(Framework.PluginType.InternalStorage) select ip).FirstOrDefault();
                if (p != null)
                {
                    cancel = ! await p.SaveAllData();
                }
            }
            else
            {
                int cnt = 0;
                cnt += (from Framework.Data.Geocache c in Core.Geocaches where !c.Saved select c).Count();
                cnt += (from Framework.Data.Log c in Core.Logs where !c.Saved select c).Count();
                cnt += (from Framework.Data.LogImage c in Core.LogImages where !c.Saved select c).Count();
                cnt += (from Framework.Data.Waypoint c in Core.Waypoints where !c.Saved select c).Count();
                if (cnt > 0)
                {
                    System.Windows.Forms.DialogResult res = MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ASKSAVEDATA), Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                    if (res == System.Windows.Forms.DialogResult.Yes)
                    {
                        Framework.Interfaces.IPluginInternalStorage p = (from Framework.Interfaces.IPluginInternalStorage ip in Core.GetPlugin(Framework.PluginType.InternalStorage) select ip).FirstOrDefault();
                        if (p != null)
                        {
                            cancel = !await p.SaveAllData();
                        }
                    }
                    else if (res == System.Windows.Forms.DialogResult.No)
                    {
                    }
                    else
                    {
                        cancel = true;
                    }
                }
            }
            if (!cancel)
            {
                _allowedToClose = true;
                Close();
            }
        }

        public Point BottomRightOfClientArea
        {
            get
            {
                return (new Point(statusStripMain.Right, statusStripMain.Top));
            }
        }

        void core_PluginAdded(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            e.Plugin.Notification += new Framework.EventArguments.NotificationEventHandler(p_Notification);
        }

        void p_Notification(object sender, Framework.EventArguments.NotificationEventArgs e)
        {
            _notificationContainer.AddNotificationMessage(e.MessageBox);
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Core.ActiveGeocache == null)
            {
                toolStripStatusLabelActiveGeocache.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOCACHESELECTED);
                toolStripStatusLabelActiveGeocache.IsLink = false;
                toolStripSplitButtonTranslate.Enabled = false;
            }
            else
            {
                toolStripStatusLabelActiveGeocache.Text = string.Format("{0} - {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name ?? "");
                toolStripStatusLabelActiveGeocache.IsLink = true;
                toolStripSplitButtonTranslate.Enabled = true;
            }
        }

        void core_ShortcutInfoChanged(object sender, EventArgs e)
        {
            foreach (PluginAction pa in _pluginActionList)
            {
                ToolStripMenuItem mi = pa.Tag as ToolStripMenuItem;
                if (mi != null)
                {
                    mi.ShortcutKeys = Keys.None;
                    mi.ShortcutKeyDisplayString = "";
                }
            }

            foreach (PluginAction pa in _pluginActionList)
            {
                ToolStripMenuItem mi = pa.Tag as ToolStripMenuItem;
                if (mi != null)
                {
                    var p = (from s in Core.ShortcutInfo where s.PluginType == pa.plugin.GetType().ToString() && s.PluginAction == pa.action && (s.PluginSubAction ?? "") == (pa.subaction ?? "") select s).FirstOrDefault();
                    if (p != null)
                    {
                        mi.ShortcutKeyDisplayString = p.ShortcutKeyString;
                        mi.ShortcutKeys = p.ShortcutKeys;
                    }
                }
            }
        }

        public void UpdateCountStatus()
        {
            _selCount = (from Framework.Data.Geocache wp in Core.Geocaches
                            where wp.Selected
                            select wp).Count();

            toolStripStatusLabelCounts.Text = string.Format("{0}:{6}({4})/{1}/{2}/{3} ({5})", Core.Geocaches.Count, Core.Logs.Count, Core.Waypoints.Count, Core.LogImages.Count, _selCount, Core.GeocachingComAccount.AccountName, Core.GeocacheImages.Count);
            toolStripStatusLabelCounts.ToolTipText = string.Format("#Geocaches: {0}\r\n#Geocache images: {6}\r\n#Selected: {4}\r\n#Logs: {1}\r\n#Waypoints: {2}\r\n#Log images: {3}\r\nUser name: {5}", Core.Geocaches.Count, Core.Logs.Count, Core.Waypoints.Count, Core.LogImages.Count, _selCount, Core.GeocachingComAccount.AccountName, Core.GeocacheImages.Count);
        }

        public void UpdateLocations()
        {
            toolStripStatusLabelHomeLocation.Text = string.Format("{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_HOME), Utils.Conversion.GetCoordinatesPresentation(Core.HomeLocation));
            toolStripStatusLabelCenterLocation.Text = string.Format("{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_CENTER), Utils.Conversion.GetCoordinatesPresentation(Core.CenterLocation));
        }

        public void UpdateDataSourceName(Framework.Interfaces.IPlugin pin)
        {
            Framework.Interfaces.IPluginInternalStorage ipin = pin as Framework.Interfaces.IPluginInternalStorage;
            if (ipin != null)
            {
                if (string.IsNullOrEmpty(ipin.DataSourceName))
                {
                    toolStripStatusLabelDataSourceName.Text = "-";
                    toolStripStatusLabelDataSourceName.ToolTipText = "No data source";
                }
                else
                {
                    toolStripStatusLabelDataSourceName.Text = System.IO.Path.GetFileName(ipin.DataSourceName);
                    toolStripStatusLabelDataSourceName.ToolTipText = ipin.DataSourceName;
                }
            }
            else
            {
                toolStripStatusLabelDataSourceName.Text = "-";
            }
        }

        public void AddPluginActionForToolbar(PluginAction pa)
        {
            var tsp = (from t in _toolbarActionProperties where t.PluginType == pa.plugin.PluginType && t.Action == (pa.action ?? "") && t.SubAction == (pa.subaction ?? "") select t).FirstOrDefault();
            if (tsp != null)
            {
                ToolStripButton tb = new ToolStripButton(tsp.ButtonImage);
                if (!string.IsNullOrEmpty(pa.subaction))
                {
                    tb.Text = string.Format("{0} - {1}", Utils.LanguageSupport.Instance.GetTranslation(pa.action), Utils.LanguageSupport.Instance.GetTranslation(pa.subaction));
                }
                else
                {
                    tb.Text = string.Format("{0}", Utils.LanguageSupport.Instance.GetTranslation(pa.action));
                }
                tb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
                tb.ImageTransparentColor = System.Drawing.Color.Magenta;
                tb.Click += new EventHandler(mi_Click);
                tb.Tag = pa;
                tsp.DefaultToolStrip.Items.Add(tb);
                if (_sizeInitialized && (
                    (tsp.DefaultToolStrip == toolStripFile && PluginSettings.Instance.ToolbarFile) ||
                    (tsp.DefaultToolStrip == toolStripSearch && PluginSettings.Instance.ToolbarSearch) ||
                    (tsp.DefaultToolStrip == toolStripScripts && PluginSettings.Instance.ToolbarScripts) ||
                    (tsp.DefaultToolStrip == toolStripWindows && PluginSettings.Instance.ToolbarWindows) ||
                    (tsp.DefaultToolStrip == toolStripMaps && PluginSettings.Instance.ToolbarMaps) ||
                    (tsp.DefaultToolStrip == toolStripActions && PluginSettings.Instance.ToolbarActions) ||
                    (tsp.DefaultToolStrip == toolStripLiveAPI && PluginSettings.Instance.ToolbarLiveAPI)
                    ))
                {
                    tsp.DefaultToolStrip.Visible = true;
                }
                _toolbarItems.Add(tb);

                //custom toolbar?
                if (CustomToolbarSettingForm.GetToolbarPropertySelected(tsp))
                {
                    tb = new ToolStripButton(tsp.ButtonImage);
                    if (!string.IsNullOrEmpty(pa.subaction))
                    {
                        tb.Text = string.Format("{0} - {1}", Utils.LanguageSupport.Instance.GetTranslation(pa.action), Utils.LanguageSupport.Instance.GetTranslation(pa.subaction));
                    }
                    else
                    {
                        tb.Text = string.Format("{0}", Utils.LanguageSupport.Instance.GetTranslation(pa.action));
                    }
                    tb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
                    tb.ImageTransparentColor = System.Drawing.Color.Magenta;
                    tb.Click += new EventHandler(mi_Click);
                    tb.Tag = pa;
                    toolStripCustom.Items.Add(tb);
                    _toolbarItems.Add(tb);
                }
            }
            if (pa.plugin.PluginType == Framework.PluginType.Script)
            {
                if (pa.action != "Select Folder" && pa.action != "Refresh" && pa.action != "-")
                {
                    toolStripComboBoxScript.Items.Add(pa);
                }
            }
            else if (pa.plugin.PluginType == Framework.PluginType.Action)
            {
                if (pa.action == "Action builder")
                {
                    if (pa.subaction != "Editor" && pa.subaction != "-")
                    {
                        ActionBuilderPluginAction bpa = new ActionBuilderPluginAction();
                        bpa.action = pa.action;
                        bpa.plugin = pa.plugin;
                        bpa.subaction = pa.subaction;
                        bpa.Tag = pa.Tag;
                        toolStripComboBoxBuilderFlows.Items.Add(bpa);
                    }
                }
                else if (pa.action == "Action sequence")
                {
                    if (pa.subaction != "Edit" && pa.subaction != "-")
                    {
                        ActionSequencePluginAction bpa = new ActionSequencePluginAction();
                        bpa.action = pa.action;
                        bpa.plugin = pa.plugin;
                        bpa.subaction = pa.subaction;
                        bpa.Tag = pa.Tag;
                        toolStripComboBoxActionSequence.Items.Add(bpa);
                    }
                }
            }
        }

        public override void AddPluginAction(PluginAction pa)
        {
            AddPluginActionForToolbar(pa);

            ToolStripItem mi = new ToolStripMenuItem();
            mi.Text = Utils.LanguageSupport.Instance.GetTranslation(pa.action);
            mi.Tag = pa;
            pa.Tag = mi;
            mi.Click += new EventHandler(mi_Click);

            var p = (from s in Core.ShortcutInfo where s.PluginType == pa.plugin.GetType().ToString() && s.PluginAction == pa.action && (s.PluginSubAction ?? "") == (pa.subaction ?? "") select s).FirstOrDefault();
            if (p != null)
            {
                (mi as ToolStripMenuItem).ShortcutKeyDisplayString = p.ShortcutKeyString;
                (mi as ToolStripMenuItem).ShortcutKeys = p.ShortcutKeys;
            }
            var tsp = (from t in _toolbarActionProperties where t.PluginType == pa.plugin.PluginType && t.Action == (pa.action ?? "") && t.SubAction == (pa.subaction ?? "") select t).FirstOrDefault();
            if (tsp != null)
            {
                (mi as ToolStripMenuItem).Image = tsp.ButtonImage;
                (mi as ToolStripMenuItem).ImageScaling = ToolStripItemImageScaling.None;
            }

            ToolStripMenuItem misub = null;
            if (!string.IsNullOrEmpty(pa.subaction))
            {
                mi.Text = Utils.LanguageSupport.Instance.GetTranslation(pa.subaction);

                misub = (from m in _pluginActionList
                         where m.action == pa.action && m.plugin == pa.plugin && string.IsNullOrEmpty(m.subaction)
                         select m.Tag as ToolStripMenuItem).FirstOrDefault();

                if (misub == null)
                {
                    //ok, parent does not exist, create
                    PluginAction parentPa = new PluginAction();
                    parentPa.action = pa.action;
                    parentPa.subaction = null;
                    parentPa.plugin = pa.plugin;
                    _pluginActionList.Add(parentPa);

                    misub = new ToolStripMenuItem();
                    misub.Text = Utils.LanguageSupport.Instance.GetTranslation(pa.action);
                    misub.Tag = pa;
                    parentPa.Tag = misub;
                    misub.DropDownItems.Add(mi);

                    mi = misub;
                }
                else
                {
                    if (mi.Text == "-")
                    {
                        misub.DropDownItems.Add(new ToolStripSeparator());
                        mi.Dispose();
                    }
                    else
                    {
                        misub.DropDownItems.Add(mi);
                    }
                    return;
                }
            }

            if (mi.Text == "-")
            {
                mi.Dispose();
                mi = new ToolStripSeparator();
            }
            switch (pa.plugin.PluginType)
            {
                case Framework.PluginType.InternalStorage:
                    fileToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.LanguageSupport:
                    languageToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.ImportData:
                    importToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.ExportData:
                    exportToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.UIChildWindow:
                    windowToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.GeocacheSelectFilter:
                    searchToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.Action:
                    actionToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.Script:
                    scriptsToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.PluginManager:
                    pluginsToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.Debug:
                    windowToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.LiveAPI:
                    liveAPIToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.OKAPI:
                    oKAPIToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.ImageResource:
                    actionToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.GenericWindow:
                    windowToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.Map:
                    mapToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.Account:
                    fileToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                case Framework.PluginType.GeocacheCollection:
                    windowToolStripMenuItem.DropDownItems.Add(mi);
                    break;
                default:
                    pluginsToolStripMenuItem.DropDownItems.Add(mi);
                    break;
            }
        }

        private async Task startPluginAction(PluginAction pa)
        {
            if (pa != null)
            {
                PrepareCommandExecution(); ;
                try
                {
                    if (string.IsNullOrEmpty(pa.subaction))
                    {
                        await pa.plugin.ActionAsync(pa.action);
                    }
                    else
                    {
                        await pa.plugin.ActionAsync(string.Format("{0}{1}{2}", pa.action, Utils.BasePlugin.Plugin.SubActionSep, pa.subaction));
                    }
                }
                catch(Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                CommandExecutionFinished();
            }
        }

        async void mi_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            if (mi != null)
            {
                await startPluginAction(mi.Tag as PluginAction);
            }
            else if (sender is ToolStripButton)
            {
                await startPluginAction((sender as ToolStripButton).Tag as PluginAction);
            }
        }

        public override void RemovePluginAction(PluginAction pa)
        {
            if (pa != null)
            {
                if (toolStripComboBoxScript.Items.Contains(pa))
                {
                    toolStripComboBoxScript.Items.Remove(pa);
                }
                if (pa.plugin.PluginType == Framework.PluginType.Action && pa.action == "Action builder")
                {
                    for (int i = 0; i < toolStripComboBoxBuilderFlows.Items.Count; i++)
                    {
                        ActionBuilderPluginAction bpa = toolStripComboBoxBuilderFlows.Items[i] as ActionBuilderPluginAction;
                        if (bpa != null)
                        {
                            if (bpa.subaction == pa.subaction)
                            {
                                toolStripComboBoxBuilderFlows.Items.Remove(bpa);
                                break;
                            }
                        }
                    }
                }
                else if (pa.plugin.PluginType == Framework.PluginType.Action && pa.action == "Action sequence")
                {
                    for (int i = 0; i < toolStripComboBoxActionSequence.Items.Count; i++)
                    {
                        ActionSequencePluginAction bpa = toolStripComboBoxActionSequence.Items[i] as ActionSequencePluginAction;
                        if (bpa != null)
                        {
                            if (bpa.subaction == pa.subaction)
                            {
                                toolStripComboBoxActionSequence.Items.Remove(bpa);
                                break;
                            }
                        }
                    }
                }
                ToolStripMenuItem mi = pa.Tag as ToolStripMenuItem;
                if (mi != null)
                {
                    mi.Dispose();
                    pa.Tag = null;
                }
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);

            fileToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILE);
            importToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT);
            exportToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPORT);
            searchToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCH);
            actionToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTION);
            scriptsToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SCRIPTS);
            liveAPIToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LIVEAPI);
            oKAPIToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OKAPI);
            languageToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LANGUAGE);
            originalLanguageToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ORIGINALTEXT);
            pluginsToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLUGINS);
            windowToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WINDOW);
            mapToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAPS);
            helpToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HELP);
            aboutGlobalcachingApplicationToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ABOUT);
            settingsToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETTINGS);
            forumToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FORUM);
            instructionVideosToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INSTRUCTIONVIDEOS);
            changeHistoryToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CHANGEHISTORY);
            UpdateLocations();
            core_ActiveGeocacheChanged(sender, null);
            fileToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_FILE), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            searchToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCH), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            scriptsToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_SCRIPTS), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            toolStripButtonExecute.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTE);
            windowsToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_WINDOW), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            mapsToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_MAPS), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            liveAPIToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_LIVEAPI), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            customToolbarToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CUSTOMTOOLBAR);
            actionToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_ACTION), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            actionSequencerToolbarToolStripMenuItem.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIONSEQUNCER), Utils.LanguageSupport.Instance.GetTranslation(STR_TOOLBAR));
            toolStripButtonExecuteAction.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTE);
            toolStripButtonExecuteActionSequence.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTE);
            menuItemPlugins.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLUGINS);
            if (_exitMeniItem != null)
            {
                _exitMeniItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXIT);
            }

            foreach (ToolStripItem tmi in menuItemPlugins.DropDownItems)
            {
                tmi.Text = Utils.LanguageSupport.Instance.GetTranslation((tmi.Tag as Framework.Interfaces.IPlugin).FriendlyName);
            }

            if (Core.SelectedLanguage != null)
            {
                nederlandstaligeHelpToolStripMenuItem.Visible = Core.SelectedLanguage.LCID == 1043;
            }
            else
            {
                nederlandstaligeHelpToolStripMenuItem.Visible = false;
            }

            foreach (ToolStripMenuItem ti in languageToolStripMenuItem.DropDownItems)
            {
                ti.Checked = false;
            }
            if (Core.SelectedLanguage == System.Globalization.CultureInfo.InvariantCulture)
            {
                originalLanguageToolStripMenuItem.Checked = true;
            }
            else
            {
                /* //not possible yet
                for (int i = 1; i < languageToolStripMenuItem.DropDownItems.Count; i++)
                {
                    ToolStripMenuItem mi = (languageToolStripMenuItem.DropDownItems[i] as ToolStripMenuItem);
                    if (mi != null)
                    {
                        PluginAction pa = (mi.Tag as PluginAction);
                        if (pa != null)
                        {
                            mi.Checked = false;
                        }
                    }
                }
                */
            }

            foreach (PluginAction pa in _pluginActionList)
            {
                if (pa.Tag is ToolStripMenuItem)
                {
                    (pa.Tag as ToolStripMenuItem).Text = string.IsNullOrEmpty(pa.subaction) ? Utils.LanguageSupport.Instance.GetTranslation(pa.action) : Utils.LanguageSupport.Instance.GetTranslation(pa.subaction);
                }
            }
            foreach (ToolStripButton tb in _toolbarItems)
            {
                if (tb.Tag is PluginAction)
                {
                    PluginAction pa = tb.Tag as PluginAction;
                    if (!string.IsNullOrEmpty(pa.subaction))
                    {
                        tb.Text = string.Format("{0} - {1}", Utils.LanguageSupport.Instance.GetTranslation(pa.action), Utils.LanguageSupport.Instance.GetTranslation(pa.subaction));
                    }
                    else
                    {
                        tb.Text = string.Format("{0}", Utils.LanguageSupport.Instance.GetTranslation(pa.action));
                    }                    
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormSettingsTreeView dlg = new FormSettingsTreeView(Core, this.OwnerPlugin))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    oKAPIToolStripMenuItem.Visible = PluginSettings.Instance.ShowOKAPIMenu;
                    SelectedLanguageChanged(this, EventArgs.Empty);
                }
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_allowedToClose)
            {
                Core.PluginAdded -= new Framework.EventArguments.PluginEventHandler(core_PluginAdded);
                Core.PrepareClosingApplication();
                if (_frmProgress != null)
                {
                    _frmProgress.Dispose();
                    _frmProgress = null;
                }
            }
            else
            {
                e.Cancel = true;
                this.BeginInvoke((Action)(() => { _exitMeniItem_Click(null, EventArgs.Empty); }));
            }
        }

        private void originalLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.SelectedLanguage = System.Globalization.CultureInfo.InvariantCulture;
        }

        private void toolStripStatusLabelHomeLocation_DoubleClick(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, Core.HomeLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Core.HomeLocation.SetLocation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void toolStripStatusLabelCenterLocation_DoubleClick(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, Core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Core.CenterLocation.SetLocation(dlg.Result.Lat, dlg.Result.Lon);
                    Core.Geocaches.BeginUpdate();
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);
                    }
                    Core.Geocaches.EndUpdate();
                }
            }
        }


        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            this.Refresh();
            if (_sizeInitialized)
            {
                PluginSettings.Instance.WindowMaximized = (WindowState == FormWindowState.Maximized);
                if (WindowState == FormWindowState.Normal && this.Visible)
                {
                    PluginSettings.Instance.WindowPos = this.Bounds;
                }
            }
        }

        private void aboutGlobalcachingApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.ShowAboutDialog();
        }

        private void toolStripStatusLabelActiveGeocache_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null && !string.IsNullOrEmpty(Core.ActiveGeocache.Url))
            {
                try
                {
                    System.Diagnostics.Process.Start(Core.ActiveGeocache.Url);
                }
                catch
                {
                }
            }
        }

        private void forumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://forum.globalcaching.eu");
            }
            catch
            {
            }
        }


        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array a = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (a != null)
                {
                    PrepareCommandExecution();
                    try
                    {
                        OwnerPlugin.OnDropFile((from string s in a select s).ToArray());
                    }
                    catch
                    {
                    }
                    CommandExecutionFinished();
                }
            }
            catch
            {
            }
        }

        public void PrepareCommandExecution()
        {
            panel1.Enabled = false;
            menuStripMain.Enabled = false;
            if (this.MdiChildren != null)
            {
                foreach (Form frm in this.MdiChildren)
                {
                    frm.Enabled = false;
                }
            }
        }
        public void CommandExecutionFinished()
        {
            if (this.MdiChildren != null)
            {
                foreach (Form frm in this.MdiChildren)
                {
                    frm.Enabled = true;
                }
            }
            menuStripMain.Enabled = true;
            panel1.Enabled = true;
        }

        private void FormMain_LocationChanged(object sender, EventArgs e)
        {
            this.Refresh();
            if (_sizeInitialized)
            {
                PluginSettings.Instance.WindowMaximized = (WindowState == FormWindowState.Maximized);
                if (WindowState == FormWindowState.Normal && this.Visible)
                {
                    PluginSettings.Instance.WindowPos = this.Bounds;
                }
            }
        }

        private void toolStripContainer1_TopToolStripPanel_SizeChanged(object sender, EventArgs e)
        {
            toolStripContainer1.Height = toolStripContainer1.TopToolStripPanel.Height;
        }

        private void fileToolbarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized)
            {
                PluginSettings.Instance.ToolbarFile = fileToolbarToolStripMenuItem.Checked;
                toolStripFile.Visible = PluginSettings.Instance.ToolbarFile && toolStripFile.Items.Count > 0;
            }
        }

        private void searchToolbarToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized)
            {
                PluginSettings.Instance.ToolbarSearch = searchToolbarToolStripMenuItem.Checked;
                toolStripSearch.Visible = PluginSettings.Instance.ToolbarSearch && toolStripSearch.Items.Count > 0;
            }
        }

        private void toolStripFile_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarFileLocation = toolStripFile.Location;
            }
        }

        private void toolStripSearch_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarSearchLocation = toolStripSearch.Location;
            }
        }

        private void toolStripComboBoxScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripButtonExecute.Enabled = toolStripComboBoxScript.SelectedIndex >= 0;
            if (toolStripComboBoxScript.SelectedIndex >= 0)
            {
                toolStripComboBoxScript.ToolTipText = toolStripComboBoxScript.Items[toolStripComboBoxScript.SelectedIndex].ToString();
            }
        }

        private async void toolStripButtonExecute_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxScript.SelectedItem as PluginAction != null)
            {
                await startPluginAction(toolStripComboBoxScript.SelectedItem as PluginAction);
            }
        }

        private void scriptsToolbarToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized)
            {
                PluginSettings.Instance.ToolbarScripts = scriptsToolbarToolStripMenuItem.Checked;
                toolStripScripts.Visible = PluginSettings.Instance.ToolbarScripts && toolStripScripts.Items.Count > 0;
            }
        }

        private void toolStripScripts_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarScriptsLocation = toolStripScripts.Location;
            }
        }

        private void toolStripWindows_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarWindowsLocation = toolStripWindows.Location;
            }
        }

        private void windowsToolbarToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized)
            {
                PluginSettings.Instance.ToolbarWindows = windowsToolbarToolStripMenuItem.Checked;
                toolStripWindows.Visible = PluginSettings.Instance.ToolbarWindows && toolStripWindows.Items.Count > 0;
            }
        }

        private void toolStripMaps_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState!= FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarMapsLocation = toolStripMaps.Location;
            }
        }

        private void mapsToolbarToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized)
            {
                PluginSettings.Instance.ToolbarMaps = mapsToolbarToolStripMenuItem.Checked;
                toolStripMaps.Visible = PluginSettings.Instance.ToolbarMaps && toolStripMaps.Items.Count > 0;
            }
        }

        private void toolStripLiveAPI_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarLiveAPIlocation = toolStripLiveAPI.Location;
            }
        }

        private void liveAPIToolbarToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            PluginSettings.Instance.ToolbarLiveAPI = liveAPIToolbarToolStripMenuItem.Checked;
            toolStripLiveAPI.Visible = PluginSettings.Instance.ToolbarLiveAPI && toolStripLiveAPI.Items.Count>0;
        }

        private void customToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CustomToolbarSettingForm dlg = new CustomToolbarSettingForm(_toolbarActionProperties))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //todo - rebuild Custom toolbar
                    toolStripCustom.Visible = PluginSettings.Instance.ToolbarCustom;
                    foreach (ToolStripButton t in toolStripCustom.Items)
                    {
                        _toolbarItems.Remove(t);
                    }
                    toolStripCustom.Items.Clear();

                    List<ToolStrip> tsl = new List<ToolStrip>();
                    tsl.AddRange(new ToolStrip[] { toolStripFile, toolStripSearch, toolStripScripts, toolStripWindows, toolStripLiveAPI, toolStripMaps });
                    foreach (ToolStrip ts in tsl)
                    {
                        foreach (ToolStripItem tsi in ts.Items)
                        {
                            PluginAction pa = tsi.Tag as PluginAction;
                            if (pa != null)
                            {
                                var tsp = (from t in _toolbarActionProperties where t.PluginType == pa.plugin.PluginType && t.Action == (pa.action ?? "") && t.SubAction == (pa.subaction ?? "") select t).FirstOrDefault();
                                if (tsp != null)
                                {
                                    if (CustomToolbarSettingForm.GetToolbarPropertySelected(tsp))
                                    {
                                        ToolStripButton tb = new ToolStripButton(tsp.ButtonImage);
                                        if (!string.IsNullOrEmpty(pa.subaction))
                                        {
                                            tb.Text = string.Format("{0} - {1}", Utils.LanguageSupport.Instance.GetTranslation(pa.action), Utils.LanguageSupport.Instance.GetTranslation(pa.subaction));
                                        }
                                        else
                                        {
                                            tb.Text = string.Format("{0}", Utils.LanguageSupport.Instance.GetTranslation(pa.action));
                                        }
                                        tb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
                                        tb.ImageTransparentColor = System.Drawing.Color.Magenta;
                                        tb.Click += new EventHandler(mi_Click);
                                        tb.Tag = pa;
                                        toolStripCustom.Items.Add(tb);
                                        _toolbarItems.Add(tb);
                                    }
                                }
                            }
                        }
                    }
                    toolStripCustom.Visible = PluginSettings.Instance.ToolbarCustom && toolStripCustom.Items.Count>0;
                }
            }
        }

        private void toolStripCustom_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarCustomLocation = toolStripCustom.Location;
            }
        }

        private void toolStripActions_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarActionsLocation = toolStripActions.Location;
            }
        }

        private void actionToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.ToolbarActions = actionToolbarToolStripMenuItem.Checked;
            toolStripActions.Visible = PluginSettings.Instance.ToolbarActions;
        }

        private void toolStripComboBoxBuilderFlows_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripButtonExecuteAction.Enabled = toolStripComboBoxBuilderFlows.SelectedIndex >= 0;
            if (toolStripComboBoxBuilderFlows.SelectedIndex >= 0)
            {
                toolStripComboBoxBuilderFlows.ToolTipText = toolStripComboBoxBuilderFlows.Items[toolStripComboBoxBuilderFlows.SelectedIndex].ToString();
            }
        }

        private async void toolStripButtonExecuteAction_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxBuilderFlows.SelectedItem as PluginAction != null)
            {
                await startPluginAction(toolStripComboBoxBuilderFlows.SelectedItem as PluginAction);
            }
        }

        private void instructionVideosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.4geocaching.eu/GAPP/en/Instructionvideos");
            }
            catch
            {
            }
        }

        private void changeHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.4geocaching.eu/GAPP/en/Changehistory");
            }
            catch
            {
            }
        }

        private void nederlandstaligeHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.4geocaching.eu/GAPP/en/Nederlandstaligehelp");
            }
            catch
            {
            }
        }

        private void toolStripComboBoxActionSequence_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripButtonExecuteActionSequence.Enabled = toolStripComboBoxActionSequence.SelectedIndex >= 0;
            if (toolStripComboBoxActionSequence.SelectedIndex >= 0)
            {
                toolStripComboBoxActionSequence.ToolTipText = toolStripComboBoxActionSequence.Items[toolStripComboBoxActionSequence.SelectedIndex].ToString();
            }
        }

        private void toolStripActionSequence_LocationChanged(object sender, EventArgs e)
        {
            if (_sizeInitialized && this.WindowState != FormWindowState.Minimized)
            {
                PluginSettings.Instance.ToolbarActionSequenceLocation = toolStripActionSequence.Location;
            }
        }

        private async void toolStripButtonExecuteActionSequence_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxActionSequence.SelectedItem as PluginAction != null)
            {
                await startPluginAction(toolStripComboBoxActionSequence.SelectedItem as PluginAction);
            }
        }

        private void actionSequencerToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.ToolbarActionSequence = actionSequencerToolbarToolStripMenuItem.Checked;
            toolStripActionSequence.Visible = PluginSettings.Instance.ToolbarActionSequence;
        }

        private void menuStripItem_DropDownOpening(object sender, EventArgs e)
        {
            checkMenuItemEnabled(sender as ToolStripMenuItem);
        }

        private void checkMenuItemEnabled(ToolStripItem ti)
        {
            ToolStripMenuItem mi = ti as ToolStripMenuItem;
            if (mi != null)
            {
                if (mi.DropDownItems != null && mi.DropDownItems.Count > 0)
                {
                    foreach (ToolStripItem m in mi.DropDownItems)
                    {
                        checkMenuItemEnabled(m);
                    }
                }
                else
                {
                    PluginAction pa = mi.Tag as PluginAction;
                    if (pa != null)
                    {
                        string action;
                        if (string.IsNullOrEmpty(pa.subaction))
                        {
                            action = pa.action;
                        }
                        else
                        {
                            action = string.Format("{0}|{1}",pa.action,pa.subaction);
                        }
                        ti.Enabled = pa.plugin.ActionEnabled(action, _selCount, Core.ActiveGeocache != null);
                    }
                }
            }
        }

        private void toolStripSplitButtonTranslate_ButtonClick(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null && !string.IsNullOrEmpty(Core.ActiveGeocache.Url))
            {
                try
                {
                    bool external = true;
                    string url = string.Format("http://www.google.com/translate?hl=en&ie=UTF8&sl=auto&tl={1}&u={0}", System.Web.HttpUtility.UrlEncode(Core.ActiveGeocache.Url), PluginSettings.Instance.SelectedTranslater);

                    try
                    {
                        Utils.BasePlugin.Plugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.GCView.GeocacheViewer") as Utils.BasePlugin.Plugin;
                        if (p != null)
                        {
                            var m = p.GetType().GetMethod("OpenInInternalBrowser");
                            external = !(bool)m.Invoke(p, null);
                        }

                        if (!external)
                        {
                            p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.Browser.BrowserPlugin") as Utils.BasePlugin.Plugin;
                            if (p != null)
                            {
                                var m = p.GetType().GetMethod("OpenNewBrowser");
                                m.Invoke(p, new object[] { url });
                            }
                        }
                    }
                    catch
                    {
                        external = true;
                    }

                    if (external)
                    {
                        System.Diagnostics.Process.Start(url);
                    }
                }
                catch
                {
                }
            }
        }

        private void dutchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem p = sender as ToolStripMenuItem;
            if (p != null && p.Tag != null)
            {
                PluginSettings.Instance.SelectedTranslater = p.Tag.ToString();
                toolStripSplitButtonTranslate.Image = p.Image;
            }
        }
    }

    public class MainFormPlugin : Utils.BasePlugin.BaseUIMainWindow
    {
        protected override Utils.BasePlugin.BaseUIMainWindowForm CreateUIMainWindowForm(Framework.Interfaces.ICore core)
        {
            core.Geocaches.ListDataChanged += new EventHandler(listDataChanged);
            core.Waypoints.ListDataChanged += new EventHandler(listDataChanged);
            core.Logs.ListDataChanged += new EventHandler(listDataChanged);
            core.LogImages.ListDataChanged += new EventHandler(listDataChanged);
            core.GeocacheImages.ListDataChanged += new EventHandler(GeocacheImages_ListDataChanged);

            core.Geocaches.ListSelectionChanged += new EventHandler(listDataChanged);

            core.Geocaches.GeocacheAdded += new Framework.EventArguments.GeocacheEventHandler(Geocaches_Geocache);
            core.Geocaches.GeocacheRemoved += new Framework.EventArguments.GeocacheEventHandler(Geocaches_Geocache);
            core.Geocaches.SelectedChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_Geocache);

            core.Waypoints.WaypointAdded += new Framework.EventArguments.WaypointEventHandler(Waypoints_Waypoint);
            core.Waypoints.WaypointRemoved += new Framework.EventArguments.WaypointEventHandler(Waypoints_Waypoint);

            core.Logs.LogAdded += new Framework.EventArguments.LogEventHandler(Logs_Log);
            core.Logs.LogRemoved += new Framework.EventArguments.LogEventHandler(Logs_Log);

            core.LogImages.LogImageAdded += new Framework.EventArguments.LogImageEventHandler(LogImages_LogImage);
            core.LogImages.LogImageRemoved += new Framework.EventArguments.LogImageEventHandler(LogImages_LogImage);

            core.GeocacheImages.GeocacheImageAdded += new Framework.EventArguments.GeocacheImageEventHandler(GeocacheImages_GeocacheImageAdded);
            core.GeocacheImages.GeocacheImageRemoved += new Framework.EventArguments.GeocacheImageEventHandler(GeocacheImages_GeocacheImageRemoved);

            core.GeocachingComAccountChanged += new Framework.EventArguments.GeocacheComAccountEventHandler(core_GeocachingComAccountChanged);

            core.HomeLocation.Changed += new Framework.EventArguments.LocationEventHandler(Location_Changed);
            core.CenterLocation.Changed += new Framework.EventArguments.LocationEventHandler(Location_Changed);

            return (new FormMain(this, core));
        }

        void GeocacheImages_GeocacheImageRemoved(object sender, Framework.EventArguments.GeocacheImageEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        void GeocacheImages_GeocacheImageAdded(object sender, Framework.EventArguments.GeocacheImageEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        void GeocacheImages_ListDataChanged(object sender, EventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);            
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();
            Framework.Interfaces.IPluginInternalStorage intStr = Core.GetPlugin(Framework.PluginType.InternalStorage).FirstOrDefault() as Framework.Interfaces.IPluginInternalStorage;
            if (intStr != null)
            {
                intStr.DataSourceNameChanged += new Framework.EventArguments.PluginEventHandler(intStr_DataSourceNameChanged);
                (this.MainForm as FormMain).UpdateDataSourceName(intStr);
            }
            (this.MainForm as FormMain).ApplicationInitialized();
        }

        public override void OnCommandLineArguments(string[] args)
        {
            (this.MainForm as FormMain).BringToFront();
            (this.MainForm as FormMain).PrepareCommandExecution();
            try
            {
                base.OnCommandLineArguments(args);
            }
            catch
            {
            }
            (this.MainForm as FormMain).CommandExecutionFinished();
        }

        void intStr_DataSourceNameChanged(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            (this.MainForm as FormMain).UpdateDataSourceName(e.Plugin);
        }

        void Location_Changed(object sender, Framework.EventArguments.LocationEventArgs e)
        {
            if (this.MainForm != null)
            {
                (this.MainForm as FormMain).UpdateLocations();
            }            
        }

        void core_GeocachingComAccountChanged(object sender, Framework.EventArguments.GeocacheComAccountEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        void LogImages_LogImage(object sender, Framework.EventArguments.LogImageEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        void Logs_Log(object sender, Framework.EventArguments.LogEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        void Waypoints_Waypoint(object sender, Framework.EventArguments.WaypointEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        void Geocaches_Geocache(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            listDataChanged(sender, EventArgs.Empty);
        }

        public override void Close()
        {
            Core.Geocaches.ListDataChanged += new EventHandler(listDataChanged);
            Core.Waypoints.ListDataChanged -= new EventHandler(listDataChanged);
            Core.Logs.ListDataChanged -= new EventHandler(listDataChanged);
            Core.LogImages.ListDataChanged -= new EventHandler(listDataChanged);
            Core.GeocacheImages.ListDataChanged -= new EventHandler(GeocacheImages_ListDataChanged);
            Core.Geocaches.ListSelectionChanged -= new EventHandler(listDataChanged);
            Core.Geocaches.GeocacheAdded -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_Geocache);
            Core.Geocaches.GeocacheRemoved -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_Geocache);
            Core.Geocaches.SelectedChanged -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_Geocache);
            Core.Waypoints.WaypointAdded -= new Framework.EventArguments.WaypointEventHandler(Waypoints_Waypoint);
            Core.Waypoints.WaypointRemoved -= new Framework.EventArguments.WaypointEventHandler(Waypoints_Waypoint);
            Core.Logs.LogAdded -= new Framework.EventArguments.LogEventHandler(Logs_Log);
            Core.Logs.LogRemoved -= new Framework.EventArguments.LogEventHandler(Logs_Log);
            Core.LogImages.LogImageAdded -= new Framework.EventArguments.LogImageEventHandler(LogImages_LogImage);
            Core.LogImages.LogImageRemoved -= new Framework.EventArguments.LogImageEventHandler(LogImages_LogImage);
            Core.GeocacheImages.GeocacheImageAdded -= new Framework.EventArguments.GeocacheImageEventHandler(GeocacheImages_GeocacheImageAdded);
            Core.GeocacheImages.GeocacheImageRemoved -= new Framework.EventArguments.GeocacheImageEventHandler(GeocacheImages_GeocacheImageRemoved);
            Core.GeocachingComAccountChanged -= new Framework.EventArguments.GeocacheComAccountEventHandler(core_GeocachingComAccountChanged);
            Core.HomeLocation.Changed -= new Framework.EventArguments.LocationEventHandler(Location_Changed);
            Core.CenterLocation.Changed -= new Framework.EventArguments.LocationEventHandler(Location_Changed);
            base.Close();
        }

        void listDataChanged(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                (this.MainForm as FormMain).UpdateCountStatus();
            }
        }
    }
}
