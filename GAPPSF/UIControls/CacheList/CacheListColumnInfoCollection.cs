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
            //load data from settings (if available)
            string s = Core.Settings.Default.CacheListColumnInfo;
            if (!string.IsNullOrEmpty(s))
            {

            }
        }

        public void AssignDataGrid(DataGrid targetGrid)
        {
            if (_targetGrid == null)
            {
                _targetGrid = targetGrid;

                //get the column infos from datagrid
                //add missing
            }
            //assign visability and visual index
        }
    }
}
