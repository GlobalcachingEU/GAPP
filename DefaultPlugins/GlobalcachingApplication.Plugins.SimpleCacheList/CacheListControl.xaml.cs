using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace GlobalcachingApplication.Plugins.SimpleCacheList
{
    /// <summary>
    /// Interaction logic for CacheListControl.xaml
    /// </summary>
    public partial class CacheListControl : UserControl
    {
        private bool _ignoreUpdate = false;

        private Brush _archivedBrush = null;
        private Brush _availableBrush = null;
        private Brush _notAvailableBrush = null;
        private Brush _foundBrush = null;
        private Brush _ownBrush = null;
        private Brush _extrCoordBrush = null;

        private string _filterOnText = "";
        private IEnumerable<Framework.Data.Geocache> _orgSourceList = null;

        public event EventHandler<EventArgs> OnMouseEnter;

        public CacheListControl()
        {
            InitializeComponent();

            try
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch
            {
            }
            _archivedBrush = new SolidColorBrush(Colors.White);
            _availableBrush = new SolidColorBrush(Colors.White);
            _notAvailableBrush = new SolidColorBrush(Colors.White);
            _foundBrush = new SolidColorBrush(Colors.White);
            _ownBrush = new SolidColorBrush(Colors.White);
            _extrCoordBrush = new SolidColorBrush(Colors.White);

            if (Properties.Settings.Default.SortOnColumnIndex >= 0 && Properties.Settings.Default.SortOnColumnIndex < cacheList.Columns.Count)
            {
                this.cacheList.Columns[Properties.Settings.Default.SortOnColumnIndex].SortDirection = Properties.Settings.Default.SortDirection == 0 ? ListSortDirection.Ascending : ListSortDirection.Descending;
            }
        }

        public void InitBrushes()
        {
            _archivedBrush = new SolidColorBrush(new System.Windows.Media.Color()
            {
                A = 255,
                R = Properties.Settings.Default.BkColorArchived.R,
                G = Properties.Settings.Default.BkColorArchived.G,
                B = Properties.Settings.Default.BkColorArchived.B
            });
            _availableBrush = new SolidColorBrush(new System.Windows.Media.Color()
            {
                A = 255,
                R = Properties.Settings.Default.BkColorAvailable.R,
                G = Properties.Settings.Default.BkColorAvailable.G,
                B = Properties.Settings.Default.BkColorAvailable.B
            });
            _notAvailableBrush = new SolidColorBrush(new System.Windows.Media.Color()
            {
                A = 255,
                R = Properties.Settings.Default.BkColorNotAvailable.R,
                G = Properties.Settings.Default.BkColorNotAvailable.G,
                B = Properties.Settings.Default.BkColorNotAvailable.B
            });
            _foundBrush = new SolidColorBrush(new System.Windows.Media.Color()
            {
                A = 255,
                R = Properties.Settings.Default.BkColorFound.R,
                G = Properties.Settings.Default.BkColorFound.G,
                B = Properties.Settings.Default.BkColorFound.B
            });
            _ownBrush = new SolidColorBrush(new System.Windows.Media.Color()
            {
                A = 255,
                R = Properties.Settings.Default.BkColorOwned.R,
                G = Properties.Settings.Default.BkColorOwned.G,
                B = Properties.Settings.Default.BkColorOwned.B
            });
            _extrCoordBrush = new SolidColorBrush(new System.Windows.Media.Color()
            {
                A = 255,
                R = Properties.Settings.Default.BkColorExtraCoord.R,
                G = Properties.Settings.Default.BkColorExtraCoord.G,
                B = Properties.Settings.Default.BkColorExtraCoord.B
            });
            //cacheList.ScrollM
        }

        public DataGrid GeocacheDataGrid
        {
            get { return cacheList; }
        }

        public string FilterOnText
        {
            get { return _filterOnText; }
            set
            {
                if (_filterOnText != value)
                {
                    _filterOnText = value;
                    UpdateListFilter(true);
                }
            }
        }

        private void UpdateListFilter(bool preserveSort)
        {
            if (_orgSourceList != null)
            {
                DataGridSortDescription sort = null;
                if (preserveSort) sort = DataGridUtil.SaveSorting(cacheList);
                if (string.IsNullOrEmpty(_filterOnText))
                {
                    cacheList.ItemsSource = _orgSourceList;
                }
                else
                {
                    string s = _filterOnText.ToLower();
                    cacheList.ItemsSource = (from a in _orgSourceList
                                             where (a.City!=null && a.City.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                             (a.Name!=null && a.Name.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                             (a.Country!=null && a.Country.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                             (a.State != null && a.State.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                             (a.Municipality != null && a.Municipality.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                             (a.Owner != null && a.Owner.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                             a.Code.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0
                                             select a).ToList();
                }
                if (preserveSort) DataGridUtil.RestoreSorting(sort, cacheList);
            }
        }

        public void UpdateDataGrid(IEnumerable<Framework.Data.Geocache> list)
        {
            _orgSourceList = list;
            //using (var d = Dispatcher.DisableProcessing())
            //if (!_ignoreUpdate)
            {
                DataGridSortDescription sort = DataGridUtil.SaveSorting(cacheList);
                cacheList.ItemsSource = null;
                if (string.IsNullOrEmpty(_filterOnText))
                {
                    cacheList.ItemsSource = list;
                }
                else
                {
                    UpdateListFilter(false);
                }
                if (Properties.Settings.Default.EnableAutomaticSorting)
                {
                    DataGridUtil.RestoreSorting(sort, cacheList);
                }
            }
            _ignoreUpdate = false;
        }

        public void UpdateDataGrid(Framework.Data.GeocacheCollection list)
        {
            if (!_ignoreUpdate)
            {
                UpdateDataGrid((from Framework.Data.Geocache a in list select a).ToList());
            }
            _ignoreUpdate = false;
        }

        private void cacheList_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            _ignoreUpdate = true;
        }

        void cacheList_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            // Get the DataRow corresponding to the DataGridRow that is loading.
            Framework.Data.Geocache item = e.Row.Item as Framework.Data.Geocache;
            if (item != null)
            {
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();

                //DataRow row = e.Row;

                // Access cell values values if needed...
                // var colValue = row["ColumnName1]";
                // var colValue2 = row["ColumName2]";

                // Set the background color of the DataGrid row based on whatever data you like from 
                // the row.
                //if (item.Owner == SimpleCacheListForm.FixedCore.GeocachingComAccount.AccountName)
                if (item.IsOwn)
                {
                    e.Row.Background = _ownBrush;
                }
                else if (item.Found)
                {
                    e.Row.Background = _foundBrush;
                }
                else if (item.Archived)
                {
                    e.Row.Background = _archivedBrush;
                }
                else if (item.ContainsCustomLatLon)
                {
                    e.Row.Background = _extrCoordBrush;
                }
                else if (!item.Available)
                {
                    e.Row.Background = _notAvailableBrush;
                }
                else if (item.ContainsCustomLatLon)
                {
                    e.Row.Background = _extrCoordBrush;
                }
                else
                {
                    e.Row.Background = _availableBrush;
                }
            }
            else
            {
                e.Row.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private async void cacheList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                if (dgr != null)
                {
                    Framework.Data.Geocache item = dgr.Item as Framework.Data.Geocache;
                    if (item != null)
                    {
                        await Utils.PluginSupport.ExecuteDefaultActionAsync(SimpleCacheListForm.FixedCore, "GlobalcachingApplication.Plugins.GCView.GeocacheViewer");
                    }
                }
            }
        }

        private void cacheList_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cacheList.Columns.Count; i++)
            {
                sb.AppendFormat("{0}|", cacheList.Columns[i].DisplayIndex);
            }
            Properties.Settings.Default.ColumnOrder = sb.ToString();
            Properties.Settings.Default.Save();
        }

        private void cacheList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (SimpleCacheListForm.FixedCore.ActiveGeocache != null)
                    {
                        Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(SimpleCacheListForm.FixedCore, "GlobalcachingApplication.Plugins.QuickAc.Actions");
                        if (p != null)
                        {
                            p.ActionAsync("Delete|Active");
                        }
                    }
                }));
                e.Handled = true;
            }
        }

        private void cacheList_MouseEnter(object sender, MouseEventArgs e)
        {
            if (OnMouseEnter != null)
            {
                OnMouseEnter(this, EventArgs.Empty);
            }
        }
    }

    public class DataGridSortDescription
    {
        public SortDescriptionCollection SortDescription;
        public IDictionary<DataGridColumn, ListSortDirection?> SortDirection;
    }
    public static class DataGridUtil
    {
        public static DataGridSortDescription SaveSorting(DataGrid grid)
        {
            DataGridSortDescription sortDescription = new DataGridSortDescription();

            //Save the current sort order of the columns
            ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
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

        public static void RestoreSorting(DataGridSortDescription sortDescription, DataGrid grid)
        {
            if (sortDescription.SortDescription != null && sortDescription.SortDescription.Count == 0)
            {
                if (Properties.Settings.Default.EnableAutomaticSorting)
                {
                    if (Properties.Settings.Default.SortOnColumnIndex >= 0 && Properties.Settings.Default.SortOnColumnIndex < grid.Columns.Count)
                    {
                        SortDescription sd = new SortDescription(grid.Columns[Properties.Settings.Default.SortOnColumnIndex].SortMemberPath, Properties.Settings.Default.SortDirection == 0 ? ListSortDirection.Ascending : ListSortDirection.Descending);
                        sortDescription.SortDescription.Add(sd);
                    }
                }
            }
            //restore the column sort order
            if (sortDescription.SortDescription != null && sortDescription.SortDescription.Count > 0)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
                view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
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

    public class PathConverter : IValueConverter
    {
        public PathConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Framework.Data.GeocacheType gct = value as Framework.Data.GeocacheType;
            if (gct != null)
            {
                return Utils.ImageSupport.Instance.GetImagePath(SimpleCacheListForm.FixedCore, Framework.Data.ImageSize.Small, gct);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ContainerConverter : IValueConverter
    {
        public ContainerConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Framework.Data.GeocacheContainer gct = value as Framework.Data.GeocacheContainer;
            if (gct != null)
            {
                return Utils.ImageSupport.Instance.GetImagePath(SimpleCacheListForm.FixedCore, Framework.Data.ImageSize.Small, gct);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CompassPathConverter : IValueConverter
    {
        public CompassPathConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Utils.ImageSupport.Instance.GetImagePath(SimpleCacheListForm.FixedCore, Framework.Data.ImageSize.Small, Utils.Calculus.GetCompassDirectionFromAngle((int)value));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CustomPropertyConverter : IValueConverter
    {
        public CustomPropertyConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && SimpleCacheListForm.FixedAttributes!=null)
            {
                DataGridRow row = value as DataGridRow;
                if (row!=null)
                {
                    Framework.Data.Geocache gc = row.Item as Framework.Data.Geocache;
                    if (gc != null)
                    {
                        int index = int.Parse((string)parameter);
                        return gc.GetCustomAttribute(SimpleCacheListForm.FixedAttributes[index]) as string;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GeocacheCoordConverter : IValueConverter
    {
        public GeocacheCoordConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Framework.Data.Geocache gc = value as Framework.Data.Geocache;
            if (gc != null)
            {
                if (gc.ContainsCustomLatLon)
                {
                    return Utils.Conversion.GetCoordinatesPresentation((double)gc.CustomLat, (double)gc.CustomLon);
                }
                else
                {
                    return Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

}
