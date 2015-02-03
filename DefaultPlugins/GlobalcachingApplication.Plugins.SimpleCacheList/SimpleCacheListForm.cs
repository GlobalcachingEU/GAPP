using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.SimpleCacheList
{
    public partial class SimpleCacheListForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_CACHETYPE = "Cache type";
        public const string STR_SIMPLECACHELIST = "Simple Cache List";
        public const string STR_SHOWSELECTEDONLY = "Show selected only";
        public const string STR_SHOWFLAGGEDONLY = "Show flagged only";

        public const string STR_VIEWGEOCACHE = "View geocache";
        public const string STR_VIEWGEOCACHEFROMORG = "View geocache from original location";
        public const string STR_CENTERLOCATION = "Center location";
        public const string STR_EDITNOTE = "Edit note";
        public const string STR_SETCOORDS = "Set additional coordinates";
        public const string STR_CLEARCOORDS = "Clear additional coordinates";
        public const string STR_EDITGEOCACHE = "Geocache Editor";
        public const string STR_EDITWAYPOINTS = "Waypoint Editor";
        public const string STR_DELETE = "Delete";
        public const string STR_PRESET = "Preset";

        public const string STR_SELECTED = "Selected";
        public const string STR_FLAGGED = "Flagged";
        public const string STR_TYPE = "Type";

        private Framework.Data.GeocacheCollection _gcCollection = null;
        public static Framework.Interfaces.ICore FixedCore = null;
        public static string[] FixedAttributes = null;
        private int _fixedColumnCount = 0;
        private bool _ignoreListSelectionChanged = false;
        private bool _ignoreGeocacheSelectionChanged = false;
        private List<string> _orgHeader = new List<string>();
        private bool _initializing = true;
        private List<Preset> _presets = new List<Preset>();
        private string _presetFilename = null;

        public SimpleCacheListForm()
        {
            InitializeComponent();
        }
        public SimpleCacheListForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            _initializing = true;
            FixedCore = core;
            InitializeComponent();

            if (PluginSettings.Instance.WindowPos != null && !PluginSettings.Instance.WindowPos.IsEmpty)
            {
                this.Bounds = PluginSettings.Instance.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SIMPLECACHELIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_BACKGROUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ARCHIVED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_AVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_FOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_OWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_NOTAVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DEFERREDSCROLLING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ENABLEAUTOSORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SHOWSELECTEDONLY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SHOWFLAGGEDONLY));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_VIEWGEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_VIEWGEOCACHEFROMORG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CENTERLOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EDITNOTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SETCOORDS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CLEARCOORDS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EDITGEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EDITWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_PRESET));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TYPE));

            for (int i = 0; i < cacheListControl1.GeocacheDataGrid.Columns.Count; i++)
            {
                if (cacheListControl1.GeocacheDataGrid.Columns[i].Header as string == "Test")
                {
                    _fixedColumnCount = i;
                    break;
                }
                else if (!string.IsNullOrEmpty(cacheListControl1.GeocacheDataGrid.Columns[i].Header as string))
                {
                    core.LanguageItems.Add(new Framework.Data.LanguageItem(cacheListControl1.GeocacheDataGrid.Columns[i].Header as string));
                }
                _orgHeader.Add(cacheListControl1.GeocacheDataGrid.Columns[i].Header as string);
            }

            core.SelectedLanguageChanged += new EventHandler(core_SelectedLanguageChanged);
            core_SelectedLanguageChanged(this, EventArgs.Empty);

            _gcCollection = Core.Geocaches;
            _gcCollection.GeocacheAdded += new Framework.EventArguments.GeocacheEventHandler(_gcCollection_GeocacheAdded);
            _gcCollection.GeocacheRemoved += new Framework.EventArguments.GeocacheEventHandler(_gcCollection_GeocacheRemoved);
            _gcCollection.GeocacheSelectedChanged += new Framework.EventArguments.GeocacheEventHandler(_gcCollection_GeocacheSelectedChanged);
            _gcCollection.DataChanged += new Framework.EventArguments.GeocacheEventHandler(_gcCollection_DataChanged);
            _gcCollection.ListDataChanged += new EventHandler(_gcCollection_ListDataChanged);
            _gcCollection.ListSelectionChanged += new EventHandler(_gcCollection_ListSelectionChanged);
            _gcCollection.SelectedChanged += new Framework.EventArguments.GeocacheEventHandler(_gcCollection_SelectedChanged);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.GeocachingComAccountChanged += new Framework.EventArguments.GeocacheComAccountEventHandler(core_GeocachingComAccountChanged);
            core.UserWaypoints.ListDataChanged += new EventHandler(UserWaypoints_ListDataChanged);
            core.UserWaypoints.UserWaypointAdded += new Framework.EventArguments.UserWaypointEventHandler(UserWaypoints_UserWaypointAdded);
            core.UserWaypoints.UserWaypointRemoved += new Framework.EventArguments.UserWaypointEventHandler(UserWaypoints_UserWaypointRemoved);
            core.Logs.ListDataChanged += new EventHandler(Logs_ListDataChanged);
            core.GeocachingAccountNames.Changed += new Framework.EventArguments.GeocachingAccountNamesEventHandler(GeocachingAccountNames_Changed);

            cacheListControl1.InitBrushes();
            cacheListControl1.GeocacheDataGrid.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(GeocacheDataGrid_SelectionChanged);

            if (!string.IsNullOrEmpty(PluginSettings.Instance.VisibleColumns))
            {
                string[] parts = PluginSettings.Instance.VisibleColumns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < _fixedColumnCount; i++)
                {
                    cacheListControl1.GeocacheDataGrid.Columns[i].Visibility = parts.Contains(i.ToString()) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                }
            }

            if (!string.IsNullOrEmpty(PluginSettings.Instance.ColumnOrder))
            {
                try
                {
                    string[] order = PluginSettings.Instance.ColumnOrder.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < order.Length; i++)
                    {
                        cacheListControl1.GeocacheDataGrid.Columns[i].DisplayIndex = int.Parse(order[i]);
                    }
                }
                catch
                {
                }
            }

            _presetFilename = System.IO.Path.Combine(core.PluginDataPath, "clpresets.xml");
            _presets = Preset.LoadFromFile(_presetFilename);
            comboBox1.Items.AddRange(_presets.ToArray());

            _initializing = false;
            cacheListControl1.OnMouseEnter += new EventHandler<EventArgs>(cacheListControl1_OnMouseEnter);
        }

        void GeocachingAccountNames_Changed(object sender, Framework.EventArguments.GeocachingAccountNamesEventArgs e)
        {
            UpdateList();
        }

        void cacheListControl1_OnMouseEnter(object sender, EventArgs e)
        {
            if (PluginSettings.Instance.AutoTopPanel && panel3.Visible)
            {
                button1_Click(sender, e);
            }
        }

        void Logs_ListDataChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        void UserWaypoints_UserWaypointRemoved(object sender, Framework.EventArguments.UserWaypointEventArgs e)
        {
            UpdateList();
        }

        void UserWaypoints_UserWaypointAdded(object sender, Framework.EventArguments.UserWaypointEventArgs e)
        {
            UpdateList();
        }

        void UserWaypoints_ListDataChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        void _gcCollection_GeocacheSelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.ContainsFocus)
            {
                //ignore
                _ignoreGeocacheSelectionChanged = true;
            }
        }

        internal void SettingsUpdated()
        {
            cacheListControl1.InitBrushes();
            UpdateList();
        }

        void core_GeocachingComAccountChanged(object sender, Framework.EventArguments.GeocacheComAccountEventArgs e)
        {
            UpdateList();
        }

        void GeocacheDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_ignoreListSelectionChanged && this.Enabled)
            {
                Core.ActiveGeocache = cacheListControl1.GeocacheDataGrid.SelectedItem as Framework.Data.Geocache;
            }
            _ignoreGeocacheSelectionChanged = false;
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Core.ActiveGeocache == null)
            {
                cacheListControl1.GeocacheDataGrid.SelectedItem = null;
            }
            else
            {
                if (cacheListControl1.GeocacheDataGrid.Items.Contains(Core.ActiveGeocache))
                {
                    cacheListControl1.GeocacheDataGrid.SelectedItem = Core.ActiveGeocache;
                    cacheListControl1.GeocacheDataGrid.ScrollIntoView(cacheListControl1.GeocacheDataGrid.SelectedItem);
                }
            }
        }

        void _gcCollection_SelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (!_ignoreGeocacheSelectionChanged)
            {
                UpdateList();
            }
            _ignoreGeocacheSelectionChanged = false;
        }

        void core_SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SIMPLECACHELIST);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PRESET);
            this.checkBoxShowFlaggedOnly.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWFLAGGEDONLY);
            this.checkBoxShowSelectedOnly.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWSELECTEDONLY);

            for (int i = 0; i < _orgHeader.Count; i++)
            {
                if (!string.IsNullOrEmpty(_orgHeader[i]))
                {
                    cacheListControl1.GeocacheDataGrid.Columns[i].Header = Utils.LanguageSupport.Instance.GetTranslation(_orgHeader[i]);
                }
            }
            var style = new System.Windows.Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new System.Windows.Setter(System.Windows.Controls.ToolTipService.ToolTipProperty, Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTED)));
            cacheListControl1.GeocacheDataGrid.Columns[0].HeaderStyle = style;

            style = new System.Windows.Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new System.Windows.Setter(System.Windows.Controls.ToolTipService.ToolTipProperty, Utils.LanguageSupport.Instance.GetTranslation(STR_FLAGGED)));
            cacheListControl1.GeocacheDataGrid.Columns[1].HeaderStyle = style;

            style = new System.Windows.Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new System.Windows.Setter(System.Windows.Controls.ToolTipService.ToolTipProperty, Utils.LanguageSupport.Instance.GetTranslation(STR_TYPE)));
            cacheListControl1.GeocacheDataGrid.Columns[2].HeaderStyle = style;

            viewGeocacheFromOriginalLocationToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_VIEWGEOCACHEFROMORG);
            viewGeocacheToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_VIEWGEOCACHE);
            editNoteToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EDITNOTE);
            setAdditionalCoordsToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETCOORDS);
            clearAdditionalCoordsToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARCOORDS);
            setCenterLocationToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CENTERLOCATION);
            geocacheEditorToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EDITGEOCACHE);
            waypointEditorToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EDITWAYPOINTS);
            deleteGeocacheToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);

            checkedListBoxVisibleColumns.Items.Clear();
            int index = 0;
            foreach (System.Windows.Controls.DataGridColumn col in cacheListControl1.GeocacheDataGrid.Columns)
            {
                if (string.IsNullOrEmpty(col.Header as string))
                {
                    checkedListBoxVisibleColumns.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_CACHETYPE), col.Visibility == System.Windows.Visibility.Visible);
                }
                else
                {
                    checkedListBoxVisibleColumns.Items.Add(col.Header, col.Visibility== System.Windows.Visibility.Visible);
                }
                index++;
                if (index >= _fixedColumnCount)
                {
                    break;
                }
            }
        }


        private void AddGeocacheToList(Framework.Data.Geocache gc)
        {
            UpdateList();
        }

        private void SimpleCacheListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                Core.SelectedLanguageChanged -= new EventHandler(core_SelectedLanguageChanged);
            }
        }

        public void UpdateList()
        {
            if (this.Visible)
            {
                _ignoreListSelectionChanged = true;

                if (checkBoxShowSelectedOnly.Checked || checkBoxShowFlaggedOnly.Checked)
                {
                    bool selOnly = checkBoxShowSelectedOnly.Checked;
                    bool flagOnly = checkBoxShowFlaggedOnly.Checked;
                    var gcList = from Framework.Data.Geocache wp in Core.Geocaches
                                 where (!selOnly || wp.Selected) && (!flagOnly || wp.Flagged)
                                 select wp;

                    cacheListControl1.UpdateDataGrid(gcList.ToArray());
                }
                else
                {
                    cacheListControl1.UpdateDataGrid(Core.Geocaches);
                }

                _ignoreListSelectionChanged = false;
                core_ActiveGeocacheChanged(this, null);
            }
        }

        private void SimpleCacheListForm_Shown(object sender, EventArgs e)
        {
            cacheListControl1.UpdateDataGrid(Core.Geocaches);
        }

        void _gcCollection_ListSelectionChanged(object sender, EventArgs e)
        {
            if (!_ignoreGeocacheSelectionChanged)
            {
                UpdateList();
            }
            _ignoreGeocacheSelectionChanged = false;
        }

        void _gcCollection_ListDataChanged(object sender, EventArgs e)
        {
            //check custom columns
            FixedAttributes = Core.Geocaches.CustomAttributes;
            //assupmtion: the order does not change!
            for (int i = 0; (i < FixedAttributes.Length && _fixedColumnCount + i < cacheListControl1.GeocacheDataGrid.Columns.Count); i++)
            {
                cacheListControl1.GeocacheDataGrid.Columns[_fixedColumnCount + i].Header = FixedAttributes[i];
                cacheListControl1.GeocacheDataGrid.Columns[_fixedColumnCount+i].Visibility = System.Windows.Visibility.Visible;
            }
            for (int i = _fixedColumnCount + FixedAttributes.Length; i < cacheListControl1.GeocacheDataGrid.Columns.Count; i++)
            {
                cacheListControl1.GeocacheDataGrid.Columns[i].Visibility = System.Windows.Visibility.Hidden;
            }
            UpdateList();
        }

        void _gcCollection_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            UpdateList();
        }

        void _gcCollection_GeocacheRemoved(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            UpdateList();
        }

        void _gcCollection_GeocacheAdded(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            UpdateList();
        }

        private void checkBoxShowSelectedOnly_CheckedChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void SimpleCacheListForm_EnabledChanged(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                //restore selection, weird I know
                if (Core.ActiveGeocache == null || cacheListControl1.GeocacheDataGrid.Items.Contains(Core.ActiveGeocache))
                {
                    cacheListControl1.GeocacheDataGrid.SelectedItem = Core.ActiveGeocache;
                }
                else
                {
                    cacheListControl1.GeocacheDataGrid.SelectedItem = null;
                }
            }
        }

        private void checkedListBoxVisibleColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue== CheckState.Checked)
            {
                cacheListControl1.GeocacheDataGrid.Columns[e.Index].Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                cacheListControl1.GeocacheDataGrid.Columns[e.Index].Visibility = System.Windows.Visibility.Hidden;
            }
            if (!_initializing)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _fixedColumnCount; i++)
                {
                    if (cacheListControl1.GeocacheDataGrid.Columns[i].Visibility == System.Windows.Visibility.Visible)
                    {
                        sb.Append(i.ToString());
                        sb.Append(",");
                    }
                }
                PluginSettings.Instance.VisibleColumns = sb.ToString();
            }
        }

        private async void viewGeocacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await Utils.PluginSupport.ExecuteDefaultActionAsync(Core, "GlobalcachingApplication.Plugins.GCView.GeocacheViewer");
        }

        private void checkBoxShowFlaggedOnly_CheckedChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private async void editNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await Utils.PluginSupport.ExecuteDefaultActionAsync(Core, "GlobalcachingApplication.Plugins.NoteEdit.GeocacheNoteEditor");
        }

        private void setCenterLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                Core.CenterLocation.SetLocation(Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon);
                Core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache gc in Core.Geocaches)
                {
                    Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);
                }
                Core.Geocaches.EndUpdate();
            }
        }

        private void buttonFilter_Click(object sender, EventArgs e)
        {
            cacheListControl1.FilterOnText = textBoxFilter.Text;
        }

        private void textBoxFilter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                buttonFilter_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void SimpleCacheListForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void SimpleCacheListForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void setAdditionalCoordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                Framework.Data.Location l = new Framework.Data.Location();
                if (Core.ActiveGeocache.ContainsCustomLatLon)
                {
                    l.SetLocation((double)Core.ActiveGeocache.CustomLat, (double)Core.ActiveGeocache.CustomLon);
                }
                else
                {
                    l.SetLocation(Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon);
                }
                using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, l))
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Core.ActiveGeocache.BeginUpdate();
                        Core.ActiveGeocache.CustomLat = dlg.Result.Lat;
                        Core.ActiveGeocache.CustomLon = dlg.Result.Lon;
                        Core.ActiveGeocache.EndUpdate();
                    }
                }
            }
        }

        private void clearAdditionalCoordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                Core.ActiveGeocache.BeginUpdate();
                Core.ActiveGeocache.CustomLat = null;
                Core.ActiveGeocache.CustomLon = null;
                Core.ActiveGeocache.EndUpdate();
            }
        }

        private void viewGeocacheFromOriginalLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Core.ActiveGeocache != null && Core.ActiveGeocache.Url != null && Core.ActiveGeocache.Url.ToLower().StartsWith("http"))
                {
                    System.Diagnostics.Process.Start(Core.ActiveGeocache.Url);
                }
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (panel3.Visible)
            {
                panel3.Visible = false;
                button1.Text = "v";
                panel1.Height = 30;
            }
            else
            {
                panel3.Visible = true;
                button1.Text = "^";
                panel1.Height = 114;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Core.Geocaches.BeginUpdate();
            if (cacheListControl1.GeocacheDataGrid.SelectedItems != null)
            {
                foreach (Framework.Data.Geocache gc in cacheListControl1.GeocacheDataGrid.SelectedItems)
                {
                    gc.Selected = !gc.Selected;
                }
            }
            _ignoreGeocacheSelectionChanged = false;
            Core.Geocaches.EndUpdate();
        }

        public void SaveSettings()
        {
            int index = _orgHeader.IndexOf("Name");
            if (index >= 0)
            {
                PluginSettings.Instance.ColumnNameWidth = (int)cacheListControl1.GeocacheDataGrid.Columns[index].Width.DisplayValue;
            }
            index = _orgHeader.IndexOf("Owner");
            if (index >= 0)
            {
                PluginSettings.Instance.ColumnOwnerWidth = (int)cacheListControl1.GeocacheDataGrid.Columns[index].Width.DisplayValue;
            }
            index = _orgHeader.IndexOf("Country");
            if (index >= 0)
            {
                PluginSettings.Instance.ColumnCountryWidth = (int)cacheListControl1.GeocacheDataGrid.Columns[index].Width.DisplayValue;
            }
            index = _orgHeader.IndexOf("State");
            if (index >= 0)
            {
                PluginSettings.Instance.ColumnStateWidth = (int)cacheListControl1.GeocacheDataGrid.Columns[index].Width.DisplayValue;
            }
            index = _orgHeader.IndexOf("City");
            if (index >= 0)
            {
                PluginSettings.Instance.ColumnCityWidth = (int)cacheListControl1.GeocacheDataGrid.Columns[index].Width.DisplayValue;
            }
            for (int i = 0; i < cacheListControl1.GeocacheDataGrid.Columns.Count; i++)
            {
                if (cacheListControl1.GeocacheDataGrid.Columns[i].SortDirection != null)
                {
                    PluginSettings.Instance.SortOnColumnIndex = i;
                    PluginSettings.Instance.SortDirection = cacheListControl1.GeocacheDataGrid.Columns[i].SortDirection == ListSortDirection.Ascending ? 0 : 1;
                }
            }

        }

        private async void geocacheEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await Utils.PluginSupport.ExecuteDefaultActionAsync(Core, "GlobalcachingApplication.Plugins.GCEdit.GCEditor");
        }

        private async void waypointEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await Utils.PluginSupport.ExecuteDefaultActionAsync(Core, "GlobalcachingApplication.Plugins.GCEdit.WPEditor");
        }

        private async void deleteGeocacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.QuickAc.Actions");
                if (p != null)
                {
                    await p.ActionAsync("Delete|Active");
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = comboBox1.SelectedIndex >= 0;
            button4.Enabled = comboBox1.SelectedIndex >= 0;
            Preset p = comboBox1.SelectedItem as Preset;
            if (p!=null)
            {
                applyPreset(p);
            }
        }

        private void applyPreset(Preset p)
        {
            _initializing = true;

            PluginSettings.Instance.VisibleColumns = p.VisibleColumns;
            PluginSettings.Instance.ColumnOrder = p.ColumnOrder;
            if (!string.IsNullOrEmpty(PluginSettings.Instance.VisibleColumns))
            {
                string[] parts = PluginSettings.Instance.VisibleColumns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < _fixedColumnCount; i++)
                {
                    cacheListControl1.GeocacheDataGrid.Columns[i].Visibility = parts.Contains(i.ToString()) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                    checkedListBoxVisibleColumns.SetItemCheckState(i, cacheListControl1.GeocacheDataGrid.Columns[i].Visibility == System.Windows.Visibility.Visible ? CheckState.Checked : CheckState.Unchecked);
                }
            }
            if (!string.IsNullOrEmpty(PluginSettings.Instance.ColumnOrder))
            {
                try
                {
                    string[] order = PluginSettings.Instance.ColumnOrder.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < order.Length; i++)
                    {
                        cacheListControl1.GeocacheDataGrid.Columns[i].DisplayIndex = int.Parse(order[i]);
                    }
                }
                catch
                {
                }
            }
            _initializing = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button5.Enabled = textBox1.Text.Length > 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Preset p = (from a in _presets where a.Name==textBox1.Text select a).FirstOrDefault();
            if (p == null)
            {
                p = Preset.FromCurrentSettings(p, textBox1.Text);
                _presets.Add(p);
                comboBox1.Items.Add(p);
            }
            else
            {
                p = Preset.FromCurrentSettings(p, textBox1.Text);
            }
            Preset.SaveToFile(_presetFilename, _presets);
            textBox1.Text = "";
            comboBox1.SelectedItem = p;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Preset p = comboBox1.SelectedItem as Preset;
            if (p != null)
            {
                applyPreset(p);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Preset p = comboBox1.SelectedItem as Preset;
            if (p != null)
            {
                _presets.Remove(p);
                comboBox1.Items.Remove(p);
                Preset.SaveToFile(_presetFilename, _presets);
                comboBox1_SelectedIndexChanged(sender, e);
            }
        }

        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            if (PluginSettings.Instance.AutoTopPanel && !panel3.Visible)
            {
                button1_Click(sender, e);
            }
        }

    }

    public class SimpleCacheList : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Simple cache list";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SHOW);
            return await base.InitializeAsync(core);
        }

        public override void ApplicationClosing()
        {
            if (UIChildWindowForm != null)
            {
                (UIChildWindowForm as SimpleCacheListForm).SaveSettings();
            }
            base.ApplicationClosing();
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new SimpleCacheListForm(this, core));
        }

        protected override void InitUIMainWindow(Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin)
        {
            base.InitUIMainWindow(mainWindowPlugin);
            Action(ACTION_SHOW);
        }

        public override string FriendlyName
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            UIChildWindowForm.Show();
                            (UIChildWindowForm as SimpleCacheListForm).UpdateList();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }

        public override bool ApplySettings(List<UserControl> configPanels)
        {
            foreach (var uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    PluginSettings.Instance.BkColorArchived = (uc as SettingsPanel).ArchivedBkColor;
                    PluginSettings.Instance.BkColorAvailable = (uc as SettingsPanel).AvailableBkColor;
                    PluginSettings.Instance.BkColorNotAvailable = (uc as SettingsPanel).NotAvailableBkColor;
                    PluginSettings.Instance.BkColorFound = (uc as SettingsPanel).FoundBkColor;
                    PluginSettings.Instance.BkColorOwned = (uc as SettingsPanel).OwnBkColor;
                    PluginSettings.Instance.BkColorExtraCoord = (uc as SettingsPanel).ExtraCoordBkColor;
                    PluginSettings.Instance.DeferredScrolling = (uc as SettingsPanel).DeferredScrolling;
                    PluginSettings.Instance.EnableAutomaticSorting = (uc as SettingsPanel).EnableAutomaticSorting;
                    PluginSettings.Instance.AutoTopPanel = (uc as SettingsPanel).AutoTopPanel;

                    (UIChildWindowForm as SimpleCacheListForm).SettingsUpdated();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

    }
}
