using GAPPSF.Commands;
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
        private ICollectionView _geocacheCollectionView = null;
        private static CacheListColumnInfoCollection _cacheListColumnInfoCollection = null;

        public CacheList()
        {
            if (_cacheListColumnInfoCollection == null)
            {
                _cacheListColumnInfoCollection = new CacheListColumnInfoCollection();
            }

            InitializeComponent();
            DataContext = this;

            if (Core.Settings.Default.CacheListEnableAutomaticSorting && Core.Settings.Default.CacheListSortOnColumnIndex >= 0 && Core.Settings.Default.CacheListSortOnColumnIndex < cacheList.Columns.Count)
            {
                this.cacheList.Columns[Core.Settings.Default.CacheListSortOnColumnIndex].SortDirection = Core.Settings.Default.CacheListSortDirection == 0 ? ListSortDirection.Ascending : ListSortDirection.Descending;
            }


            ColorPickerArchived.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.ArchivedRowColor);
            ColorPickerDisabled.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.DisabledRowColor);
            ColorPickerOwn.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.IsOwnRowColor);
            ColorPickerFound.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.FoundRowColor);

            setGeocacheCollectionView();
            _cacheListColumnInfoCollection.AssignDataGrid(cacheList);
            _cacheListColumnInfoCollection.UpdateDataGrid(cacheList);

            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Localization.TranslationManager.Instance.LanguageChanged += Instance_LanguageChanged;

            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                cacheList.ScrollIntoView(Core.ApplicationData.Instance.ActiveGeocache);
            }
        }

        void Instance_LanguageChanged(object sender, EventArgs e)
        {
            foreach(var item in ColumnInfoCollection)
            {
                item.Name = cacheList.Columns[item.ColumnIndex].Header as string;
            }
        }

        AsyncDelegateCommand _copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new AsyncDelegateCommand(param => this.CopyGeocache(), param => cacheList.SelectedItems.Count > 0);
                }
                return _copyCommand;
            }
        }
        async private Task CopyGeocache()
        {
            if (cacheList.SelectedItems.Count>0)
            {
                List<string> gcList = new List<string>();
                foreach (Core.Data.Geocache gc in cacheList.SelectedItems)
                {
                    gcList.Add(gc.Code);
                }
                await Clipboard.ClipboardHandler.Instance.CopyGeocachesAsync(Core.ApplicationData.Instance.ActiveDatabase, gcList);
            }
        }

        AsyncDelegateCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new AsyncDelegateCommand(param => this.DeleteGeocache(false), param => cacheList.SelectedItems.Count > 0);
                }
                return _deleteCommand;
            }
        }
        AsyncDelegateCommand _deleteIgnoreCommand;
        public ICommand DeleteIgnoreCommand
        {
            get
            {
                if (_deleteIgnoreCommand == null)
                {
                    _deleteIgnoreCommand = new AsyncDelegateCommand(param => this.DeleteGeocache(true), param => cacheList.SelectedItems.Count > 0);
                }
                return _deleteIgnoreCommand;
            }
        }
        async private Task DeleteGeocache(bool ignore)
        {
            if (cacheList.SelectedItems.Count > 0)
            {
                List<Core.Data.Geocache> gcList = new List<Core.Data.Geocache>();
                foreach (Core.Data.Geocache gc in cacheList.SelectedItems)
                {
                    gcList.Add(gc);
                }
                Core.ApplicationData.Instance.ActiveGeocache = null;
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                        {
                            int index = 0;
                            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("DeletingGeocaches", "DeletingGeocaches", gcList.Count, 0, true))
                            {
                                foreach (var gc in gcList)
                                {
                                    Utils.DataAccess.DeleteGeocache(gc);
                                    if (ignore)
                                    {
                                        Core.Settings.Default.AddIgnoreGeocacheCodes((new string[] { gc.Code }).ToList());
                                    }
                                    index++;

                                    if (DateTime.Now >= nextUpdate)
                                    {
                                        if (!prog.Update("DeletingGeocaches", gcList.Count, index))
                                        {
                                            break;
                                        }
                                        nextUpdate = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            }
                        });
                }
            }
        }

        public static CacheListColumnInfoCollection ColumnInfoCollection
        {
            get { return _cacheListColumnInfoCollection; }
        }

        private bool GeocacheViewFilter(object o)
        {
            Core.Data.Geocache gc = o as Core.Data.Geocache;
            bool result = !Core.Settings.Default.CacheListShowSelectedOnly || gc.Selected;
            result &= !Core.Settings.Default.CacheListShowFlaggedOnly || gc.Flagged;
            string ft = Core.Settings.Default.CacheListFilterText;
            if (!string.IsNullOrEmpty(ft))
            {
                if (result)
                {
                    string s = gc.Code;
                    result = s.IndexOf(ft, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    s = gc.Name;
                    result |= s != null && s.IndexOf(ft, StringComparison.CurrentCultureIgnoreCase) >= 0;
                }
            }
            return result;
        }

        private void setGeocacheCollectionView()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                DataGridSortDescription sort = null;
                sort = DataGridUtil.SaveSorting(cacheList, this._geocacheCollectionView);
                this._geocacheCollectionView = CollectionViewSource.GetDefaultView(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
                this._geocacheCollectionView.Filter = this.GeocacheViewFilter;
                cacheList.ItemsSource = this._geocacheCollectionView;
                if (sort != null && Core.Settings.Default.CacheListEnableAutomaticSorting)
                {
                    DataGridUtil.RestoreSorting(sort, cacheList, this._geocacheCollectionView);
                }
            }
            else
            {
                cacheList.ItemsSource = null;
            }
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDatabase")
            {
                setGeocacheCollectionView();
            }
            else if (e.PropertyName == "AccountInfos")
            {
                if ((cacheList.ItemsSource as ListCollectionView) != null)
                {
                    (cacheList.ItemsSource as ListCollectionView).Refresh();
                }
            }
            else if (e.PropertyName=="ActiveGeocache")
            {
                if (Core.ApplicationData.Instance.ActiveGeocache!=null)
                {
                    cacheList.ScrollIntoView(Core.ApplicationData.Instance.ActiveGeocache);
                }
            }
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
            for (int i = 0; i < cacheList.Columns.Count; i++)
            {
                if (cacheList.Columns[i].SortDirection != null)
                {
                    Core.Settings.Default.CacheListSortOnColumnIndex = i;
                    Core.Settings.Default.CacheListSortDirection = cacheList.Columns[i].SortDirection == ListSortDirection.Ascending ? 0 : 1;
                }
            }

            Localization.TranslationManager.Instance.LanguageChanged -= Instance_LanguageChanged;
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
        }

        RelayCommand _invertSelectionCommand;
        public ICommand InvertSelectionCommand
        {
            get
            {
                if (_invertSelectionCommand == null)
                {
                    _invertSelectionCommand = new RelayCommand(param => this.invertSelection(),
                        param => cacheList.SelectedItems.Count>0);
                }
                return _invertSelectionCommand;
            }
        }
        public void invertSelection()
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
            {
                foreach (Core.Data.Geocache gc in cacheList.SelectedItems)
                {
                    gc.Selected = !gc.Selected;
                }
            }
        }


        void cacheList_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            // Get the DataRow corresponding to the DataGridRow that is loading.
            GAPPSF.Core.Data.Geocache item = e.Row.Item as GAPPSF.Core.Data.Geocache;
            if (item != null)
            {
                e.Row.Header = (e.Row.GetIndex()+1).ToString();
            }
            else
            {
                e.Row.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("CacheList") as string;
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

        private void ColorPickerArchived_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.ArchivedRowColor = ColorPickerArchived.SelectedColor.ToString();
        }

        private void ColorPickerDisabled_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.DisabledRowColor = ColorPickerDisabled.SelectedColor.ToString();
        }

        private void ColorPickerOwn_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.IsOwnRowColor = ColorPickerOwn.SelectedColor.ToString();
        }

        private void ColorPickerFound_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.FoundRowColor = ColorPickerFound.SelectedColor.ToString();
        }
    }


}
