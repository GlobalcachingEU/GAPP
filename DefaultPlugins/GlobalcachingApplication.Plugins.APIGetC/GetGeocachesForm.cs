using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIGetC
{
    public partial class GetGeocachesForm : Form
    {
        public const string STR_TITLE = "Get geocaches using Live API";
        public const string STR_AREACODE = "Areas / Codes";
        public const string STR_LOCATION = "Location";
        public const string STR_RADIUS = "Radius";
        public const string STR_CODES = "Codes";
        public const string STR_NAME = "Name";
        public const string STR_HIDDENBY = "Hidden by";
        public const string STR_KM = "km";
        public const string STR_MILES = "miles";
        public const string STR_CLEAR = "Clear";
        public const string STR_PROPERTIES = "Properties";
        public const string STR_EXCLUDE = "Exclude";
        public const string STR_LIMITS = "Limits";
        public const string STR_GEOCACHETYPE = "Geocache type";
        public const string STR_CONTAINER = "Container";
        public const string STR_FAVMIN = "Fav. min";
        public const string STR_FAVMAX = "Fav. max";
        public const string STR_DIFMIN = "Dif. min";
        public const string STR_DIFMAX = "Dif. max";
        public const string STR_TERMIN = "Ter. min";
        public const string STR_TERMAX = "Ter. max";
        public const string STR_TRACKMIN = "Track. min";
        public const string STR_TRACKMAX = "Track. max";
        public const string STR_FOUNDBYUSERS = "Found by users";
        public const string STR_HIDDENBYUSERS = "Hidden by users";
        public const string STR_ARCHIVED = "Archived";
        public const string STR_AVAILABLE = "Available";
        public const string STR_PMO = "Premium Member Only";
        public const string STR_TOTALMAX = "Total maximum";
        public const string STR_MAXPERREQUEST = "Maximum per request";
        public const string STR_MAXLOGS = "Maximum logs";
        public const string STR_COMMASEP = "Comma separated";
        public const string STR_IMPORT = "Import";
        public const string STR_CURRENTSETTINGS = "Active";
        public const string STR_PRESET = "Preset";
        public const string STR_DEFAULT = "Default";
        public const string STR_RELOAD = "Reload";
        public const string STR_PUBLISHED = "Published";
        public const string STR_BETWEEN = "between";
        public const string STR_AND = "and";
        public const string STR_MAX30DAYS = "max. 30 days.";

        private GetGeocaches _ownerPlugin = null;
        private Framework.Interfaces.ICore _core = null;
        public Utils.API.LiveV6.SearchForGeocachesRequest SearchForGeocachesRequestProperties = null;
        public int Max = 10;

        private List<PresetSettings> _presetSettings = new List<PresetSettings>();
        private string _presetsFile = null;
        private PresetSettings _defaultSettings = null;

        public GetGeocachesForm()
        {
            InitializeComponent();
        }

        public GetGeocachesForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AREACODE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RADIUS);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODES);
            this.label14.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HIDDENBY);
            this.radioButtonKm.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KM);
            this.radioButtonMiles.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MILES);
            this.buttonLocationClear.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAR);
            this.groupBox5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PROPERTIES);
            this.groupBox7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXCLUDE);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LIMITS);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHETYPE);
            this.label16.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONTAINER);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FAVMIN);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FAVMAX);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DIFMIN);
            this.label11.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DIFMAX);
            this.label13.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TERMIN);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TERMAX);
            this.label20.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKMIN);
            this.label19.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKMAX);
            this.label17.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOUNDBYUSERS);
            this.label18.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HIDDENBYUSERS);
            this.checkBoxArchived.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ARCHIVED);
            this.checkBoxAvailable.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVAILABLE);
            this.checkBoxPremium.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PMO);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TOTALMAX);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXPERREQUEST);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            this.label44.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMASEP);
            this.label46.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PRESET);
            this.buttonImport.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RELOAD);
            this.label48.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PUBLISHED);
            this.label49.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BETWEEN);
            this.label50.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AND);
            this.label51.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAX30DAYS);

            this.radioButtonKm.Checked = Properties.Settings.Default.UseMetric;
            this.radioButtonMiles.Checked = !Properties.Settings.Default.UseMetric;
            this.numericUpDownLogCount.Value = Properties.Settings.Default.MaxLogs;
            this.numericUpDownMaxPerPage.Value = Properties.Settings.Default.MaxPerRequest;
            this.numericUpDownMaximum.Value = Properties.Settings.Default.TotalMaximum;

            _ownerPlugin = owner as GetGeocaches;
            _core = core;

            foreach (var gt in core.GeocacheTypes)
            {
                if (gt.ID > 0)
                {
                    imageListGeocacheTypes.Images.Add(gt.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, gt)));
                    listViewGeocacheType.Items.Add(new ListViewItem(gt.Name, imageListGeocacheTypes.Images.Count - 1));
                }
            }
            foreach (ListViewItem lvi in listViewGeocacheType.Items)
            {
                lvi.Checked = true;
            }
            foreach (var cnt in core.GeocacheContainers)
            {
                if (cnt.ID > 0)
                {
                    imageListContainers.Images.Add(cnt.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, cnt)));
                    listViewContainer.Items.Add(new ListViewItem(cnt.Name, imageListContainers.Images.Count - 1));
                }
            }
            foreach (ListViewItem lvi in listViewContainer.Items)
            {
                lvi.Checked = true;
            }

            _defaultSettings = CurrentSettings;
            _defaultSettings.Name = Utils.LanguageSupport.Instance.GetTranslation(STR_DEFAULT);
            comboBox1.Items.Add(_defaultSettings);
            LoadPresets();

            if (_core.GeocachingComAccount.MemberTypeId > 1)
            {
                groupBox5.Enabled = true;
                groupBox7.Enabled = true;
            }
            else
            {
                groupBox5.Enabled = false;
                groupBox7.Enabled = false;
            }
        }

        private void LoadPresets()
        {
            try
            {
                _presetsFile = System.IO.Path.Combine(new string[] { _core.PluginDataPath, "ApiImportPresets.xml" });

                if (System.IO.File.Exists(_presetsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_presetsFile);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList presetNodes = root.SelectNodes("preset");
                    if (presetNodes != null)
                    {
                        bool first = true;
                        foreach (XmlNode n in presetNodes)
                        {
                            PresetSettings ps = new PresetSettings();
                            ps.Name = n.SelectSingleNode("Name").InnerText;
                            ps.CacheTypes = new int[n.SelectSingleNode("CacheTypes").ChildNodes.Count];
                            for (int i = 0; i < ps.CacheTypes.Length; i++)
                            {
                                ps.CacheTypes[i] = int.Parse(n.SelectSingleNode("CacheTypes").ChildNodes[i].InnerText);
                            }
                            ps.Codes = n.SelectSingleNode("Codes").InnerText;
                            ps.Containers = new int[n.SelectSingleNode("Containers").ChildNodes.Count];
                            for (int i = 0; i < ps.Containers.Length; i++)
                            {
                                ps.Containers[i] = int.Parse(n.SelectSingleNode("Containers").ChildNodes[i].InnerText);
                            }
                            ps.DifMax = Utils.Conversion.StringToDouble(n.SelectSingleNode("DifMax").InnerText);
                            ps.DifMin = Utils.Conversion.StringToDouble(n.SelectSingleNode("DifMin").InnerText);
                            if (string.IsNullOrEmpty(n.SelectSingleNode("ExclArchived").InnerText))
                            {
                                ps.ExclArchived = null;
                            }
                            else
                            {
                                ps.ExclArchived = bool.Parse(n.SelectSingleNode("ExclArchived").InnerText);
                            }
                            if (string.IsNullOrEmpty(n.SelectSingleNode("ExclAvailable").InnerText))
                            {
                                ps.ExclAvailable = null;
                            }
                            else
                            {
                                ps.ExclAvailable = bool.Parse(n.SelectSingleNode("ExclAvailable").InnerText);
                            }
                            ps.ExclFoundBy = n.SelectSingleNode("ExclFoundBy").InnerText;
                            ps.ExclHiddenBy = n.SelectSingleNode("ExclHiddenBy").InnerText;
                            if (string.IsNullOrEmpty(n.SelectSingleNode("ExclPMO").InnerText))
                            {
                                ps.ExclPMO = null;
                            }
                            else
                            {
                                ps.ExclPMO = bool.Parse(n.SelectSingleNode("ExclPMO").InnerText);
                            }
                            ps.FavMax = Utils.Conversion.StringToDouble(n.SelectSingleNode("FavMax").InnerText);
                            ps.FavMin = Utils.Conversion.StringToDouble(n.SelectSingleNode("FavMin").InnerText);
                            ps.GeocacheName = n.SelectSingleNode("GeocacheName").InnerText;
                            ps.HiddenBy = n.SelectSingleNode("HiddenBy").InnerText;
                            ps.Km = bool.Parse(n.SelectSingleNode("Km").InnerText);
                            ps.LimitMaxLogs = int.Parse(n.SelectSingleNode("LimitMaxLogs").InnerText);
                            ps.LimitMaxRequest = int.Parse(n.SelectSingleNode("LimitMaxRequest").InnerText);
                            ps.LimitMaxTotal = int.Parse(n.SelectSingleNode("LimitMaxTotal").InnerText);
                            ps.Location = n.SelectSingleNode("Location").InnerText;
                            ps.Radius = Utils.Conversion.StringToDouble(n.SelectSingleNode("Radius").InnerText);
                            ps.TerMax = Utils.Conversion.StringToDouble(n.SelectSingleNode("TerMax").InnerText);
                            ps.TerMin = Utils.Conversion.StringToDouble(n.SelectSingleNode("TerMin").InnerText);
                            ps.TrackMax = Utils.Conversion.StringToDouble(n.SelectSingleNode("TrackMax").InnerText);
                            ps.TrackMin = Utils.Conversion.StringToDouble(n.SelectSingleNode("TrackMin").InnerText);

                            XmlNode btd = n.SelectSingleNode("BetweenPublishedDates");
                            if (btd == null)
                            {
                                ps.BetweenPublishedDates = false;
                            }
                            else
                            {
                                ps.BetweenPublishedDates = false;
                                ps.FromPublishedDate = DateTime.Parse(btd.SelectSingleNode("FromPublishedDate").InnerText);
                                ps.ToPublishedDate = DateTime.Parse(btd.SelectSingleNode("ToPublishedDate").InnerText);
                            }

                            if (first)
                            {
                                first = false;
                                CurrentSettings = ps;
                            }
                            else
                            {
                                _presetSettings.Add(ps);
                                comboBox1.Items.Add(ps);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void SavePresets()
        {
            try
            {
                List<PresetSettings> allsettings = new List<PresetSettings>();
                allsettings.Add(CurrentSettings);
                allsettings.AddRange(_presetSettings);

                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("presets");
                doc.AppendChild(root);
                foreach (PresetSettings settings in allsettings)
                {
                    XmlElement preset = doc.CreateElement("preset");
                    root.AppendChild(preset);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(settings.Name);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("CacheTypes");
                    preset.AppendChild(el);
                    foreach (int i in settings.CacheTypes)
                    {
                        XmlElement subel = doc.CreateElement("ID");
                        txt = doc.CreateTextNode(i.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("Codes");
                    txt = doc.CreateTextNode(settings.Codes);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("Containers");
                    preset.AppendChild(el);
                    foreach (int i in settings.Containers)
                    {
                        XmlElement subel = doc.CreateElement("ID");
                        txt = doc.CreateTextNode(i.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("DifMax");
                    txt = doc.CreateTextNode(settings.DifMax.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("DifMin");
                    txt = doc.CreateTextNode(settings.DifMin.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("ExclArchived");
                    txt = doc.CreateTextNode(settings.ExclArchived==null?"":((bool)settings.ExclArchived).ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("ExclAvailable");
                    txt = doc.CreateTextNode(settings.ExclAvailable==null?"":((bool)settings.ExclAvailable).ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("ExclFoundBy");
                    txt = doc.CreateTextNode(settings.ExclFoundBy);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("ExclHiddenBy");
                    txt = doc.CreateTextNode(settings.ExclHiddenBy);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("ExclPMO");
                    txt = doc.CreateTextNode(settings.ExclPMO==null?"":((bool)settings.ExclPMO).ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("FavMax");
                    txt = doc.CreateTextNode(settings.FavMax.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("FavMin");
                    txt = doc.CreateTextNode(settings.FavMin.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("GeocacheName");
                    txt = doc.CreateTextNode(settings.GeocacheName);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("HiddenBy");
                    txt = doc.CreateTextNode(settings.HiddenBy);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("Km");
                    txt = doc.CreateTextNode(settings.Km.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("LimitMaxLogs");
                    txt = doc.CreateTextNode(settings.LimitMaxLogs.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("LimitMaxRequest");
                    txt = doc.CreateTextNode(settings.LimitMaxRequest.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("LimitMaxTotal");
                    txt = doc.CreateTextNode(settings.LimitMaxTotal.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("Location");
                    txt = doc.CreateTextNode(settings.Location);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("Name");
                    txt = doc.CreateTextNode(settings.Name);
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("Radius");
                    txt = doc.CreateTextNode(settings.Radius.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("TerMax");
                    txt = doc.CreateTextNode(settings.TerMax.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("TerMin");
                    txt = doc.CreateTextNode(settings.TerMin.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("TrackMax");
                    txt = doc.CreateTextNode(settings.TrackMax.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    el = doc.CreateElement("TrackMin");
                    txt = doc.CreateTextNode(settings.TrackMin.ToString());
                    el.AppendChild(txt);
                    preset.AppendChild(el);

                    if (settings.BetweenPublishedDates)
                    {
                        el = doc.CreateElement("BetweenPublishedDates");
                        preset.AppendChild(el);

                        XmlElement subel = doc.CreateElement("FromPublishedDate");
                        txt = doc.CreateTextNode(settings.FromPublishedDate.ToString("s"));
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                        subel = doc.CreateElement("ToPublishedDate");
                        txt = doc.CreateTextNode(settings.ToPublishedDate.ToString("s"));
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }
                }
                doc.Save(_presetsFile);
            }
            catch
            {
            }
        }

        private PresetSettings CurrentSettings
        {
            get
            {
                PresetSettings result = new PresetSettings();
                result.Name = STR_CURRENTSETTINGS;
                result.CacheTypes = (from ListViewItem s in listViewGeocacheType.Items where s.Checked select (int)Utils.DataAccess.GetGeocacheType(_core.GeocacheTypes, s.SubItems[0].Text).ID).ToArray();
                result.Codes = textBoxCodes.Text;
                result.Containers = (from ListViewItem s in listViewContainer.Items where s.Checked select (int)Utils.DataAccess.GetGeocacheContainer(_core.GeocacheContainers, s.SubItems[0].Text).ID).ToArray();
                result.DifMax = (double)numericUpDownMaxDif.Value;
                result.DifMin = (double)numericUpDownMinDif.Value;
                if (checkBoxArchived.CheckState == CheckState.Indeterminate)
                {
                    result.ExclArchived = null;
                }
                else
                {
                    result.ExclArchived = checkBoxArchived.CheckState == CheckState.Checked;
                }
                if (checkBoxAvailable.CheckState == CheckState.Indeterminate)
                {
                    result.ExclAvailable = null;
                }
                else
                {
                    result.ExclAvailable = checkBoxAvailable.CheckState == CheckState.Checked;
                }
                if (checkBoxPremium.CheckState == CheckState.Indeterminate)
                {
                    result.ExclPMO = null;
                }
                else
                {
                    result.ExclPMO = checkBoxPremium.CheckState == CheckState.Checked;
                }
                result.ExclFoundBy = textBoxNotFound.Text;
                result.ExclHiddenBy = textBoxNotHidden.Text;
                result.FavMax = (double)numericUpDownMaxFav.Value;
                result.FavMin = (double)numericUpDownMinFav.Value;
                result.GeocacheName = textBoxName.Text;
                result.HiddenBy = textBoxHiddenBy.Text;
                result.Km = radioButtonKm.Checked;
                result.LimitMaxLogs = (int)numericUpDownLogCount.Value;
                result.LimitMaxRequest = (int)numericUpDownMaxPerPage.Value;
                result.LimitMaxTotal = (int)numericUpDownMaximum.Value;
                result.Location = textBoxLocation.Text;
                result.Radius = (int)numericUpDownRadius.Value;
                result.TerMax = (double)numericUpDownMaxTer.Value;
                result.TerMin = (double)numericUpDownMinTer.Value;
                result.TrackMax = (double)numericUpDownTrackMax.Value;
                result.TrackMin = (double)numericUpDownTrackMin.Value;
                result.BetweenPublishedDates = checkBox1.Checked;
                result.FromPublishedDate = dateTimePicker1.Value;
                result.ToPublishedDate = dateTimePicker2.Value;
                return result;
            }
            set
            {
                if (value != null)
                {
                    foreach (ListViewItem s in listViewGeocacheType.Items)
                    {
                        s.Checked = value.CacheTypes.Contains((int)Utils.DataAccess.GetGeocacheType(_core.GeocacheTypes, s.SubItems[0].Text).ID);
                    }
                    textBoxCodes.Text = value.Codes;
                    foreach (ListViewItem s in listViewContainer.Items)
                    {
                        s.Checked = value.Containers.Contains((int)Utils.DataAccess.GetGeocacheContainer(_core.GeocacheContainers, s.SubItems[0].Text).ID);
                    }
                    numericUpDownMaxDif.Value = (decimal)value.DifMax;
                    numericUpDownMinDif.Value = (decimal)value.DifMin;
                    if (value.ExclArchived == null)
                    {
                        checkBoxArchived.CheckState = CheckState.Indeterminate;
                    }
                    else
                    {
                        checkBoxArchived.CheckState = (bool)value.ExclArchived ? CheckState.Checked : CheckState.Unchecked;
                    }
                    if (value.ExclAvailable == null)
                    {
                        checkBoxAvailable.CheckState = CheckState.Indeterminate;
                    }
                    else
                    {
                        checkBoxAvailable.CheckState = (bool)value.ExclAvailable ? CheckState.Checked : CheckState.Unchecked;
                    }
                    if (value.ExclPMO == null)
                    {
                        checkBoxPremium.CheckState = CheckState.Indeterminate;
                    }
                    else
                    {
                        checkBoxPremium.CheckState = (bool)value.ExclPMO ? CheckState.Checked : CheckState.Unchecked;
                    }
                    textBoxNotFound.Text = value.ExclFoundBy;
                    textBoxNotHidden.Text=  value.ExclHiddenBy;
                    numericUpDownMaxFav.Value = (decimal)value.FavMax;
                    numericUpDownMinFav.Value = (decimal)value.FavMin;
                    textBoxName.Text = value.GeocacheName;
                    textBoxHiddenBy.Text = value.HiddenBy;
                    radioButtonKm.Checked = value.Km;
                    radioButtonMiles.Checked = !value.Km;
                    numericUpDownLogCount.Value = (decimal)value.LimitMaxLogs;
                    numericUpDownMaxPerPage.Value = (decimal)value.LimitMaxRequest;
                    numericUpDownMaximum.Value = (decimal)value.LimitMaxTotal;
                    textBoxLocation.Text = value.Location;
                    numericUpDownRadius.Value = (decimal)value.Radius;
                    numericUpDownMaxTer.Value = (decimal)value.TerMax;
                    numericUpDownMinTer.Value = (decimal)value.TerMin;
                    numericUpDownTrackMax.Value = (decimal)value.TrackMax;
                    numericUpDownTrackMin.Value = (decimal)value.TrackMin;
                    checkBox1.Checked = value.BetweenPublishedDates;
                    if (value.BetweenPublishedDates)
                    {
                        dateTimePicker1.Value = value.FromPublishedDate;
                        dateTimePicker2.Value = value.ToPublishedDate;
                    }
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseMetric = this.radioButtonKm.Checked;
            Properties.Settings.Default.MaxLogs = (int)this.numericUpDownLogCount.Value;
            Properties.Settings.Default.MaxPerRequest = (int)this.numericUpDownMaxPerPage.Value;
            Properties.Settings.Default.TotalMaximum = (int)this.numericUpDownMaximum.Value;
            Properties.Settings.Default.Save();

            SavePresets();

            SearchForGeocachesRequestProperties = new Utils.API.LiveV6.SearchForGeocachesRequest();
            SearchForGeocachesRequestProperties.IsLite = _core.GeocachingComAccount.MemberTypeId==1;
            Max = (int)numericUpDownMaximum.Value;
            SearchForGeocachesRequestProperties.MaxPerPage = (int)Math.Min(numericUpDownMaximum.Value, numericUpDownMaxPerPage.Value);
            if (textBoxLocation.Text != "")
            {
                double dist = (double)numericUpDownRadius.Value * 1000.0;
                if (radioButtonMiles.Checked) dist *= 1.6214;
                Framework.Data.Location l = Utils.Conversion.StringToLocation(textBoxLocation.Text);

                SearchForGeocachesRequestProperties.PointRadius = new Utils.API.LiveV6.PointRadiusFilter();
                SearchForGeocachesRequestProperties.PointRadius.DistanceInMeters = (long)dist;
                SearchForGeocachesRequestProperties.PointRadius.Point = new Utils.API.LiveV6.LatLngPoint();
                SearchForGeocachesRequestProperties.PointRadius.Point.Latitude = l.Lat;
                SearchForGeocachesRequestProperties.PointRadius.Point.Longitude = l.Lon;
            }
            if (textBoxCodes.Text != "")
            {
                string[] parts = textBoxCodes.Text.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                SearchForGeocachesRequestProperties.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                SearchForGeocachesRequestProperties.CacheCode.CacheCodes = (from s in parts select s.ToUpper()).ToArray();
            }
            SearchForGeocachesRequestProperties.GeocacheLogCount = (int)numericUpDownLogCount.Value;
            if (_core.GeocachingComAccount.MemberTypeId > 1)
            {
                SearchForGeocachesRequestProperties.FavoritePoints = new Utils.API.LiveV6.FavoritePointsFilter();
                SearchForGeocachesRequestProperties.FavoritePoints.MinFavoritePoints = (int)numericUpDownMinFav.Value;
                SearchForGeocachesRequestProperties.FavoritePoints.MaxFavoritePoints = (int)numericUpDownMaxFav.Value;
                SearchForGeocachesRequestProperties.Difficulty = new Utils.API.LiveV6.DifficultyFilter();
                SearchForGeocachesRequestProperties.Difficulty.MinDifficulty = (double)numericUpDownMinDif.Value;
                SearchForGeocachesRequestProperties.Difficulty.MaxDifficulty = (double)numericUpDownMaxDif.Value;
                SearchForGeocachesRequestProperties.Terrain = new Utils.API.LiveV6.TerrainFilter();
                SearchForGeocachesRequestProperties.Terrain.MinTerrain = (double)numericUpDownMinTer.Value;
                SearchForGeocachesRequestProperties.Terrain.MaxTerrain = (double)numericUpDownMaxTer.Value;

                if (checkBoxArchived.CheckState != CheckState.Indeterminate ||
                    checkBoxAvailable.CheckState != CheckState.Indeterminate ||
                    checkBoxPremium.CheckState != CheckState.Indeterminate)
                {
                    SearchForGeocachesRequestProperties.GeocacheExclusions = new Utils.API.LiveV6.GeocacheExclusionsFilter();
                    if (checkBoxArchived.CheckState != CheckState.Indeterminate)
                    {
                        SearchForGeocachesRequestProperties.GeocacheExclusions.Archived = (checkBoxArchived.CheckState != CheckState.Checked);
                    }
                    if (checkBoxAvailable.CheckState != CheckState.Indeterminate)
                    {
                        SearchForGeocachesRequestProperties.GeocacheExclusions.Available = (checkBoxAvailable.CheckState != CheckState.Checked);
                    }
                    if (checkBoxPremium.CheckState != CheckState.Indeterminate)
                    {
                        SearchForGeocachesRequestProperties.GeocacheExclusions.Premium = (checkBoxPremium.CheckState != CheckState.Checked);
                    }
                }
            }
            if (textBoxName.Text != "")
            {
                SearchForGeocachesRequestProperties.GeocacheName = new Utils.API.LiveV6.GeocacheNameFilter();
                SearchForGeocachesRequestProperties.GeocacheName.GeocacheName = textBoxName.Text;
            }
            if (_core.GeocachingComAccount.MemberTypeId > 1)
            {
                long[] gcTypeIds = (from ListViewItem s in listViewGeocacheType.Items where s.Checked select (long)Utils.DataAccess.GetGeocacheType(_core.GeocacheTypes, s.SubItems[0].Text).ID).ToArray();
                if (gcTypeIds.Length < listViewGeocacheType.Items.Count)
                {
                    SearchForGeocachesRequestProperties.GeocacheType = new Utils.API.LiveV6.GeocacheTypeFilter();
                    SearchForGeocachesRequestProperties.GeocacheType.GeocacheTypeIds = gcTypeIds;
                }
                long[] cntTypeIds = (from ListViewItem s in listViewContainer.Items where s.Checked select (long)Utils.DataAccess.GetGeocacheContainer(_core.GeocacheContainers, s.SubItems[0].Text).ID).ToArray();
                if (cntTypeIds.Length < listViewContainer.Items.Count)
                {
                    SearchForGeocachesRequestProperties.GeocacheContainerSize = new Utils.API.LiveV6.GeocacheContainerSizeFilter();
                    SearchForGeocachesRequestProperties.GeocacheContainerSize.GeocacheContainerSizeIds = cntTypeIds;
                }
                if (textBoxNotFound.Text != "")
                {
                    SearchForGeocachesRequestProperties.NotFoundByUsers = new Utils.API.LiveV6.NotFoundByUsersFilter();
                    string[] parts = textBoxNotFound.Text.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    SearchForGeocachesRequestProperties.NotFoundByUsers.UserNames = (from s in parts select s.Trim()).ToArray();
                }
                if (textBoxNotHidden.Text != "")
                {
                    SearchForGeocachesRequestProperties.NotHiddenByUsers = new Utils.API.LiveV6.NotHiddenByUsersFilter();
                    string[] parts = textBoxNotHidden.Text.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    SearchForGeocachesRequestProperties.NotHiddenByUsers.UserNames = (from s in parts select s.Trim()).ToArray();
                }
            }
            if (textBoxHiddenBy.Text != "")
            {
                SearchForGeocachesRequestProperties.HiddenByUsers = new Utils.API.LiveV6.HiddenByUsersFilter();
                string[] parts = textBoxHiddenBy.Text.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                SearchForGeocachesRequestProperties.HiddenByUsers.UserNames = (from s in parts select s.Trim()).ToArray();
            }
            if (_core.GeocachingComAccount.MemberTypeId > 1)
            {
                SearchForGeocachesRequestProperties.TrackableCount = new Utils.API.LiveV6.TrackableCountFilter();
                SearchForGeocachesRequestProperties.TrackableCount.MinTrackables = (int)numericUpDownTrackMin.Value;
                SearchForGeocachesRequestProperties.TrackableCount.MaxTrackables = (int)numericUpDownTrackMax.Value;
                SearchForGeocachesRequestProperties.TrackableLogCount = 0;
                if (checkBox1.Checked)
                {
                    SearchForGeocachesRequestProperties.CachePublishedDate = new Utils.API.LiveV6.CachePublishedDateFilter();
                    SearchForGeocachesRequestProperties.CachePublishedDate.Range = new Utils.API.LiveV6.DateRange();
                    SearchForGeocachesRequestProperties.CachePublishedDate.Range.StartDate = dateTimePicker1.Value < dateTimePicker2.Value ? dateTimePicker1.Value : dateTimePicker2.Value;
                    SearchForGeocachesRequestProperties.CachePublishedDate.Range.EndDate = dateTimePicker2.Value > dateTimePicker1.Value ? dateTimePicker2.Value : dateTimePicker1.Value;
                }
            }
        }

        private void buttonLocation_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, _core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBoxLocation.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result);
                }
            }
        }

        private void textBoxLocation_TextChanged(object sender, EventArgs e)
        {
            buttonImport.Enabled = ((textBoxLocation.Text.Length > 0 || textBoxCodes.Text.Length > 0) &&
                !checkBox1.Checked || Math.Abs((dateTimePicker2.Value-dateTimePicker1.Value).TotalDays)<30);
            if (textBoxLocation.Text.Length > 0)
            {
                textBoxCodes.Text = "";
            }
        }

        private void textBoxCodes_TextChanged(object sender, EventArgs e)
        {
            buttonImport.Enabled = ((textBoxLocation.Text.Length > 0 || textBoxCodes.Text.Length > 0) &&
                !checkBox1.Checked || Math.Abs((dateTimePicker2.Value - dateTimePicker1.Value).TotalDays) < 30);
            if (textBoxCodes.Text.Length > 0)
            {
                textBoxLocation.Text = "";
            }
        }

        private void buttonLocationClear_Click(object sender, EventArgs e)
        {
            textBoxLocation.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (PresetsForm dlg = new PresetsForm(_presetSettings, CurrentSettings))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add(_defaultSettings);
                    _presetSettings = dlg.Presets;
                    comboBox1.Items.AddRange(_presetSettings.ToArray());
                    SavePresets();
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                CurrentSettings = comboBox1.SelectedItem as PresetSettings;
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                CurrentSettings = comboBox1.SelectedItem as PresetSettings;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = checkBox1.Checked;
            dateTimePicker2.Enabled = checkBox1.Checked;
            buttonImport.Enabled = ((textBoxLocation.Text.Length > 0 || textBoxCodes.Text.Length > 0) &&
                !checkBox1.Checked || Math.Abs((dateTimePicker2.Value - dateTimePicker1.Value).TotalDays) < 30);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            buttonImport.Enabled = ((textBoxLocation.Text.Length > 0 || textBoxCodes.Text.Length > 0) &&
                !checkBox1.Checked || Math.Abs((dateTimePicker2.Value - dateTimePicker1.Value).TotalDays) < 30);
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            buttonImport.Enabled = ((textBoxLocation.Text.Length > 0 || textBoxCodes.Text.Length > 0) &&
                !checkBox1.Checked || Math.Abs((dateTimePicker2.Value - dateTimePicker1.Value).TotalDays) < 30);
        }

    }

    public class GetGeocaches : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTING = "Importing geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_SHOW = "Import geocaches";

        private Utils.API.LiveV6.SearchForGeocachesRequest req = null;
        private int max = 10;
        private int _apiLimit = -1;
        private int _apiLeft = -1;
        private string _errormessage = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_ARCHIVED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_AREACODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_AVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_CLEAR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_CODES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_COMMASEP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_CONTAINER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_DIFMAX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_DIFMIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_EXCLUDE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_FAVMAX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_FAVMIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_FOUNDBYUSERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_GEOCACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_HIDDENBY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_HIDDENBYUSERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_KM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_LIMITS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_MAXLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_MAXPERREQUEST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_MILES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_PMO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_PROPERTIES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_RADIUS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_TERMAX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_TERMIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_TOTALMAX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_TRACKMAX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_TRACKMIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_PRESET));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_CURRENTSETTINGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_DEFAULT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_RELOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_PUBLISHED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_BETWEEN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_AND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GetGeocachesForm.STR_MAX30DAYS));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_CURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_SAVED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PresetsForm.STR_TITLE));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }


        protected override void ImportMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTING, max, 0, true))
            {
                try
                {
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        req.AccessToken = client.Token;

                        var resp = client.Client.SearchForGeocaches(req);
                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                        {
                            if (resp.CacheLimits != null)
                            {
                                _apiLimit = resp.CacheLimits.MaxCacheCount;
                                _apiLeft = resp.CacheLimits.CachesLeft;
                            }
                            Utils.API.Import.AddGeocaches(Core, resp.Geocaches);

                            if (resp.Geocaches.Count() >= req.MaxPerPage && req.MaxPerPage < max)
                            {
                                if (progress.UpdateProgress(STR_IMPORTING, STR_IMPORTING, max, resp.Geocaches.Count()))
                                {
                                    var mreq = new Utils.API.LiveV6.GetMoreGeocachesRequest();
                                    mreq.AccessToken = req.AccessToken;
                                    mreq.GeocacheLogCount = req.GeocacheLogCount;
                                    mreq.MaxPerPage = (int)Math.Min(req.MaxPerPage, max - resp.Geocaches.Count());
                                    mreq.StartIndex = resp.Geocaches.Count();
                                    mreq.TrackableLogCount = req.TrackableLogCount;

                                    while (resp.Status.StatusCode == 0 && resp.Geocaches != null && resp.Geocaches.Count() >= req.MaxPerPage)
                                    {
                                        resp = client.Client.GetMoreGeocaches(mreq);

                                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                        {
                                            if (resp.CacheLimits != null)
                                            {
                                                _apiLimit = resp.CacheLimits.MaxCacheCount;
                                                _apiLeft = resp.CacheLimits.CachesLeft;
                                            }

                                            Utils.API.Import.AddGeocaches(Core, resp.Geocaches);
                                            if (!progress.UpdateProgress(STR_IMPORTING, STR_IMPORTING, max, mreq.StartIndex + resp.Geocaches.Count()))
                                            {
                                                break;
                                            }

                                            mreq.StartIndex += resp.Geocaches.Count();
                                            mreq.MaxPerPage = (int)Math.Min(req.MaxPerPage, max - mreq.StartIndex);
                                            if (mreq.StartIndex >= max - 1)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            _errormessage = resp.Status.StatusMessage;
                                        }

                                    }
                                }
                            }
                        }
                        else
                        {
                            _errormessage = resp.Status.StatusMessage;
                        }
                    }
                }
                catch(Exception e)
                {
                    _errormessage = e.Message;
                }
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                {
                    using (GetGeocachesForm dlg = new GetGeocachesForm(this, Core))
                    {
                        if (dlg.ShowDialog() == DialogResult.OK && dlg.SearchForGeocachesRequestProperties!=null)
                        {
                            req = dlg.SearchForGeocachesRequestProperties;
                            max = dlg.Max;
                            _apiLimit = -1;
                            _errormessage = null;
                            PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            if (_apiLimit >= 0)
                            {
                                Utils.Dialogs.LiveAPICachesLeftForm.ShowMessage(_apiLimit, _apiLeft);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
