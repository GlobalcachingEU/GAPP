using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.FilterEx
{
    public partial class GeocacheSearchForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Select geocaches";
        public const string STR_NEWSEARCH = "New search";
        public const string STR_SEARCHWITHINSELECTION = "Search within selection";
        public const string STR_ADDTOSELECTION = "Add to current selection";
        public const string STR_LOCATION = "Location";
        public const string STR_CITYCOORD = "City / Coordinates";
        public const string STR_RADIUS = "Radius";
        public const string STR_CHECK = "Check";
        public const string STR_KM = "km";
        public const string STR_MILES = "miles";
        public const string STR_GEOCACHETYPE = "Geocache type";
        public const string STR_CONTAINER = "Container";
        public const string STR_DIFFICULTY = "Difficulty";
        public const string STR_TERRAIN = "Terrain";
        public const string STR_FROM = "From";
        public const string STR_TO = "To";
        public const string STR_FAVORITES = "Favorites";
        public const string STR_PUBLICATIONDATE = "Publication date";
        public const string STR_ATTRIBUTES = "Attributes";
        public const string STR_MINCOUNT = "Min. #";
        public const string STR_SELECT = "Select";
        public const string STR_COUNTRYSTATE = "Country / State";
        public const string STR_MUNICIPALITYCITY = "Municipality / City";
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_REFRESH = "Refresh";
        public const string STR_FOUND = "Found";
        public const string STR_IHAVEFOUND = "I found";
        public const string STR_IHAVENOTFOUND = "I have not found";
        public const string STR_OWN = "Own";
        public const string STR_IOWN = "I own";
        public const string STR_IDONOTOWN = "I do not own";
        public const string STR_CITY = "City";
        public const string STR_MUNICIPALITY = "Municipality";
        public const string STR_FOUNDBY = "Found by";
        public const string STR_NOTFOUNDBY = "Not found by";
        public const string STR_USERNAMES = "User names";
        public const string STR_COMMASEP = "Comma separated";
        public const string STR_ATLEASTONE = "At least one user";
        public const string STR_ALLUSERS = "Found by all";
        public const string STR_NOTBYANY = "Not found by any";

        public GeocacheSearchForm()
        {
            InitializeComponent();
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.radioButtonAddToCurrent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSELECTION);
            this.radioButtonWithinSelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCHWITHINSELECTION);
            this.radioButtonNewSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWSEARCH);
            this.checkBoxLocation.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CITYCOORD);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RADIUS);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CHECK);
            this.radioButtonKm.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KM);
            this.radioButtonMiles.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MILES);
            this.checkBoxCacheType.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHETYPE);
            this.checkBoxContainer.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONTAINER);
            this.checkBoxDifficulty.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DIFFICULTY);
            this.checkBoxTerrain.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TERRAIN);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FROM);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TO);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FROM);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TO);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FROM);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TO);
            this.checkBoxFavorites.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FAVORITES);
            this.checkBoxPubDate.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PUBLICATIONDATE);
            this.checkBoxAttributes.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ATTRIBUTES);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MINCOUNT);
            this.buttonDoSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            this.checkBoxCountryState.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRYSTATE);
            this.label19.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRY);
            this.label22.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STATE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REFRESH);
            this.checkBoxFound.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOUND);
            this.radioButtonIFound.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IHAVEFOUND);
            this.radioButtonIHaveNotFound.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IHAVENOTFOUND);
            this.checkBoxMunicipalityOther.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MUNICIPALITYCITY);
            this.label26.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MUNICIPALITY);
            this.label24.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CITY);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REFRESH);
            this.checkBoxFoundBy.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOUNDBY);
            this.checkBoxNotFoundBy.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTFOUNDBY);
            this.label27.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAMES);
            this.label31.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAMES);
            this.label44.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMASEP);
            this.label30.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMASEP);
            this.radioButton1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ATLEASTONE);
            this.radioButton4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ATLEASTONE);
            this.radioButton2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALLUSERS);
            this.radioButton3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTBYANY);
            this.checkBoxOwn.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OWN);
            this.radioButtonIOwn.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IOWN);
            this.radioButtonIDontOwnFound.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IDONOTOWN);
        }

        public GeocacheSearchForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            foreach (var gt in core.GeocacheTypes)
            {
                if (gt.ID > 0)
                {
                    imageListGeocacheTypes.Images.Add(gt.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, gt)));
                    listViewGeocacheType.Items.Add(new ListViewItem(gt.Name, imageListGeocacheTypes.Images.Count-1));
                }
            }
            foreach (var cnt in core.GeocacheContainers)
            {
                if (cnt.ID > 0)
                {
                    imageListContainers.Images.Add(cnt.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, cnt)));
                    listViewContainer.Items.Add(new ListViewItem(cnt.Name, imageListContainers.Images.Count - 1));
                }
            }
            foreach (var attr in core.GeocacheAttributes)
            {
                if (attr.ID > 0)
                {
                    imageListAttributes.Images.Add(attr.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, attr, Framework.Data.GeocacheAttribute.State.Yes)));
                    listViewAttributes.Items.Add(new ListViewItem(attr.Name, imageListAttributes.Images.Count - 1));
                }
            }
            foreach (var attr in core.GeocacheAttributes)
            {
                if (attr.ID > 0)
                {
                    imageListAttributes.Images.Add(attr.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, attr, Framework.Data.GeocacheAttribute.State.No)));
                    listViewAttributes.Items.Add(new ListViewItem(attr.Name, imageListAttributes.Images.Count - 1));
                }
            }
            string[] v = new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" };
            comboBoxDifficultyFrom.Items.AddRange(v);
            comboBoxDifficultyFrom.SelectedItem = "1";
            comboBoxDifficultyTo.Items.AddRange(v);
            comboBoxDifficultyTo.SelectedItem = "5";
            comboBoxTerrainFrom.Items.AddRange(v);
            comboBoxTerrainFrom.SelectedItem = "1";
            comboBoxTerrainTo.Items.AddRange(v);
            comboBoxTerrainTo.SelectedItem = "5";
            dateTimePickerPubDateFrom.Value = DateTime.Now.AddDays(-7);

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        private void GeocacheSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void checkBoxLocation_CheckedChanged(object sender, EventArgs e)
        {
            panelLocation.Visible = checkBoxLocation.Checked;
        }
        private void checkBoxFound_CheckedChanged(object sender, EventArgs e)
        {
            panelFound.Visible = checkBoxFound.Checked;
        }
        private void checkBoxOwn_CheckedChanged(object sender, EventArgs e)
        {
            panelOwn.Visible = checkBoxOwn.Checked;
        }
        private void checkBoxFoundBy_CheckedChanged(object sender, EventArgs e)
        {
            panelFoundBy.Visible = checkBoxFoundBy.Checked;
        }
        private void checkBoxNotFoundBy_CheckedChanged(object sender, EventArgs e)
        {
            panelNotFoundBy.Visible = checkBoxNotFoundBy.Checked;
        }

        private void checkBoxMunicipalityOther_CheckedChanged(object sender, EventArgs e)
        {
            panelMunicipalityOther.Visible = checkBoxMunicipalityOther.Checked;
            if (checkBoxMunicipalityOther.Checked && comboBoxMunicipality.SelectedIndex < 0 && comboBoxCity.SelectedIndex<0)
            {
                municipalityRefresh();
            }
        }
        public void municipalityRefresh()
        {
            comboBoxMunicipality.Items.Clear();
            comboBoxMunicipality.Items.Add("");
            comboBoxMunicipality.Items.AddRange((from Framework.Data.Geocache c in Core.Geocaches where !string.IsNullOrEmpty(c.Municipality) select c.Municipality).Distinct().OrderBy(x => x).ToArray());
            comboBoxMunicipality.SelectedIndex = 0;

            comboBoxCity.Items.Clear();
            comboBoxCity.Items.Add("");
            comboBoxCity.Items.AddRange((from Framework.Data.Geocache c in Core.Geocaches where !string.IsNullOrEmpty(c.City) select c.City).Distinct().OrderBy(x => x).ToArray());
            comboBoxCity.SelectedIndex = 0;
        }

        private void checkBoxCountryState_CheckedChanged(object sender, EventArgs e)
        {
            panelCountryState.Visible = checkBoxCountryState.Checked;

            if (comboBoxCountry.Items.Count == 0)
            {
                countryStateRefresh();
            }
        }
        private void countryStateRefresh()
        {
            this.Cursor = Cursors.WaitCursor;
            comboBoxState.Items.Clear();
            comboBoxCountry.Items.Clear();
            comboBoxCountry.Items.AddRange((from Framework.Data.Geocache c in Core.Geocaches where !string.IsNullOrEmpty(c.Country) select c.Country).Distinct().ToArray());
            if (comboBoxCountry.Items.Count > 0)
            {
                comboBoxCountry.SelectedIndex = 0;
            }
            this.Cursor = Cursors.Default;
        }

        private void checkBoxCacheType_CheckedChanged(object sender, EventArgs e)
        {
            panelCacheType.Visible = checkBoxCacheType.Checked;
        }

        private void checkBoxContainer_CheckedChanged(object sender, EventArgs e)
        {
            panelContainer.Visible = checkBoxContainer.Checked;
        }

        private void checkBoxDifficulty_CheckedChanged(object sender, EventArgs e)
        {
            panelDifficulty.Visible = checkBoxDifficulty.Checked;
        }

        private void checkBoxTerrain_CheckedChanged(object sender, EventArgs e)
        {
            panelTerrain.Visible = checkBoxTerrain.Checked;
        }

        private void checkBoxFavorites_CheckedChanged(object sender, EventArgs e)
        {
            panelFavorites.Visible = checkBoxFavorites.Checked;
        }

        private void checkBoxPubDate_CheckedChanged(object sender, EventArgs e)
        {
            panelPubDate.Visible = checkBoxPubDate.Checked;
        }

        private void buttonDoSearch_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Core.Geocaches.BeginUpdate();
            try
            {
                if (radioButtonNewSearch.Checked)
                {
                    //reset current
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = false;
                    }
                }

                Framework.Data.Location ll = null;
                if (checkBoxLocation.Checked && textBoxCenterLocation.Text.Length > 0)
                {
                    ll = Utils.Conversion.StringToLocation(textBoxCenterLocation.Text);
                    if (ll == null)
                    {
                        ll = Utils.Geocoder.GetLocationOfAddress(textBoxCenterLocation.Text);
                        if (ll == null)
                        {
                            textBoxCenterLocation.Text = "";
                        }
                        else
                        {
                            textBoxCenterLocation.Text = Utils.Conversion.GetCoordinatesPresentation(ll);
                        }
                    }
                }

                double minDiff = Utils.Conversion.StringToDouble(comboBoxDifficultyFrom.SelectedItem.ToString());
                double maxDiff = Utils.Conversion.StringToDouble(comboBoxDifficultyTo.SelectedItem.ToString());
                double minTerr = Utils.Conversion.StringToDouble(comboBoxTerrainFrom.SelectedItem.ToString());
                double maxTerr = Utils.Conversion.StringToDouble(comboBoxTerrainTo.SelectedItem.ToString());
                double dist = radioButtonKm.Checked ? (double)numericUpDownRadius.Value : (double)numericUpDownRadius.Value / 0.621371192237;
                dist *= 1000.0;
                List<int> gcTypeIds = null;
                if (checkBoxCacheType.Checked)
                {
                    gcTypeIds = (from ListViewItem s in listViewGeocacheType.Items where s.Checked select Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, s.SubItems[0].Text).ID).ToList();
                }
                List<int> cntTypeIds = null;
                if (checkBoxContainer.Checked)
                {
                    cntTypeIds = (from ListViewItem s in listViewContainer.Items where s.Checked select Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, s.SubItems[0].Text).ID).ToList();
                }
                List<int> attrYes = null;
                List<int> attrNo = null;
                if (checkBoxAttributes.Checked)
                {
                    List<ListViewItem> lvi = (from ListViewItem s in listViewAttributes.Items select s).Take(listViewAttributes.Items.Count / 2).ToList();
                    attrYes = (from s in lvi where s.Checked select int.Parse(imageListAttributes.Images.Keys[s.ImageIndex])).ToList();
                    lvi = (from ListViewItem s in listViewAttributes.Items select s).Skip(listViewAttributes.Items.Count / 2).ToList();
                    attrNo = (from s in lvi where s.Checked select -1 * int.Parse(imageListAttributes.Images.Keys[s.ImageIndex])).ToList();
                }
                string[] foundby = null;
                if (checkBoxFoundBy.Checked)
                {
                    string[] parts = textBox1.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foundby = (from s in parts select s.ToLower().Trim()).ToArray();
                    prepareFoundBy(foundby, radioButton2.Checked);
                }
                string[] notFoundby = null;
                if (checkBoxNotFoundBy.Checked)
                {
                    string[] parts = textBox2.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    notFoundby = (from s in parts select s.ToLower().Trim()).ToArray();
                    prepareNotFoundBy(notFoundby, radioButton3.Checked);
                }
                var gcList = (from Framework.Data.Geocache wp in Core.Geocaches
                              where
                              (!radioButtonWithinSelection.Checked || wp.Selected) &&
                              (!checkBoxFavorites.Checked || wp.Favorites >= numericUpDownFavorites.Value) &&
                              (!checkBoxDifficulty.Checked || (wp.Difficulty >= minDiff && wp.Difficulty <= maxDiff)) &&
                              (!checkBoxCountryState.Checked || (comboBoxState.SelectedIndex > 0 || (wp.Country == comboBoxCountry.SelectedItem as string))) &&
                              (!checkBoxCountryState.Checked || (comboBoxState.SelectedIndex == 0 || (wp.State == comboBoxState.SelectedItem as string))) &&
                              (ll == null || Utils.Calculus.CalculateDistance(wp, ll).EllipsoidalDistance <= dist) &&
                              (gcTypeIds == null || gcTypeIds.Contains(wp.GeocacheType.ID)) &&
                              (!checkBoxMunicipalityOther.Checked || (comboBoxMunicipality.SelectedIndex < 1 || wp.Municipality == comboBoxMunicipality.SelectedItem as string)) &&
                              (!checkBoxMunicipalityOther.Checked || (comboBoxCity.SelectedIndex < 1 || wp.City == comboBoxCity.SelectedItem as string)) &&
                              (cntTypeIds == null || cntTypeIds.Contains(wp.Container.ID)) &&
                              (!checkBoxPubDate.Checked || (wp.PublishedTime >= dateTimePickerPubDateFrom.Value && wp.PublishedTime <= dateTimePickerPubDateTo.Value)) &&
                              (!checkBoxAttributes.Checked || validateAttributes(wp, attrYes, attrNo)) &&
                              (!checkBoxFound.Checked || ((radioButtonIFound.Checked && wp.Found) || (radioButtonIHaveNotFound.Checked && !wp.Found))) &&
                              (!checkBoxOwn.Checked || ((radioButtonIOwn.Checked && wp.IsOwn) || (radioButtonIDontOwnFound.Checked && !wp.IsOwn))) &&
                              (!checkBoxFoundBy.Checked || validateFoundBy(wp)) &&
                              (!checkBoxNotFoundBy.Checked || validateNotFoundBy(wp)) &&
                              (!checkBoxTerrain.Checked || (wp.Terrain >= minTerr && wp.Terrain <= maxTerr))
                              select wp).ToList();

                if (radioButtonWithinSelection.Checked || radioButtonNewSearch.Checked)
                {
                    //reset current
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = false;
                    }
                }

                foreach (Framework.Data.Geocache gc in gcList)
                {
                    gc.Selected = true;
                }
            }
            catch
            {
            }

            Core.Geocaches.EndUpdate();
            Cursor = Cursors.Default;
        }

        private List<Framework.Data.Geocache> _foundByList = null;
        private void prepareFoundBy(string[] usrs, bool foundAll)
        {
            if (foundAll)
            {
                var sel = from s in usrs
                          join Framework.Data.Log l in Core.Logs on s equals l.Finder.ToLower()
                          where l.LogType.AsFound
                          group l by l.GeocacheCode into lc
                          select new { Geocache = lc.Key, Founds = lc };
                var fo = from l in sel
                         where l.Founds.Count() >= usrs.Length
                         select l.Geocache;
                _foundByList = (from Framework.Data.Geocache g in Core.Geocaches
                                join f in fo on g.Code equals f
                                select g).Distinct().ToList();
            }
            else
            {
                _foundByList = (from Framework.Data.Geocache g in Core.Geocaches
                                join Framework.Data.Log l in Core.Logs on g.Code equals l.GeocacheCode
                                join s in usrs on l.Finder.ToLower() equals s
                                where l.Finder.ToLower() == s && l.LogType.AsFound
                                select g).Distinct().ToList();
            }
        }
        private bool validateFoundBy(Framework.Data.Geocache gc)
        {
            return _foundByList.Contains(gc);
        }


        private List<Framework.Data.Geocache> _notFoundByList = null;
        private void prepareNotFoundBy(string[] usrs, bool notFoundByAny)
        {
            if (notFoundByAny)
            {
                var sel = from s in usrs
                          join Framework.Data.Log l in Core.Logs on s equals l.Finder.ToLower()
                          where l.LogType.AsFound
                          group l by l.GeocacheCode into lc
                          select new { Geocache = lc.Key, Founds = lc };
                var fo = from l in sel
                         where l.Founds.Count() >= usrs.Length
                         select l.Geocache;
                _notFoundByList = (from Framework.Data.Geocache g in Core.Geocaches
                                join f in fo on g.Code equals f
                                select g).Distinct().ToList();
            }
            else
            {
                _notFoundByList = (from Framework.Data.Geocache g in Core.Geocaches
                                join Framework.Data.Log l in Core.Logs on g.Code equals l.GeocacheCode
                                join s in usrs on l.Finder.ToLower() equals s
                                where l.Finder.ToLower() == s && l.LogType.AsFound
                                select g).Distinct().ToList();
            }
        }
        private bool validateNotFoundBy(Framework.Data.Geocache gc)
        {
            return !_notFoundByList.Contains(gc);
        }


        private bool validateAttributes(Framework.Data.Geocache gc, List<int> attrYes, List<int> attrNo)
        {
            bool result = false;
            if (radioButtonAttrAtLeastOne.Checked)
            {
                result = (from a in gc.AttributeIds
                          join b in attrYes on a equals b
                          select a).Count() > 0;
                if (!result)
                {
                    result = (from a in gc.AttributeIds
                              join b in attrNo on a equals b
                              select a).Count() > 0;
                }
            }
            else if (radioButtonAttrAll.Checked)
            {
                List<int> allSelected = new List<int>();
                allSelected.AddRange(attrYes);
                allSelected.AddRange(attrNo);

                result = (from a in gc.AttributeIds
                          join b in allSelected on a equals b
                          select a).Count() == allSelected.Count;
            }
            else if (radioButtonAttrNone.Checked)
            {
                List<int> allSelected = new List<int>();
                allSelected.AddRange(attrYes);
                allSelected.AddRange(attrNo);

                result = (from a in gc.AttributeIds
                          join b in allSelected on a equals b
                          select a).Count() == 0;
            }
            return result;
        }

        private void buttonLocationDlg_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, Core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBoxCenterLocation.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Framework.Data.Location ll = null;
            if (textBoxCenterLocation.Text.Length > 0)
            {
                ll = Utils.Conversion.StringToLocation(textBoxCenterLocation.Text);
                if (ll == null)
                {
                    ll = Utils.Geocoder.GetLocationOfAddress(textBoxCenterLocation.Text);
                    if (ll == null)
                    {
                        textBoxCenterLocation.Text = "";
                    }
                    else
                    {
                        textBoxCenterLocation.Text = Utils.Conversion.GetCoordinatesPresentation(ll);
                    }
                }
            }
        }

        private void checkBoxAttributes_CheckedChanged(object sender, EventArgs e)
        {
            panelAttributes.Visible = checkBoxAttributes.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            countryStateRefresh();
        }

        private void comboBoxCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxState.Items.Clear();
            comboBoxState.Items.Add("");
            if (comboBoxCountry.SelectedIndex >= 0)
            {
                string s = comboBoxCountry.SelectedItem.ToString();
                comboBoxState.Items.AddRange((from Framework.Data.Geocache c in Core.Geocaches where !string.IsNullOrEmpty(c.State) && s == c.Country select c.State).Distinct().ToArray());
            }
            comboBoxState.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            municipalityRefresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Utils.PluginSupport.ExecuteDefaultAction(Core, "GlobalcachingApplication.Plugins.APIFindsOfUser.Import");            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Utils.PluginSupport.ExecuteDefaultAction(Core, "GlobalcachingApplication.Plugins.APIFindsOfUser.Import");            
        }
    }

    public class GeocacheSearch : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Search geocache";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_SEARCHWITHINSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_NEWSEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_ADDTOSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_CITYCOORD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_RADIUS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_CHECK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_KM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_MILES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_GEOCACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_TERRAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_DIFFICULTY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_GEOCACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_FROM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_TO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_FAVORITES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_ATTRIBUTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_PUBLICATIONDATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_MINCOUNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_SELECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_COUNTRYSTATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_COUNTRY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_STATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_REFRESH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_FOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_IHAVEFOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_IHAVENOTFOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_MUNICIPALITYCITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_MUNICIPALITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_CITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_FOUNDBY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_NOTFOUNDBY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_USERNAMES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_COMMASEP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_ATLEASTONE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_ALLUSERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_NOTBYANY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_OWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_IDONOTOWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheSearchForm.STR_IOWN));
            
            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new GeocacheSearchForm(this, core));
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
    }

}
