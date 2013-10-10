using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Framework.Data;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public partial class WaypointSelectorForm : Form
    {
        private ICore _theCore;

        private class WaypointListItem
        {
            public Framework.Data.Geocache GC { get; set; }
            public Framework.Data.Waypoint WP { get; set; }
            public bool isGC() { return (GC != null); }
            public override string ToString()
            {
                if (GC != null)
                {
                    return string.Format("{0}, {1}", GC.Code ?? "", GC.Name ?? "");
                }
                else if (WP != null) {
                    return string.Format("{0}, {1}", WP.Code ?? "", WP.Description ?? "");
                }
                else {
                    return "";
                }
            }
        }

        public WaypointSelectorForm(ICore core)
        {
            _theCore = core;
            InitializeComponent();
            SetLanguageSpecificControlText();
            UpdateView();
            lbWaypoints.SelectedIndex = (lbWaypoints.Items.Count > 0) ? 0 : -1;
        }

        private void SetLanguageSpecificControlText()
        {
            this.Text = StrRes.GetString(StrRes.STR_WPSEL_TITLE);
            lblWaypoint.Text = StrRes.GetString(StrRes.STR_WPSEL_WAYPOINTS);
            bnInsert.Text = StrRes.GetString(StrRes.STR_WPSEL_INSERT);
            bnCancel.Text = StrRes.GetString(StrRes.STR_WPSEL_CANCEL);
        }

        private void UpdateView() 
        {
            if (_theCore.ActiveGeocache != null)
            {
                lbWaypoints.Items.Clear();
                lbWaypoints.Items.Add(new WaypointListItem() { GC = _theCore.ActiveGeocache });
                List<Waypoint> wpts =
                    Utils.DataAccess.GetWaypointsFromGeocache(_theCore.Waypoints, _theCore.ActiveGeocache.Code);
                foreach (Waypoint w in wpts)
                {
                    if ((w.Lat != null) && (w.Lon != null))
                    {
                        lbWaypoints.Items.Add(new WaypointListItem() { WP = w });
                    }
                }
            }
        }

        private void lbWaypoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            bnInsert.Enabled = (lbWaypoints.SelectedIndex > -1);
        }

        public string WaypointName
        {
            get
            {
                WaypointListItem wp = (WaypointListItem)lbWaypoints.SelectedItem;
                if (wp != null)
                {
                    return (wp.isGC()) ? wp.GC.Code : wp.WP.Code;
                };
                return "";
            }
            private set
            {
            }
        }

        private void bnInsert_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lbWaypoints_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lbWaypoints.SelectedItem != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
