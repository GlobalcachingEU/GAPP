using GAPPSF.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for CacheList.xaml
    /// </summary>
    public partial class CacheList : UserControl, IDisposable, IUIControl
    {
        private int _rowIndex = 0;
        public static CacheListColumnInfoCollection _cacheListColumnInfoCollection = null;

        public CacheList()
        {
            if (_cacheListColumnInfoCollection == null)
            {
                _cacheListColumnInfoCollection = new CacheListColumnInfoCollection();
            }

            InitializeComponent();

            _cacheListColumnInfoCollection.AssignDataGrid(cacheList);
            _cacheListColumnInfoCollection.UpdateDataGrid(cacheList);

            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CacheListShowSelectedOnly" ||
                e.PropertyName == "CacheListShowFlaggedOnly" ||
                e.PropertyName == "CacheListFilterText")
            {
                if ((cacheList.ItemsSource as ListCollectionView) != null)
                {
                    (cacheList.ItemsSource as ListCollectionView).Refresh();
                }
            }
            else if (e.PropertyName == "CacheListColumnInfo")
            {
                _cacheListColumnInfoCollection.UpdateDataGrid(cacheList);
            }
        }

        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
        }

        void cacheList_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            // Get the DataRow corresponding to the DataGridRow that is loading.
            GAPPSF.Core.Data.Geocache item = e.Row.Item as GAPPSF.Core.Data.Geocache;
            if (item != null)
            {
                if (e.Row.GetIndex() < _rowIndex)
                {
                    _rowIndex = 0;
                }
                _rowIndex++;
                e.Row.Header = (_rowIndex).ToString();
            }
            else
            {
                e.Row.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public override string ToString()
        {
            return "Cache list";
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.CacheListWindowWidth;
            }
            set
            {
                Core.Settings.Default.CacheListWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.CacheListWindowHeight;
            }
            set
            {
                Core.Settings.Default.CacheListWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.CacheListWindowLeft;
            }
            set
            {
                Core.Settings.Default.CacheListWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.CacheListWindowTop;
            }
            set
            {
                Core.Settings.Default.CacheListWindowTop = value;
            }
        }

        private void cacheList_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            _cacheListColumnInfoCollection.UpdateFromDataGrid(cacheList);
        }
    }

    public class PathConverter : IValueConverter
    {
        public PathConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GAPPSF.Core.Data.GeocacheType gct = value as GAPPSF.Core.Data.GeocacheType;
            if (gct != null)
            {
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/CacheTypes/{0}.gif", gct.ID.ToString()));
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
            GAPPSF.Core.Data.GeocacheContainer gct = value as GAPPSF.Core.Data.GeocacheContainer;
            if (gct != null)
            {
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Container/{0}.gif", gct.Name.Replace(' ', '_')));
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
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Compass/{0}.gif", Utils.Calculus.GetCompassDirectionFromAngle((int)value).ToString()));
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
            GAPPSF.Core.Data.Geocache gc = value as GAPPSF.Core.Data.Geocache;
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
