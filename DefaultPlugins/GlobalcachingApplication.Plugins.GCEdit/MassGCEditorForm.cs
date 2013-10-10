using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCEdit
{
    public partial class MassGCEditorForm : Form
    {
        public const string STR_TITLE = "Mass Geocache Editor";
        public const string STR_NAME = "Name";
        public const string STR_PUBLISHED = "Published";
        public const string STR_COORDINATE = "Coordinate";
        public const string STR_CUSTOMCOORD = "Custom coord.";
        public const string STR_AVAILABLE = "Available";
        public const string STR_ARCHIVED = "Archived";
        public const string STR_MEMBERONY = "Member only";
        public const string STR_FOUND = "Found";
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_MUNICIPALITY = "Municipality";
        public const string STR_CITY = "City";
        public const string STR_CACHETYPE = "Geocache type";
        public const string STR_PLACEDBY = "Placed by";
        public const string STR_OWNER = "Owner";
        public const string STR_OWNERID = "Owner id";
        public const string STR_CONTAINER = "Container";
        public const string STR_TERRAIN = "Terrain";
        public const string STR_DIFFICULTY = "Difficulty";
        public const string STR_URL = "Url";
        public const string STR_HINTS = "Hints";
        public const string STR_FAVORITES = "Favorites";
        public const string STR_PERSONALENOTES = "Personal notes";
        public const string STR_LOCKED = "Locked";
        public const string STR_NEW = "New";
        public const string STR_NONAME = "Default Name";

        private Framework.Interfaces.ICore _core = null;
        private List<Framework.Data.Geocache> _gcList = null;

        public MassGCEditorForm()
        {
            InitializeComponent();
        }

        public MassGCEditorForm(Framework.Interfaces.ICore core, List<Framework.Data.Geocache> gcList)
            : this()
        {
            _core = core;
            _gcList = gcList;

            comboBoxCacheType1.SetCacheTypes(core, (from a in core.GeocacheTypes where a.ID>0 select a).ToList());
            comboBoxContainerType1.SetContainerTypes(core, (from a in core.GeocacheContainers where a.ID > 0 select a).ToList());
            comboBox5.Items.AddRange(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" });
            comboBox6.Items.AddRange(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" });
            comboBox1.Items.AddRange((from a in _gcList select a.Country??"").Distinct().OrderBy(x=>x).ToArray());
            comboBox2.Items.AddRange((from a in _gcList select a.State ?? "").Distinct().OrderBy(x => x).ToArray());
            comboBox3.Items.AddRange((from a in _gcList select a.Municipality ?? "").Distinct().OrderBy(x => x).ToArray());
            comboBox4.Items.AddRange((from a in _gcList select a.City ?? "").Distinct().OrderBy(x => x).ToArray());

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PUBLISHED);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COORDINATE);
            this.label43.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CUSTOMCOORD);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVAILABLE);
            this.label14.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ARCHIVED);
            this.label41.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MEMBERONY);
            this.label49.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOUND);
            this.label16.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRY);
            this.label18.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STATE);
            this.label20.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MUNICIPALITY);
            this.label22.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CITY);
            this.label24.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CACHETYPE);
            this.label28.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLACEDBY);
            this.label26.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OWNER);
            this.label30.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OWNERID);
            this.label32.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONTAINER);
            this.label34.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TERRAIN);
            this.label36.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DIFFICULTY);
            this.label39.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_URL);
            this.label45.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FAVORITES);
            this.label47.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PERSONALENOTES);
            this.label37.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HINTS);
            this.label51.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCKED);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Name = textBox1.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.PublishedTime = dateTimePicker1.Value;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Framework.Data.Location l = Utils.Conversion.StringToLocation(textBox4.Text);
            if (l != null)
            {
                _core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.Lat = l.Lat;
                    g.Lon = l.Lon;
                }
                _core.Geocaches.EndUpdate();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox4.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox10.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox10.Text = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            Framework.Data.Location l = Utils.Conversion.StringToLocation(textBox10.Text);
            if (l != null)
            {
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.CustomLat = l.Lat;
                    g.CustomLon = l.Lon;
                }
            }
            else
            {
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.CustomLat = null;
                    g.CustomLon = null;
                }
            }
            _core.Geocaches.EndUpdate();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Locked = checkBox5.Checked;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Available = checkBox1.Checked;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Archived = checkBox2.Checked;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.MemberOnly = checkBox3.Checked;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Found = checkBox4.Checked;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Country = comboBox1.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.State = comboBox2.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Municipality = comboBox3.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.City = comboBox4.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Framework.Data.GeocacheType gt = comboBoxCacheType1.SelectedItem as Framework.Data.GeocacheType;
            if (gt != null)
            {
                _core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.GeocacheType = gt;
                }
                _core.Geocaches.EndUpdate();
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.PlacedBy = textBox6.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Owner = textBox5.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.OwnerId = textBox7.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            Framework.Data.GeocacheContainer gt = comboBoxContainerType1.SelectedItem as Framework.Data.GeocacheContainer;
            if (gt != null)
            {
                _core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.Container = gt;
                }
                _core.Geocaches.EndUpdate();
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (comboBox5.SelectedItem != null)
            {
                double d = Utils.Conversion.StringToDouble(comboBox5.SelectedItem.ToString());
                _core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.Terrain = d;
                }
                _core.Geocaches.EndUpdate();
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (comboBox6.SelectedItem != null)
            {
                double d = Utils.Conversion.StringToDouble(comboBox6.SelectedItem.ToString());
                _core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache g in _gcList)
                {
                    g.Difficulty = d;
                }
                _core.Geocaches.EndUpdate();
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.EncodedHints = textBox8.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button25_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Url = textBox9.Text;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.Favorites = (int)numericUpDown1.Value;
            }
            _core.Geocaches.EndUpdate();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache g in _gcList)
            {
                g.PersonaleNote = textBox11.Text;
            }
            _core.Geocaches.EndUpdate();
        }
    }
}
