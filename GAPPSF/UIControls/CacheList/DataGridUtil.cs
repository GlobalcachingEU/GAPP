using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace GAPPSF.UIControls
{
    public class DataGridSortDescription
    {
        public SortDescriptionCollection SortDescription;
        public IDictionary<DataGridColumn, ListSortDirection?> SortDirection;
    }
    public static class DataGridUtil
    {
        public static DataGridSortDescription SaveSorting(DataGrid grid, ICollectionView view)
        {
            DataGridSortDescription sortDescription = new DataGridSortDescription();

            //Save the current sort order of the columns
            SortDescriptionCollection sortDescriptions = new SortDescriptionCollection();
            if (view != null)
            {
                view.SortDescriptions.ToList().ForEach(sd => sortDescriptions.Add(sd));
            }
            sortDescription.SortDescription = sortDescriptions;

            //save the sort directions (these define the little arrow on the column header...)
            IDictionary<DataGridColumn, ListSortDirection?> sortDirections = new Dictionary<DataGridColumn, ListSortDirection?>();
            foreach (DataGridColumn c in grid.Columns)
            {
                sortDirections.Add(c, c.SortDirection);
            }
            sortDescription.SortDirection = sortDirections;

            return sortDescription;
        }

        public static void RestoreSorting(DataGridSortDescription sortDescription, DataGrid grid, ICollectionView view)
        {
            if (sortDescription.SortDescription != null && sortDescription.SortDescription.Count == 0)
            {
                if (Core.Settings.Default.CacheListEnableAutomaticSorting)
                {
                    if (Core.Settings.Default.CacheListSortOnColumnIndex >= 0 && Core.Settings.Default.CacheListSortOnColumnIndex < grid.Columns.Count)
                    {
                        SortDescription sd = new SortDescription(grid.Columns[Core.Settings.Default.CacheListSortOnColumnIndex].SortMemberPath, Core.Settings.Default.CacheListSortDirection == 0 ? ListSortDirection.Ascending : ListSortDirection.Descending);
                        sortDescription.SortDescription.Add(sd);
                    }
                }
            }
            //restore the column sort order
            if (sortDescription.SortDescription != null && sortDescription.SortDescription.Count > 0)
            {
                sortDescription.SortDescription.ToList().ForEach(x => view.SortDescriptions.Add(x));
                view.Refresh();
            }

            //restore the sort directions. Arrows are nice :)
            foreach (DataGridColumn c in grid.Columns)
            {
                if (sortDescription.SortDirection.ContainsKey(c))
                {
                    c.SortDirection = sortDescription.SortDirection[c];
                }
            }
        }
    }
}
