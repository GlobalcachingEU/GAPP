using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace GlobalcachingApplication.Plugins.Startup
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_INFO = "Select the plugin/action that should be started after startup.";
        public const string STR_SORT = "Sort";

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public void Apply()
        {
            PluginSettings.Instance.Startup.Clear();
            foreach (ListViewItem lvi in listView1.Items)
            {
                if (lvi.Checked)
                {
                    PluginSettings.Instance.Startup.Add(lvi.Tag as string);
                }
            }
        }

        public SettingsPanel(Framework.Interfaces.ICore core)
            : this()
        {
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INFO);
            this.buttonSort.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SORT);

            //first the selected
            if (PluginSettings.Instance.Startup != null && PluginSettings.Instance.Startup.Count > 0)
            {
                foreach (string s in PluginSettings.Instance.Startup)
                {
                    string[] parts = s.Split(new char[] { '@' }, 2);
                    Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(core, parts[0]);
                    if (p != null)
                    {
                        List<string> actions = p.GetActionSubactionList('@');
                        if (actions.Contains(parts[1]))
                        {
                            string[] act = parts[1].Split(new char[] { '@' });
                            ListViewItem lvi = new ListViewItem(new string[] { p.GetType().ToString(), Utils.LanguageSupport.Instance.GetTranslation(parts[0]), act.Length == 1 ? "" : Utils.LanguageSupport.Instance.GetTranslation(act[1]), "" });
                            lvi.Tag = s;
                            lvi.Checked = true;
                            listView1.Items.Add(lvi);
                        }
                    }
                }
            }

            List<Framework.Interfaces.IPlugin> pins = core.GetPlugins();
            foreach (Framework.Interfaces.IPlugin p in pins)
            {
                List<string> actions = p.GetActionSubactionList('@');
                foreach (string act in actions)
                {
                    string stag = string.Format("{0}@{1}", p.GetType(), act);
                    if ((PluginSettings.Instance.Startup == null || !PluginSettings.Instance.Startup.Contains(stag)))
                    {
                        string[] parts = act.Split(new char[] { '@' });
                        ListViewItem lvi = new ListViewItem(new string[] { p.GetType().ToString(), Utils.LanguageSupport.Instance.GetTranslation(parts[0]), parts.Length == 1 ? "" : Utils.LanguageSupport.Instance.GetTranslation(parts[1]), "" });
                        lvi.Tag = stag;
                        lvi.Checked = false;
                        listView1.Items.Add(lvi);
                    }
                }
            }
            ListViewColumnSorter lvs = new ListViewColumnSorter();            
            listView1.ListViewItemSorter = lvs;
            listView1.Sort();
        }

        private void buttonSort_Click(object sender, EventArgs e)
        {
            listView1.Sort();
        }

        private int[] orderedCheckedIndices()
        {
            List<int> lst = new List<int>();
            foreach (int i in listView1.CheckedIndices)
            {
                lst.Add(i);
            }
            return (from i in lst orderby i select i).ToArray();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            bool up = false;
            bool down = false;

            if (listView1.SelectedIndices.Count > 0)
            {
                if (listView1.Items[listView1.SelectedIndices[0]].Checked)
                {
                    int selIndex = (int)listView1.SelectedIndices[0];
                    int[] sel = orderedCheckedIndices();
                    up = ((from i in sel where i < selIndex select i).Count()>0);
                    down = ((from i in sel where i > selIndex select i).Count() > 0);
                }
            }

            this.buttonUp.Enabled = up;
            this.buttonDown.Enabled = down;
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            listView1_ItemSelectionChanged(sender, null);
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            int fromIdx = listView1.SelectedIndices[0];
            int[] sel = orderedCheckedIndices();
            int beforeIndex = (from i in sel where i < fromIdx orderby i descending select i).FirstOrDefault();
            ListViewItem lvi = listView1.Items[fromIdx];
            listView1.Items.RemoveAt(fromIdx);
            listView1.Items.Insert(beforeIndex, lvi);
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            int fromIdx = listView1.SelectedIndices[0];
            int[] sel = orderedCheckedIndices();
            int afterIndex = (from i in sel where i > fromIdx orderby i ascending select i).FirstOrDefault();
            ListViewItem lvi = listView1.Items[fromIdx];
            listView1.Items.RemoveAt(fromIdx);
            listView1.Items.Insert(afterIndex, lvi);
        }
        
    }

    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            if (listviewX.Checked && listviewY.Checked)
            {
                return 0;
            }
            else if (listviewX.Checked && !listviewY.Checked)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }

    }

}
