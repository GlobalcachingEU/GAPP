using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ExportGarminPOI
{
    public partial class GarminPOIExportForm : Form
    {
        public const string STR_TITLE = "Export Garmin POI";
        public const string STR_EXPORT = "Export";
        public const string STR_POIEXPORTPATH = "Target directory for POI Loader CSV files";
        public const string STR_CLEAREXPORTDIRECTORY = "Clear directory before export";
        public const string STR_EXPORTGEOCACHEPOIS = "Export geocache POIs";
        public const string STR_EXPORTWAYPOINTPOIS = "Export waypoint POIs";
        public const string STR_LIMITS = "Limits";
        public const string STR_NAMELENGTHLIMIT = "Max. POI name length:";
        public const string STR_DESCRIPTIONLENGTHLIMIT = "Max. POI description length:";
        public const string STR_POILOADER = "POI Loader";
        public const string STR_RUNPOILOADER = "Run POI Loader after export";
        public const string STR_PASSDIRECTORY = "Pass directory to POI Loader";
        public const string STR_RUNSILENTLY = "Run silently";

        public const string STR_POINAME = "POI name";
        public const string STR_NAME = "Name";
        public const string STR_CODE = "Code";


        public GarminPOIExportForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);

            this.grpExport.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPORT);
            this.lbPOIExportPath.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_POIEXPORTPATH);
            this.cbClearExportDirectory.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAREXPORTDIRECTORY);
            this.cbExportGeocachePOIs.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPORTGEOCACHEPOIS);
            this.cbExportWaypointPOIs.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPORTWAYPOINTPOIS);
            this.grpLimits.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LIMITS);
            this.lbNameLengthLimit.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAMELENGTHLIMIT);
            this.lbDescriptionLengthLimit.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTIONLENGTHLIMIT);
            this.grpPOILoader.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_POILOADER);
            this.cbRunPOILoader.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RUNPOILOADER);
            this.cbPassDirectoryToPOILoader.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PASSDIRECTORY);
            this.cbRunPOILoaderSilently.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RUNSILENTLY);

            this.grpName.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_POINAME);
            this.rbPOINameGCName.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.rbPOINameGCCode.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);

            this.tbPOIExportPath.Text = PluginSettings.Instance.POIExportPath ?? "";
            this.cbClearExportDirectory.Checked = PluginSettings.Instance.ClearExportDirectory;
            this.cbExportGeocachePOIs.Checked = PluginSettings.Instance.ExportGeocachePOIs;
            this.cbExportWaypointPOIs.Checked = PluginSettings.Instance.ExportWaypointPOIs;
            this.numNameLengthLimit.Value = PluginSettings.Instance.NameLengthLimit;
            this.numDescriptionLengthLimit.Value = PluginSettings.Instance.DescriptionLengthLimit;
            this.cbRunPOILoader.Checked = PluginSettings.Instance.RunPOILoader;
            this.cbPassDirectoryToPOILoader.Checked = PluginSettings.Instance.PassDirectoryToPOILoader;
            this.cbRunPOILoaderSilently.Checked = PluginSettings.Instance.RunPOILoaderSilently;

            if ((PluginSettings.Instance.POINameType == "N") || (PluginSettings.Instance.POINameType == ""))
            {
                this.rbPOINameGCName.Checked = true;
            }
            if ((PluginSettings.Instance.POINameType == "C"))
            {
                this.rbPOINameGCCode.Checked = true;
            }

            if ((PluginSettings.Instance.POILoaderFilename == null) || (PluginSettings.Instance.POILoaderFilename==""))
            {
                //Set Default Position for POI Loader
                this.tbPOILoaderFilename.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                if (this.tbPOILoaderFilename.Text == "") { this.tbPOILoaderFilename.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); }

                this.tbPOILoaderFilename.Text +=
                        Path.DirectorySeparatorChar + "Garmin" + Path.DirectorySeparatorChar + "POI Loader"+
                        Path.DirectorySeparatorChar + "POI Loader.exe";
            }
            else
            {
                this.tbPOILoaderFilename.Text = PluginSettings.Instance.POILoaderFilename;
            }

            //this.llbPOILoaderWeb.Links.Add("http://www.garmin.com/products/poiloader/");

            ToggleOkBtn();
            
        }

        private void lblPOIExportPath_Click(object sender, EventArgs e)
        {

        }

        private void btnPOIExportPath_Click(object sender, EventArgs e)
        {
            if (tbPOIExportPath.Text!="") {
                folderBrowserDialog1.SelectedPath = tbPOIExportPath.Text;
            }else {
                folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); 
            }
                    
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbPOIExportPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        
        private void ToggleOkBtn()
        {
            this.buttonOK.Enabled=(tbPOIExportPath.Text!="") && (this.cbExportGeocachePOIs.Checked || this.cbExportWaypointPOIs.Checked);
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.POIExportPath = this.tbPOIExportPath.Text;
            PluginSettings.Instance.ClearExportDirectory = this.cbClearExportDirectory.Checked;
            PluginSettings.Instance.ExportGeocachePOIs = this.cbExportGeocachePOIs.Checked;
            PluginSettings.Instance.ExportWaypointPOIs = this.cbExportWaypointPOIs.Checked;
            PluginSettings.Instance.NameLengthLimit = (int)this.numNameLengthLimit.Value;
            PluginSettings.Instance.DescriptionLengthLimit = (int)this.numDescriptionLengthLimit.Value;
            PluginSettings.Instance.RunPOILoader = this.cbRunPOILoader.Checked;
            PluginSettings.Instance.POILoaderFilename = this.tbPOILoaderFilename.Text;
            PluginSettings.Instance.PassDirectoryToPOILoader = this.cbPassDirectoryToPOILoader.Checked;
            PluginSettings.Instance.RunPOILoaderSilently = this.cbRunPOILoaderSilently.Checked;
            if (this.rbPOINameGCName.Checked)
            {
                PluginSettings.Instance.POINameType = "N";
            }
            if (this.rbPOINameGCCode.Checked)
            {
                PluginSettings.Instance.POINameType = "C";
            }

            //System.Windows.Forms.MessageBox.Show(String.Join(", ",System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()));

            /*Type t = typeof(GarminPOIExportForm);
            using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.ExportGarminPOI.Resources.CITO.bmp"))
            using (Stream output = File.Create(this.tbPOIExportPath.Text+"\\TEST123.BMP"))
            {
                input.CopyTo(output);
            }*/
        }


        private void btnPOILoaderFilename_Click(object sender, EventArgs e)
        {
            if (tbPOILoaderFilename.Text != "")
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(tbPOILoaderFilename.Text);
                openFileDialog1.FileName = Path.GetFileName(tbPOILoaderFilename.Text);
            }
            
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbPOILoaderFilename.Text = openFileDialog1.FileName;
            }
        }

        private void tbPOIExportPath_TextChanged(object sender, EventArgs e)
        {
            ToggleOkBtn();
        }

        private void cbExportGeocachePOIs_CheckedChanged(object sender, EventArgs e)
        {
            ToggleOkBtn();
        }

        private void cbExportWaypointPOIs_CheckedChanged(object sender, EventArgs e)
        {
            ToggleOkBtn();
        }

        private void llbPOILoaderWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(llbPOILoaderWeb.Text);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
