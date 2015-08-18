using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.OV2
{
    public partial class ExportOV2Form : Form
    {
        public const string STR_TITLE = "Export to OV2";
        public const string STR_CODE = "Code";
        public const string STR_NAME = "Name";
        public const string STR_FAVORITES = "Favorites";
        public const string STR_OWNER = "Owner";
        public const string STR_COORDS = "Coordinates";
        public const string STR_NOTE = "Note";
        public const string STR_CONTAINER = "Container";
        public const string STR_DIFFICULTY = "Difficulty";
        public const string STR_TERRAIN = "Terrain";
        public const string STR_HINTS = "Hints";
        public const string STR_CACHETYPE = "Cache type";
        public const string STR_FILE = "File";
        public const string STR_OK = "OK";

        public ExportOV2Form()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);

            this.checkBoxCacheType.Checked = PluginSettings.Instance.gcCacheType;
            this.checkBoxCode.Checked = PluginSettings.Instance.gcCode;
            this.checkBoxContainer.Checked = PluginSettings.Instance.gcContainer;
            this.checkBoxCoords.Checked = PluginSettings.Instance.gcCoord;
            this.checkBoxDifficulty.Checked = PluginSettings.Instance.gcDifficulty;
            this.checkBoxTerrain.Checked = PluginSettings.Instance.gcTerrain;
            this.checkBoxFavorites.Checked = PluginSettings.Instance.gcFavorites;
            this.checkBoxHints.Checked = PluginSettings.Instance.gcHint;
            this.checkBoxName.Checked = PluginSettings.Instance.gcName;
            this.checkBoxNote.Checked = PluginSettings.Instance.gcNote;
            this.checkBoxOwner.Checked = PluginSettings.Instance.gcOwner;
            this.textBox1.Text = PluginSettings.Instance.LastSavedFile ?? "";

            this.checkBoxCacheType.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CACHETYPE);
            this.checkBoxCode.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.checkBoxContainer.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONTAINER);
            this.checkBoxCoords.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COORDS);
            this.checkBoxDifficulty.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DIFFICULTY);
            this.checkBoxFavorites.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FAVORITES);
            this.checkBoxTerrain.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TERRAIN);
            this.checkBoxHints.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HINTS);
            this.checkBoxName.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.checkBoxNote.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTE);
            this.checkBoxOwner.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OWNER);

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILE);
            this.buttonOK.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = textBox1.Text.Length > 0;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.gcCacheType =this.checkBoxCacheType.Checked;
            PluginSettings.Instance.gcCode = this.checkBoxCode.Checked;
            PluginSettings.Instance.gcContainer = this.checkBoxContainer.Checked;
            PluginSettings.Instance.gcCoord =this.checkBoxCoords.Checked;
            PluginSettings.Instance.gcDifficulty = this.checkBoxDifficulty.Checked;
            PluginSettings.Instance.gcFavorites =this.checkBoxFavorites.Checked;
            PluginSettings.Instance.gcHint =this.checkBoxHints.Checked;
            PluginSettings.Instance.gcName = this.checkBoxName.Checked;
            PluginSettings.Instance.gcOwner= this.checkBoxOwner.Checked;
            PluginSettings.Instance.gcNote = this.checkBoxNote.Checked;
            PluginSettings.Instance.gcTerrain = this.checkBoxTerrain.Checked;
            PluginSettings.Instance.LastSavedFile = this.textBox1.Text;
        }
    }
}
