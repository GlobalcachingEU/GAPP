using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.GCEdit
{
    public partial class WPEditorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Waypoint Editor";
        public const string STR_WAYPOINTS = "Waypoints";
        public const string STR_WAYPOINT = "Waypoint";
        public const string STR_ADDNEW = "Add new";
        public const string STR_REMOVE = "Delete";
        public const string STR_SAVE = "Save";
        public const string STR_ID = "ID";
        public const string STR_CODE = "Code";
        public const string STR_TYPE = "Type";
        public const string STR_LOCATION = "Location";
        public const string STR_NAME = "Name";
        public const string STR_COMMENT = "Comment";
        public const string STR_DESCRIPTION = "Description";
        public const string STR_URL = "Url";
        public const string STR_URLNAME = "Url name";

        public class WaypointListItem
        {
            public Framework.Data.Waypoint WP { get; set; }
            public override string ToString()
            {
                if (WP == null)
                {
                    return "";
                }
                else
                {
                    return string.Format("{0}, {1}", WP.Code??"", WP.Description??"");
                }
            }
        }

        public WPEditorForm()
        {
            InitializeComponent();
        }
        public WPEditorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            comboBoxWaypointType1.SetWaypointTypes(core, core.WaypointTypes.ToList());

            SelectedLanguageChanged(this, EventArgs.Empty);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WAYPOINTS);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WAYPOINT);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDNEW);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ID);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.label18.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TYPE);
            this.label16.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMENT);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_URL);
            this.label14.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_URLNAME);
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            UpdateView();
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            UpdateView();
        }

        private void GCEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void UpdateView()
        {
            if (Visible)
            {
                listBox1.Items.Clear();
                if (Core.ActiveGeocache == null)
                {
                    button1.Enabled = false;
                }
                else
                {
                    button1.Enabled = true;
                    Framework.Data.Geocache gc = Core.ActiveGeocache;

                    List<Framework.Data.Waypoint> wpList = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
                    foreach(Framework.Data.Waypoint wp in wpList)
                    {
                        WaypointListItem wpi = new WaypointListItem();
                        wpi.WP = wp;
                        listBox1.Items.Add(wpi);
                    }
                }
                listBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (wpi != null)
            {
                Framework.Data.Waypoint wp = wpi.WP;
                button2.Enabled = true;
                button5.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;

                textBox1.Text = wp.ID ?? "";
                textBox2.Text = wp.Code ?? "";
                textBox3.Text = wp.Name ?? "";
                textBox4.Text = wp.Comment ?? "";
                textBox5.Text = wp.Description ?? "";
                textBox6.Text = wp.Url ?? "";
                textBox7.Text = wp.UrlName ?? "";
                if (wp.Lat != null && wp.Lon != null)
                {
                    textBox8.Text = Utils.Conversion.GetCoordinatesPresentation((double)wp.Lat, (double)wp.Lon);
                }
                else
                {
                    textBox8.Text = "";
                }
                comboBoxWaypointType1.SelectedItem = wp.WPType;

            }
            else
            {
                button2.Enabled = false;
                button5.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;

                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                textBox8.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox8.Text = ""; 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (wpi != null)
            {
                Framework.Data.Waypoint wp = wpi.WP;
                Framework.Data.Location l;
                if (wp.Lat != null && wp.Lon != null)
                {
                    l = new Framework.Data.Location((double)wp.Lat, (double)wp.Lon);
                }
                else
                {
                    l = new Framework.Data.Location(Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon);
                }
                using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, l))
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        textBox8.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (wpi != null)
            {
                Core.Waypoints.Remove(wpi.WP);
                listBox1.Items.Remove(wpi);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                int index = 1;
                string code = string.Format("{0:00}{1}", index, Core.ActiveGeocache.Code.Substring(2));
                while (Utils.DataAccess.GetWaypoint(Core.Waypoints, code) != null)
                {
                    index++;
                    code = string.Format("{0:00}{1}", index, Core.ActiveGeocache.Code.Substring(2));
                }
                WaypointListItem wpi = new WaypointListItem();
                wpi.WP = new Framework.Data.Waypoint();
                wpi.WP.Code = code;
                wpi.WP.Comment = "";
                wpi.WP.DataFromDate = DateTime.Now;
                wpi.WP.Description = "";
                wpi.WP.GeocacheCode = Core.ActiveGeocache.Code;
                wpi.WP.ID = code;
                wpi.WP.Lat = null;
                wpi.WP.Lon = null;
                wpi.WP.Name = "";
                wpi.WP.Saved = false;
                wpi.WP.Time = DateTime.Now;
                wpi.WP.Url = "";
                wpi.WP.UrlName = "";
                wpi.WP.WPType = Core.WaypointTypes[1];
                Core.Waypoints.Add(wpi.WP);

                listBox1.Items.Add(wpi);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (wpi != null)
            {
                Framework.Data.Waypoint wp = wpi.WP;
                wp.BeginUpdate();

                wp.Name = textBox3.Text;
                wp.Comment = textBox4.Text;
                wp.Description = textBox5.Text;
                wp.Url = textBox6.Text;
                wp.UrlName = textBox7.Text;
                Framework.Data.Location l = Utils.Conversion.StringToLocation(textBox8.Text);
                if (l==null)
                {
                    wp.Lat = null;
                    wp.Lon = null;
                }
                else
                {
                    wp.Lat = l.Lat;
                    wp.Lon = l.Lon;
                }
                wp.WPType = comboBoxWaypointType1.SelectedItem as Framework.Data.WaypointType;
                if (wp.WPType == null)
                {
                    wp.WPType = Core.WaypointTypes[0];
                }

                wp.EndUpdate();

                typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, listBox1, new object[] { });
            }
        }

    }
}
