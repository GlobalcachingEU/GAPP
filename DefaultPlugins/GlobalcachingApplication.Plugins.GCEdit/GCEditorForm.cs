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
    public partial class GCEditorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Geocache Editor";
        public const string STR_SAVE = "Save";
        public const string STR_ID = "ID";
        public const string STR_CODE = "Code";
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

        public GCEditorForm()
        {
            InitializeComponent();
        }

        public GCEditorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            comboBoxCacheType1.SetCacheTypes(core, core.GeocacheTypes.ToList());
            comboBoxContainerType1.SetContainerTypes(core, core.GeocacheContainers.ToList());
            comboBox5.Items.AddRange(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" });
            comboBox6.Items.AddRange(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" });

            SelectedLanguageChanged(this, EventArgs.Empty);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ID);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
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
                if (Core.ActiveGeocache == null)
                {
                    button2.Enabled = false;
                    panel2.Enabled = false;
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    textBox8.Text = "";
                    textBox9.Text = "";
                    textBox10.Text = "";
                    textBox11.Text = "";
                    checkBox1.Checked = false;
                    checkBox2.Checked = false;
                    checkBox3.Checked = false;
                    checkBox4.Checked = false;
                    checkBox5.Checked = false;
                    comboBox1.Text = "";
                    comboBox2.Text = "";
                    comboBox3.Text = "";
                    comboBox4.Text = "";
                    comboBoxCacheType1.SelectedIndex = -1;
                    comboBoxContainerType1.SelectedIndex = -1;
                    comboBox5.SelectedIndex = -1;
                    comboBox6.SelectedIndex = -1;
                    numericUpDown1.Value = 0;
                }
                else
                {
                    Framework.Data.Geocache gc = Core.ActiveGeocache;

                    button2.Enabled = true;
                    panel2.Enabled = true;
                    textBox1.Text = gc.Name ?? "";
                    textBox2.Text = gc.ID ?? "";
                    textBox3.Text = gc.Code ?? "";
                    textBox4.Text = Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon);
                    textBox5.Text = gc.Owner ?? "";
                    textBox6.Text = gc.PlacedBy ?? "";
                    textBox7.Text = gc.OwnerId ?? "";
                    textBox8.Text = gc.EncodedHints ?? "";
                    textBox9.Text = gc.Url ?? "";
                    if (gc.ContainsCustomLatLon)
                    {
                        textBox10.Text = Utils.Conversion.GetCoordinatesPresentation((double)gc.CustomLat, (double)gc.CustomLon);
                    }
                    else
                    {
                        textBox10.Text = "";
                    }
                    textBox11.Text = gc.PersonaleNote ?? "";
                    checkBox1.Checked = gc.Available;
                    checkBox2.Checked = gc.Archived;
                    checkBox3.Checked = gc.MemberOnly;
                    checkBox4.Checked = gc.Found;
                    checkBox5.Checked = gc.Locked;
                    dateTimePicker1.Value = gc.PublishedTime;
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange((from Framework.Data.Geocache g in Core.Geocaches  where !string.IsNullOrEmpty(g.Country) orderby g.Country select g.Country).Distinct().ToArray());
                    comboBox1.Text = gc.Country ?? "";
                    comboBox2.Items.Clear();
                    comboBox2.Items.AddRange((from Framework.Data.Geocache g in Core.Geocaches where !string.IsNullOrEmpty(g.State) orderby g.State select g.State).Distinct().ToArray());
                    comboBox2.Text = gc.State ?? "";
                    comboBox3.Items.Clear();
                    comboBox3.Items.AddRange((from Framework.Data.Geocache g in Core.Geocaches where !string.IsNullOrEmpty(g.Municipality) orderby g.Municipality select g.Municipality).Distinct().ToArray());
                    comboBox3.Text = gc.Municipality ?? "";
                    comboBox4.Items.Clear();
                    comboBox4.Items.AddRange((from Framework.Data.Geocache g in Core.Geocaches where !string.IsNullOrEmpty(g.City) orderby g.City select g.City).Distinct().ToArray());
                    comboBox4.Text = gc.City ?? "";
                    comboBoxCacheType1.SelectedItem = gc.GeocacheType;
                    comboBoxContainerType1.SelectedItem = gc.Container;
                    comboBox5.SelectedIndex = comboBox5.Items.IndexOf(gc.Terrain.ToString("0.#").Replace(',','.'));
                    comboBox6.SelectedIndex = comboBox6.Items.IndexOf(gc.Difficulty.ToString("0.#").Replace(',', '.'));
                    numericUpDown1.Value = gc.Favorites;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox4.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, null))
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

        private void button2_Click(object sender, EventArgs e)
        {
            Framework.Data.Geocache gc = Core.ActiveGeocache;
            Framework.Data.Location l;
            gc.BeginUpdate();
            try
            {
                gc.Name = textBox1.Text;
                //gc.ID = textBox2.Text;
                //gc.Code = textBox3.Text;
                l = Utils.Conversion.StringToLocation(textBox4.Text);
                if (l != null)
                {
                    gc.Lat = l.Lat;
                    gc.Lon = l.Lon;
                }
                gc.Owner = textBox5.Text;
                gc.PlacedBy = textBox6.Text;
                gc.OwnerId = textBox7.Text;
                gc.EncodedHints = textBox8.Text;
                gc.Url = textBox9.Text;
                l = Utils.Conversion.StringToLocation(textBox10.Text);
                if (l != null)
                {
                    gc.CustomLat = l.Lat;
                    gc.CustomLon = l.Lon;
                }
                else
                {
                    gc.CustomLat = null;
                    gc.CustomLon = null;
                }
                gc.PersonaleNote = textBox11.Text;
                gc.Available = checkBox1.Checked;
                gc.Archived = checkBox2.Checked;
                gc.MemberOnly = checkBox3.Checked;
                gc.Found = checkBox4.Checked;
                gc.Locked = checkBox5.Checked;
                gc.PublishedTime = dateTimePicker1.Value;
                gc.Country = comboBox1.Text;
                gc.State = comboBox2.Text;
                gc.Municipality = comboBox3.Text;
                gc.City = comboBox4.Text;
                gc.GeocacheType = comboBoxCacheType1.SelectedItem as Framework.Data.GeocacheType;
                gc.Container = comboBoxContainerType1.SelectedItem as Framework.Data.GeocacheContainer;
                gc.Terrain = Utils.Conversion.StringToDouble(comboBox5.SelectedItem.ToString());
                gc.Difficulty = Utils.Conversion.StringToDouble(comboBox6.SelectedItem.ToString());
                gc.Favorites = (int)numericUpDown1.Value;
            }
            catch
            {
            }
            gc.EndUpdate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Framework.Data.Geocache gc = new Framework.Data.Geocache();

            int maxId = int.MaxValue - 411120 - 100;
            bool found = true;
            string gcCode = "";
            while (found)
            {
                maxId--;
                gcCode = Utils.Conversion.GetCacheCodeFromCacheID(maxId);
                found = Utils.DataAccess.GetGeocache(Core.Geocaches, gcCode) != null;
            }
            gc.Code = gcCode;
            gc.ID = maxId.ToString();
            gc.Archived = false;
            gc.Available = true;
            gc.City = "";
            gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, 3);
            gc.Country = "";
            gc.Difficulty = 1.0;
            gc.EncodedHints = "";
            gc.Favorites = 0;
            gc.Flagged = false;
            gc.Found = false;
            gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 2);
            gc.Lat = Core.CenterLocation.Lat;
            gc.Locked = false;
            gc.Lon = Core.CenterLocation.Lon;
            gc.LongDescription = "";
            gc.LongDescriptionInHtml = false;
            gc.MemberOnly = false;
            gc.Municipality = "";
            gc.Name = Utils.LanguageSupport.Instance.GetTranslation(STR_NONAME);
            gc.Notes = "";
            gc.Owner = Core.GeocachingComAccount.AccountName;
            gc.OwnerId = "0";
            gc.PersonaleNote = "";
            gc.PlacedBy = Core.GeocachingComAccount.AccountName;
            gc.PublishedTime = DateTime.Now;
            gc.Saved = false;
            gc.Selected = false;
            gc.ShortDescription = "";
            gc.ShortDescriptionInHtml = false;
            gc.State = "";
            gc.Terrain = 1.0;
            gc.Title = gc.Name;
            gc.Url = "";

            Core.Geocaches.Add(gc);
            Core.ActiveGeocache = gc;
        }
    }
}
