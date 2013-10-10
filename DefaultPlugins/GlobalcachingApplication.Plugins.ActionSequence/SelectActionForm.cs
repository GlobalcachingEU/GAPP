using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ActionSequence
{
    public partial class SelectActionForm : Form
    {
        public const string STR_TITLE = "Select action";
        public const string STR_SELECT = "Select";
        public const string STR_PLUGIN = "Plugin";
        public const string STR_ACTION = "Action";
        public const string STR_SUBACTION = "Sub action";

        public SelectActionForm()
        {
            InitializeComponent();
        }

        public SelectActionForm(Framework.Interfaces.ICore core)
            : this()
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLUGIN);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTION);
            this.listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SUBACTION);

            char[] sep = new char[]{'|'};
            List<SequenceInfo.ActionInfo> allActions = new List<SequenceInfo.ActionInfo>();
            List<Framework.Interfaces.IPlugin> plugins = core.GetPlugins();
            foreach (var p in plugins)
            {
                List<string> actions = p.GetActionSubactionList(sep[0]);
                foreach (string s in actions)
                {
                    string[] parts = s.Split(sep, 2);
                    SequenceInfo.ActionInfo ai = new SequenceInfo.ActionInfo();
                    ai.Plugin = p.GetType().ToString();
                    ai.Action = parts[0];
                    if (parts.Length == 2)
                    {
                        ai.SubAction = parts[1];
                    }
                    else
                    {
                        ai.SubAction = "";
                    }
                    allActions.Add(ai);
                }
            }
            var allact = (from a in allActions select a).OrderBy(x => x.Plugin).ThenBy(x => x.Action).ThenBy(x => x.SubAction);
            foreach (var a in allact)
            {
                ListViewItem lvi = new ListViewItem(new string[]{
                Utils.LanguageSupport.Instance.GetTranslation(a.Plugin),
                Utils.LanguageSupport.Instance.GetTranslation(a.Action),
                Utils.LanguageSupport.Instance.GetTranslation(a.SubAction)
                });
                lvi.Tag = a;
                listView1.Items.Add(lvi);
            }

            listView1.ListViewItemSorter = new Utils.ListViewColumnSorter();
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).SortColumn = 0;
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).Order = SortOrder.Ascending;

        }

        public SequenceInfo.ActionInfo SelectedAction { get; private set; }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (sender as ListView);
            if (lv != null)
            {
                Utils.ListViewColumnSorter lvcs = (lv.ListViewItemSorter as Utils.ListViewColumnSorter);
                if (lvcs != null)
                {
                    // Determine if clicked column is already the column that is being sorted.
                    if (e.Column == lvcs.SortColumn)
                    {
                        // Reverse the current sort direction for this column.
                        if (lvcs.Order == SortOrder.Ascending)
                        {
                            lvcs.Order = SortOrder.Descending;
                        }
                        else
                        {
                            lvcs.Order = SortOrder.Ascending;
                        }
                    }
                    else
                    {
                        // Set the column number that is to be sorted; default to ascending.
                        lvcs.SortColumn = e.Column;
                        lvcs.Order = SortOrder.Ascending;
                    }

                    // Perform the sort with these new sort options.
                    lv.Sort();
                }
            }
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            button1.Left = panel1.Width / 2 - button1.Width / 2;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = (listView1.SelectedItems.Count > 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                SelectedAction = listView1.SelectedItems[0].Tag as SequenceInfo.ActionInfo;
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
    }
}
