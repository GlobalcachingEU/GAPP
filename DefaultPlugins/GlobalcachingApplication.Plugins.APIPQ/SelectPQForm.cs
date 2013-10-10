using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIPQ
{
    public partial class SelectPQForm : Form
    {
        public const string STR_TITLE = "Select Pocket Query for download";
        public const string STR_OK = "OK";
        public const string STR_NAME = "Name";
        public const string STR_GENERATED = "Generated";
        public const string STR_Count = "Count";
        public const string STR_TYPE = "Type";
        public const string STR_DOWNLOADABLE = "Downloadable";
        public const string STR_PROCESSED = "Processed";
        public const string STR_UNSELECTALL = "Unselect all";
        public const string STR_SELECTALL = "Select all";

        List<Utils.API.LiveV6.PQData> _selection;

        public SelectPQForm()
        {
            InitializeComponent();
        }

        public SelectPQForm(Utils.API.LiveV6.PQData[] pqData, Hashtable processedPq)
        {
            InitializeComponent();

            foreach (Utils.API.LiveV6.PQData pq in pqData)
            {
                ListViewItem lvi = new ListViewItem(new string[]{pq.Name, pq.DateLastGenerated.ToShortDateString(), pq.PQCount.ToString(), pq.PQSearchType.ToString(), pq.IsDownloadAvailable?"Yes":"No", "-"});
                lvi.Tag = pq;
                if (!pq.IsDownloadAvailable)
                {
                    lvi.BackColor = Color.LightGray;
                    lvi.UseItemStyleForSubItems = true;
                }
                else
                {
                    object o = processedPq[pq.Name];
                    if (o != null)
                    {
                        DateTime processed = (DateTime)o;
                        lvi.SubItems[5].Text = processed.ToShortDateString();
                        if (processed >= pq.DateLastGenerated)
                        {
                            lvi.BackColor = Color.LightGreen;
                            lvi.UseItemStyleForSubItems = true;
                        }
                        else
                        {
                            lvi.Checked = true;
                        }
                    }
                    else
                    {
                        lvi.Checked = true;
                    }
                }
                listView1.Items.Add(lvi);
            }

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.buttonOK.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.buttonSelectAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.buttonUnselectAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UNSELECTALL);
            listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GENERATED);
            listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_Count);
            listView1.Columns[3].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TYPE);
            listView1.Columns[4].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADABLE);
            listView1.Columns[5].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PROCESSED);

            listView1.ListViewItemSorter = new Utils.ListViewColumnSorter();
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).SortColumn = 1;
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).Order = SortOrder.Ascending;
        }

        public List<Utils.API.LiveV6.PQData> SelectedPQs
        {
            get { return _selection; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _selection = new List<Utils.API.LiveV6.PQData>();
            foreach (ListViewItem lvi in listView1.CheckedItems)
            {
                Utils.API.LiveV6.PQData pq = lvi.Tag as Utils.API.LiveV6.PQData;
                if (pq.IsDownloadAvailable)
                {
                    _selection.Add(pq);
                }
            }
        }

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

        private void buttonUnselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++ )
            {
                listView1.Items[i].Checked = false;
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = true;
            }
        }
    }
}
