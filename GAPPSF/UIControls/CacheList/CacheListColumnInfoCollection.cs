using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GAPPSF.UIControls
{
    public class CacheListColumnInfoCollection : ObservableCollection<CacheListColumnInfo>
    {
        private DataGrid _targetGrid = null;

        public CacheListColumnInfoCollection()
        {
        }

        public void AssignDataGrid(DataGrid targetGrid)
        {
            if (_targetGrid == null)
            {
                _targetGrid = targetGrid;

                //load data from settings (if available)
                //Core.Settings.Default.CacheListColumnInfo = "";
                string s = Core.Settings.Default.CacheListColumnInfo;
                if (!string.IsNullOrEmpty(s))
                {
                    string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string l in lines)
                    {
                        string[] parts = l.Split(new char[] { '|' });
                        CacheListColumnInfo c = new CacheListColumnInfo(int.Parse(parts[0]), int.Parse(parts[1]), bool.Parse(parts[2]), targetGrid.Columns[int.Parse(parts[0])].Header as string);
                        Add(c);
                    }
                }

                //get the column infos from datagrid
                for (int i = 0; i < targetGrid.Columns.Count; i++)
                {
                    //add missing
                    var f = (from a in this where a.ColumnIndex == i select a).FirstOrDefault();
                    if (f==null)
                    {
                        Add(new CacheListColumnInfo(i, targetGrid.Columns[i].DisplayIndex, targetGrid.Columns[i].Visibility== System.Windows.Visibility.Visible, targetGrid.Columns[i].Header as string));
                    }
                }

                foreach (var c in this)
                {
                    c.PropertyChanged += c_PropertyChanged;
                }
            }
        }

        public void UpdateDataGrid(DataGrid dg)
        {
            foreach (var c in this)
            {
                dg.Columns[c.ColumnIndex].Visibility = c.Visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                if (c.VisualIndex >= 0)
                {
                    dg.Columns[c.ColumnIndex].DisplayIndex = c.VisualIndex;
                }
            }
        }

        public void UpdateFromDataGrid(DataGrid dg)
        {
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                var f = (from a in this where a.ColumnIndex == i select a).FirstOrDefault();
                if (f != null)
                {
                    f.VisualIndex = dg.Columns[i].DisplayIndex;
                }
            }
            SaveCurrentSettings();
        }

        public void SaveCurrentSettings()
        {
            StringBuilder s = new StringBuilder();
            foreach(var c in this)
            {
                s.AppendLine(string.Format("{0}|{1}|{2}", c.ColumnIndex, c.VisualIndex, c.Visible));
            }
            Core.Settings.Default.CacheListColumnInfo = s.ToString();
        }

        void c_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                SaveCurrentSettings();
            }
        }
    }
}
