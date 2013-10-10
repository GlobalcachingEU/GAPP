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

            this.checkBoxCacheType.Checked = Properties.Settings.Default.gcCacheType;
            this.checkBoxCode.Checked = Properties.Settings.Default.gcCode;
            this.checkBoxContainer.Checked = Properties.Settings.Default.gcContainer;
            this.checkBoxCoords.Checked = Properties.Settings.Default.gcCoord;
            this.checkBoxDifficulty.Checked = Properties.Settings.Default.gcDifficulty;
            this.checkBoxTerrain.Checked = Properties.Settings.Default.gcTerrain;
            this.checkBoxFavorites.Checked = Properties.Settings.Default.gcFavorites;
            this.checkBoxHints.Checked = Properties.Settings.Default.gcHint;
            this.checkBoxName.Checked = Properties.Settings.Default.gcName;
            this.checkBoxNote.Checked = Properties.Settings.Default.gcOwner;
            this.checkBoxOwner.Checked = Properties.Settings.Default.gcTerrain;
            this.textBox1.Text = Properties.Settings.Default.LastSavedFile ?? "";

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
            Properties.Settings.Default.gcCacheType =this.checkBoxCacheType.Checked;
            Properties.Settings.Default.gcCode = this.checkBoxCode.Checked;
            Properties.Settings.Default.gcContainer = this.checkBoxContainer.Checked;
            Properties.Settings.Default.gcCoord =this.checkBoxCoords.Checked;
            Properties.Settings.Default.gcDifficulty = this.checkBoxDifficulty.Checked;
            Properties.Settings.Default.gcFavorites =this.checkBoxFavorites.Checked;
            Properties.Settings.Default.gcHint =this.checkBoxHints.Checked;
            Properties.Settings.Default.gcName = this.checkBoxName.Checked;
            Properties.Settings.Default.gcOwner= this.checkBoxNote.Checked;
            Properties.Settings.Default.gcTerrain = this.checkBoxOwner.Checked;
            Properties.Settings.Default.LastSavedFile = this.textBox1.Text;
            Properties.Settings.Default.Save();
        }
    }
}
