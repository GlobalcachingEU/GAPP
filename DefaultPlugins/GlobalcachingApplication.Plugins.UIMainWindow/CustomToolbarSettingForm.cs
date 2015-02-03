using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    public partial class CustomToolbarSettingForm : Form
    {
        public const string STR_TITLE = "Custom toolbar settings";
        public const string STR_SHOWTOOLBAR = "Show custom toolbar";
        public const string STR_OK = "OK";

        public CustomToolbarSettingForm()
        {
            InitializeComponent();
        }

        public CustomToolbarSettingForm(List<FormMain.ToolbarActionProperties> toolbarProperties)
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWTOOLBAR);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);

            this.checkBox1.Checked = PluginSettings.Instance.ToolbarCustom;

            TreeNode tn;

            tn = new TreeNode(FormMain.STR_FILE, imageList1.Images.Count, imageList1.Images.Count);
            treeView1.Nodes.Add(tn);
            var tbpl = from t in toolbarProperties where t.PluginType == Framework.PluginType.InternalStorage select t;
            foreach (FormMain.ToolbarActionProperties tbp in tbpl)
            {
                imageList1.Images.Add(tbp.ButtonImage);
                tn = new TreeNode(getToolbarPropertyText(tbp), imageList1.Images.Count-1, imageList1.Images.Count-1);
                tn.Tag = tbp;
                tn.Checked = GetToolbarPropertySelected(tbp);
                treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(tn);
            }

            tn = new TreeNode(FormMain.STR_SEARCH, imageList1.Images.Count, imageList1.Images.Count);
            treeView1.Nodes.Add(tn);
            tbpl = from t in toolbarProperties where t.PluginType == Framework.PluginType.GeocacheSelectFilter select t;
            foreach (FormMain.ToolbarActionProperties tbp in tbpl)
            {
                imageList1.Images.Add(tbp.ButtonImage);
                tn = new TreeNode(getToolbarPropertyText(tbp), imageList1.Images.Count - 1, imageList1.Images.Count - 1);
                tn.Tag = tbp;
                tn.Checked = GetToolbarPropertySelected(tbp);
                treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(tn);
            }

            tn = new TreeNode(FormMain.STR_LIVEAPI, imageList1.Images.Count, imageList1.Images.Count);
            treeView1.Nodes.Add(tn);
            tbpl = from t in toolbarProperties where t.PluginType == Framework.PluginType.LiveAPI select t;
            foreach (FormMain.ToolbarActionProperties tbp in tbpl)
            {
                imageList1.Images.Add(tbp.ButtonImage);
                tn = new TreeNode(getToolbarPropertyText(tbp), imageList1.Images.Count - 1, imageList1.Images.Count - 1);
                tn.Tag = tbp;
                tn.Checked = GetToolbarPropertySelected(tbp);
                treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(tn);
            }

            tn = new TreeNode(FormMain.STR_WINDOW, imageList1.Images.Count, imageList1.Images.Count);
            treeView1.Nodes.Add(tn);
            tbpl = from t in toolbarProperties where t.PluginType == Framework.PluginType.UIChildWindow select t;
            foreach (FormMain.ToolbarActionProperties tbp in tbpl)
            {
                imageList1.Images.Add(tbp.ButtonImage);
                tn = new TreeNode(getToolbarPropertyText(tbp), imageList1.Images.Count - 1, imageList1.Images.Count - 1);
                tn.Tag = tbp;
                tn.Checked = GetToolbarPropertySelected(tbp);
                treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(tn);
            }

            tn = new TreeNode(FormMain.STR_MAPS, imageList1.Images.Count, imageList1.Images.Count);
            treeView1.Nodes.Add(tn);
            tbpl = from t in toolbarProperties where t.PluginType == Framework.PluginType.Map select t;
            foreach (FormMain.ToolbarActionProperties tbp in tbpl)
            {
                imageList1.Images.Add(tbp.ButtonImage);
                tn = new TreeNode(getToolbarPropertyText(tbp), imageList1.Images.Count - 1, imageList1.Images.Count - 1);
                tn.Tag = tbp;
                tn.Checked = GetToolbarPropertySelected(tbp);
                treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(tn);
            }

            tn = new TreeNode(FormMain.STR_PLUGINS, imageList1.Images.Count, imageList1.Images.Count);
            treeView1.Nodes.Add(tn);
            tbpl = from t in toolbarProperties where t.PluginType == Framework.PluginType.PluginManager select t;
            foreach (FormMain.ToolbarActionProperties tbp in tbpl)
            {
                imageList1.Images.Add(tbp.ButtonImage);
                tn = new TreeNode(getToolbarPropertyText(tbp), imageList1.Images.Count - 1, imageList1.Images.Count - 1);
                tn.Tag = tbp;
                tn.Checked = GetToolbarPropertySelected(tbp);
                treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(tn);
            }

            treeView1.ExpandAll();
        }

        private string getToolbarPropertyText(FormMain.ToolbarActionProperties tbp)
        {
            return string.IsNullOrEmpty(tbp.SubAction) ? Utils.LanguageSupport.Instance.GetTranslation(tbp.Action ?? "") : string.Format("{0} -> {1}", Utils.LanguageSupport.Instance.GetTranslation(tbp.Action ?? ""), Utils.LanguageSupport.Instance.GetTranslation(tbp.SubAction));
        }
        public static  bool GetToolbarPropertySelected(FormMain.ToolbarActionProperties tbp)
        {
            bool result = false;
            if (PluginSettings.Instance.ToolbarCustomButtons != null && PluginSettings.Instance.ToolbarCustomButtons.Count > 0)
            {
                result = PluginSettings.Instance.ToolbarCustomButtons.Contains(string.Format("{0}|{1}|{2}", tbp.PluginType.ToString(), tbp.Action ?? "", tbp.SubAction ?? ""));
            }
            return result;
        }

        public List<FormMain.ToolbarActionProperties> SelectedToolbarProperties
        {
            get
            {
                List<FormMain.ToolbarActionProperties> result = new List<FormMain.ToolbarActionProperties>();
                //only second level
                foreach (TreeNode fn in treeView1.Nodes)
                {
                    foreach (TreeNode tn in fn.Nodes)
                    {
                        if (tn.Checked)
                        {
                            result.Add(tn.Tag as FormMain.ToolbarActionProperties);
                        }
                    }
                }
                return result;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.ToolbarCustom = this.checkBox1.Checked;
            PluginSettings.Instance.ToolbarCustomButtons.Clear();
            List<FormMain.ToolbarActionProperties> props = SelectedToolbarProperties;
            foreach (FormMain.ToolbarActionProperties p in props)
            {
                PluginSettings.Instance.ToolbarCustomButtons.Add(string.Format("{0}|{1}|{2}", p.PluginType.ToString(), p.Action ?? "", p.SubAction ?? ""));
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode tn in e.Node.Nodes)
            {
                tn.Checked = e.Node.Checked;
            }
        }
    }
}
