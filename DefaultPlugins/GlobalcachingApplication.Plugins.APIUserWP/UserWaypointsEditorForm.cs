using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIUserWP
{
    public partial class UserWaypointsEditorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "User Waypoint Editor";
        public const string STR_ERROR = "Error";
        public const string STR_FAILED = "Failed";
        public const string STR_WAYPOINTS = "Waypoints";
        public const string STR_WAYPOINT = "Waypoint";
        public const string STR_ID = "ID";
        public const string STR_LOCATION = "Location";
        public const string STR_DESCRIPTION = "Description";
        public const string STR_DATE = "Date";
        public const string STR_ADDNEW = "Add new";
        public const string STR_SAVE = "Save";
        public const string STR_DELETE = "Delete";
        public const string STR_COPY = "Copy to custom coordinates";

        public class WaypointListItem
        {
            public Framework.Data.UserWaypoint WP { get; set; }
            public override string ToString()
            {
                if (WP == null)
                {
                    return "";
                }
                else
                {
                    return string.Format("{0}", WP.Description ?? "");
                }
            }
        }

        public UserWaypointsEditorForm()
        {
            InitializeComponent();
        }

        public UserWaypointsEditorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            SelectedLanguageChanged(this, EventArgs.Empty);

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
            core.UserWaypoints.UserWaypointRemoved += new Framework.EventArguments.UserWaypointEventHandler(UserWaypoints_UserWaypointRemoved);
            core.UserWaypoints.UserWaypointAdded += new Framework.EventArguments.UserWaypointEventHandler(UserWaypoints_UserWaypointAdded);
            core.UserWaypoints.ListDataChanged += new EventHandler(UserWaypoints_ListDataChanged);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WAYPOINTS);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WAYPOINT);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ID);
            this.label16.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDNEW);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPY);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
        }


        void UserWaypoints_ListDataChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        void UserWaypoints_UserWaypointAdded(object sender, Framework.EventArguments.UserWaypointEventArgs e)
        {
            UpdateView();
        }

        void UserWaypoints_UserWaypointRemoved(object sender, Framework.EventArguments.UserWaypointEventArgs e)
        {
            UpdateView();
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

        private void UserWaypointsEditorForm_FormClosing(object sender, FormClosingEventArgs e)
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

                    List<Framework.Data.UserWaypoint> wpList = Utils.DataAccess.GetUserWaypointsFromGeocache(Core.UserWaypoints, Core.ActiveGeocache.Code);
                    foreach (Framework.Data.UserWaypoint wp in wpList)
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
            if (wpi == null)
            {
                textBox1.Text = "";
                textBox8.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                textBox2.ReadOnly = true;
            }
            else
            {
                textBox1.Text = wpi.WP.ID > 0 ? wpi.WP.ID.ToString() : "";
                textBox8.Text = Utils.Conversion.GetCoordinatesPresentation(wpi.WP.Lat, wpi.WP.Lon);
                textBox2.Text = wpi.WP.Description ?? "";
                textBox3.Text = wpi.WP.Date.ToString();
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = wpi.WP.ID <= 0;
                button5.Enabled = wpi.WP.ID <= 0;
                textBox2.ReadOnly = wpi.WP.ID > 0;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (wpi != null)
            {
                Framework.Data.UserWaypoint wp = wpi.WP;
                Framework.Data.Location l = new Framework.Data.Location(wp.Lat, wp.Lon);
                using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, l))
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        textBox8.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                WaypointListItem wpi = new WaypointListItem();
                wpi.WP = new Framework.Data.UserWaypoint();
                wpi.WP.ID = -1;
                wpi.WP.Description = "Coordinate Override";
                wpi.WP.Lat = Core.ActiveGeocache.Lat;
                wpi.WP.Lon = Core.ActiveGeocache.Lon;
                wpi.WP.Date = DateTime.Now;
                listBox1.Items.Add(wpi);
                listBox1.SelectedItem = wpi;
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (Core.ActiveGeocache != null && wpi != null)
            {
                Core.Geocaches.BeginUpdate();
                Core.ActiveGeocache.CustomLat = wpi.WP.Lat;
                Core.ActiveGeocache.CustomLon = wpi.WP.Lon;
                Core.Geocaches.EndUpdate();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            if (wpi != null)
            {
                if (wpi.WP.ID <= 0)
                {
                    listBox1.Items.Remove(wpi);
                    button1.Enabled = true;
                }
                else
                {
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
                    {
                        try
                        {
                            Utils.API.LiveV6.StatusResponse resp = null;
                            Cursor = Cursors.WaitCursor;
                            try
                            {
                                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(Core))
                                {
                                    resp = api.Client.DeleteUserWaypoint(api.Token, wpi.WP.ID);
                                }
                            }
                            finally
                            {
                                Cursor = Cursors.Default;
                            }
                            if (resp != null)
                            {
                                if (resp.StatusCode == 0)
                                {
                                    Core.UserWaypoints.Remove(wpi.WP);
                                }
                                else
                                {
                                    MessageBox.Show(resp.StatusMessage ?? "", Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                                }
                            }
                            else
                            {
                                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FAILED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                            }
                        }
                        catch
                        {
                            MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FAILED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WaypointListItem wpi = listBox1.SelectedItem as WaypointListItem;
            Framework.Data.Location ll = Utils.Conversion.StringToLocation(textBox8.Text);
            if (Core.ActiveGeocache!=null && wpi != null && wpi.WP.ID <= 0)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
                {
                    try
                    {
                        Utils.API.LiveV6.SaveUserWaypointResponse resp = null;
                        Cursor = Cursors.WaitCursor;
                        try
                        {
                            using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(Core))
                            {
                                var req = new Utils.API.LiveV6.SaveUserWaypointRequest();
                                req.AccessToken = api.Token;
                                req.CacheCode = Core.ActiveGeocache.Code;
                                req.Description = textBox2.Text;
                                req.Latitude = ll.Lat;
                                req.Longitude = ll.Lon;
                                resp = api.Client.SaveUserWaypoint(req);
                            }
                        }
                        finally
                        {
                            Cursor = Cursors.Default;
                        }
                        if (resp != null)
                        {
                            if (resp.Status.StatusCode == 0)
                            {
                                Framework.Data.UserWaypoint wp = Utils.API.Convert.UserWaypoint(Core, resp.NewWaypoint);
                                wp.Saved = false;
                                wpi.WP = wp;
                                Core.UserWaypoints.Add(wp);
                            }
                            else
                            {
                                MessageBox.Show(resp.Status.StatusMessage ?? "", Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                            }
                        }
                        else
                        {
                            MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FAILED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                        }
                    }
                    catch
                    {
                        MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FAILED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                    }
                }
            }
        }
    }
}
