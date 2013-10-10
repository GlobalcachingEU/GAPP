using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.Geometry
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_LEVEL = "Level";
        public const string STR_AREA = "Area";
        public const string STR_PARENT = "Parent";
        public const string STR_ADDCREATE = "Add / Create";
        public const string STR_DELETE = "Delete";
        public const string STR_SET = "Set";
        public const string STR_RESTOREDEFAULT = "Restore default";
        public const string STR_IMPORTINGPOLY = "Importing polygons";
        public const string STR_IMPORTINGFILE = "Importing file...";

        private AreasPlugin _ownerPlugin = null;
        private string[] _filenames = null;
        private ManualResetEvent _actionReady = null;
        private Framework.Data.AreaType _selectedAreaType;
        private Framework.Data.AreaInfo _selectedParent;

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public SettingsPanel(AreasPlugin ownerPlugin)
        {
            InitializeComponent();

            _ownerPlugin = ownerPlugin;

            this.labelLevel.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LEVEL);
            this.labelArea.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AREA);
            this.labelParent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PARENT);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PARENT);
            this.buttonAdd.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDCREATE);
            this.buttonDelete.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.buttonSetParent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SET);
            this.buttonRestore.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESTOREDEFAULT);

            comboBoxLevel.Items.AddRange(Enum.GetNames(typeof(Framework.Data.AreaType)));
            comboBoxLevel.SelectedIndex = 0;
        }

        private void comboBoxLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxAreaName.Items.Clear();
            List<Framework.Data.AreaInfo> ail = _ownerPlugin.GetAreasByLevel((Framework.Data.AreaType)Enum.Parse(typeof(Framework.Data.AreaType), comboBoxLevel.SelectedItem.ToString()));
            comboBoxAreaName.Items.AddRange((from a in ail select a).ToArray());

            comboBoxParent.Items.Clear();
            Framework.Data.AreaType selType = (Framework.Data.AreaType)Enum.Parse(typeof(Framework.Data.AreaType), comboBoxLevel.SelectedItem.ToString());
            List<Framework.Data.AreaInfo> ail2 = null;
            switch (selType)
            {
                case Framework.Data.AreaType.City:
                    ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Municipality);
                    break;
                case Framework.Data.AreaType.Municipality:
                    ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.State);
                    break;
                case Framework.Data.AreaType.State:
                    ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Country);
                    break;
                case Framework.Data.AreaType.Other:
                    ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Country);
                    ail2.AddRange(_ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.State));
                    ail2.AddRange(_ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Municipality));
                    ail2.AddRange(_ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Other));
                    break;
            }
            comboBoxParent.Items.Add("-- none --");
            if (ail2 != null)
            {
                comboBoxParent.Items.AddRange((from a in ail2 select a).ToArray());
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _selectedAreaType = (Framework.Data.AreaType)Enum.Parse(typeof(Framework.Data.AreaType), comboBoxLevel.SelectedItem.ToString());
                _selectedParent = comboBoxParent.SelectedItem as Framework.Data.AreaInfo;
                _filenames = openFileDialog1.FileNames;
                _actionReady = new ManualResetEvent(false);
                _actionReady.Reset();
                Thread thrd = new Thread(new ThreadStart(this.processMethod));
                thrd.Start();
                while (!_actionReady.WaitOne(100))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
                _actionReady.Close();
                comboBoxLevel_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void processMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(_ownerPlugin, STR_IMPORTINGPOLY, STR_IMPORTINGFILE, _filenames.Length, 0))
            {
                int index = 0;
                try
                {
                    foreach (string fn in _filenames)
                    {
                        progress.UpdateProgress(STR_IMPORTINGPOLY, STR_IMPORTINGFILE, _filenames.Length, index);
                        if (fn.ToLower().EndsWith(".txt"))
                        {
                            if (!_ownerPlugin.processTextFile(_selectedAreaType, fn, _selectedParent))
                            {
                                break;
                            }
                        }
                        else if (fn.ToLower().EndsWith(".gpx"))
                        {
                            if (!_ownerPlugin.processGpxFile(_selectedAreaType, fn, _selectedParent))
                            {
                                break;
                            }
                        }
                        index++;
                    }
                }
                catch
                {
                }
            }
            _actionReady.Set();
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            _ownerPlugin.RestoreDefaultDatabase();
            this.Enabled = false;
        }

        private void comboBoxAreaName_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonDelete.Enabled = (comboBoxAreaName.SelectedIndex >= 0);
            comboBoxParentOfArea.Items.Clear();
            Framework.Data.AreaInfo ai = comboBoxAreaName.SelectedItem as Framework.Data.AreaInfo;
            if (ai !=null)
            {
                List<Framework.Data.AreaInfo> ail2 = null;
                switch (ai.Level)
                {
                    case Framework.Data.AreaType.City:
                        ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Municipality);
                        break;
                    case Framework.Data.AreaType.Municipality:
                        ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.State);
                        break;
                    case Framework.Data.AreaType.State:
                        ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Country);
                        break;
                    case Framework.Data.AreaType.Other:
                        ail2 = _ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Country);
                        ail2.AddRange(_ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.State));
                        ail2.AddRange(_ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Municipality));
                        ail2.AddRange(_ownerPlugin.GetAreasByLevel(Framework.Data.AreaType.Other));
                        break;
                }
                comboBoxParentOfArea.Items.Add("-- none --");
                if (ail2 != null)
                {
                    comboBoxParentOfArea.Items.AddRange((from a in ail2 select a).ToArray());
                    if (ai.ParentID != null)
                    {
                        comboBoxParentOfArea.SelectedItem = (from a in ail2 where a.ID == ai.ParentID select a).FirstOrDefault();
                    }
                    else
                    {
                        comboBoxParentOfArea.SelectedIndex = 0;
                    }
                }
                else
                {
                    comboBoxParentOfArea.SelectedIndex = 0;
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (comboBoxAreaName.SelectedItem as Framework.Data.AreaInfo!=null)
            {
                _ownerPlugin.deleteArea(comboBoxAreaName.SelectedItem as Framework.Data.AreaInfo);
            }
        }

        private void buttonSetParent_Click(object sender, EventArgs e)
        {
            if (comboBoxAreaName.SelectedItem as Framework.Data.AreaInfo != null)
            {
                _ownerPlugin.setParent(comboBoxAreaName.SelectedItem as Framework.Data.AreaInfo, comboBoxParentOfArea.SelectedItem as Framework.Data.AreaInfo);
            }
        }

    }
}
