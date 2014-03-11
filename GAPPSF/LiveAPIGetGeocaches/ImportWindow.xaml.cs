using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GAPPSF.LiveAPIGetGeocaches
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImportWindow()
        {
            InitializeComponent();

            long[] lt = new long[] { 2, 3, 4, 5, 6, 8, 9, 11, 12, 13, 137, 453, 605, 1304, 1858, 3653, 3773, 3774, 4738 };
            var at = (from a in cacheTypes.AvailableTypes where !lt.Contains(a.Item.ID) select a).ToList();
            foreach (var b in at)
            {
                cacheTypes.AvailableTypes.Remove(b);
            }
            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCCacheTypes))
            {
                string[] parts = Core.Settings.Default.LiveAPIGetGCCacheTypes.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    int id = int.Parse(s);
                    (from a in cacheTypes.AvailableTypes where a.Item.ID == id select a).FirstOrDefault().IsChecked = true;
                }
            }
            foreach (var a in cacheTypes.AvailableTypes)
            {
                a.PropertyChanged += a_PropertyChanged;
            }

            cacheContainers.AvailableTypes.Remove((from a in cacheContainers.AvailableTypes where a.Item.ID == 0 select a).FirstOrDefault());
            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCCacheContainers))
            {
                string[] parts = Core.Settings.Default.LiveAPIGetGCCacheContainers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    int id = int.Parse(s);
                    (from a in cacheContainers.AvailableTypes where a.Item.ID == id select a).FirstOrDefault().IsChecked = true;
                }
            }
            foreach (var a in cacheContainers.AvailableTypes)
            {
                a.PropertyChanged += b_PropertyChanged;
            }

            DataContext = this;
        }

        private void b_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            saveCacheContainers();
        }

        private void a_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            saveCacheTypes();
        }

        private void saveCacheTypes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in cacheTypes.AvailableTypes)
            {
                if (a.IsChecked)
                {
                    sb.AppendLine(a.Item.ID.ToString());
                }
            }
            Core.Settings.Default.LiveAPIGetGCCacheTypes = sb.ToString();
        }
        private void saveCacheContainers()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in cacheContainers.AvailableTypes)
            {
                if (a.IsChecked)
                {
                    sb.AppendLine(a.Item.ID.ToString());
                }
            }
            Core.Settings.Default.LiveAPIGetGCCacheContainers = sb.ToString();
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        private AsyncDelegateCommand _importCommand;
        public AsyncDelegateCommand ImportCommand
        {
            get
            {
                if (_importCommand==null)
                {
                    _importCommand = new AsyncDelegateCommand(param => PerformImport(), param => canImport());
                }
                return _importCommand;
            }
        }
        private bool canImport()
        {
            bool result = false;
            if ((from a in cacheTypes.AvailableTypes where a.IsChecked select a).Count()>0
                && (from a in cacheContainers.AvailableTypes where a.IsChecked select a).Count() > 0)
            {
                if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCCodes)
                    || !string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCLocation))
                result = true;
            }
            return result;
        }
        public async Task PerformImport()
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var SearchForGeocachesRequestProperties = new LiveAPI.LiveV6.SearchForGeocachesRequest();
                        SearchForGeocachesRequestProperties.IsLite = Core.Settings.Default.LiveAPIMemberTypeId == 1;
                        SearchForGeocachesRequestProperties.MaxPerPage = Core.Settings.Default.LiveAPIImportGeocachesBatchSize;
                        if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCLocation) && string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCCodes))
                        {
                            double dist = Core.Settings.Default.LiveAPIGetGCRadius * 1000.0;
                            if (Core.Settings.Default.LiveAPIGetGCLocationKm== UIControls.GeocacheFilter.BooleanEnum.False) dist *= 1.6214;
                            Core.Data.Location l = Utils.Conversion.StringToLocation(Core.Settings.Default.LiveAPIGetGCLocation);

                            SearchForGeocachesRequestProperties.PointRadius = new LiveAPI.LiveV6.PointRadiusFilter();
                            SearchForGeocachesRequestProperties.PointRadius.DistanceInMeters = (long)dist;
                            SearchForGeocachesRequestProperties.PointRadius.Point = new LiveAPI.LiveV6.LatLngPoint();
                            SearchForGeocachesRequestProperties.PointRadius.Point.Latitude = l.Lat;
                            SearchForGeocachesRequestProperties.PointRadius.Point.Longitude = l.Lon;
                        }
                        if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCCodes))
                        {
                            string[] parts = Core.Settings.Default.LiveAPIGetGCCodes.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            SearchForGeocachesRequestProperties.CacheCode = new LiveAPI.LiveV6.CacheCodeFilter();
                            SearchForGeocachesRequestProperties.CacheCode.CacheCodes = (from s in parts select s.ToUpper()).ToArray();
                        }
                        SearchForGeocachesRequestProperties.GeocacheLogCount = Core.Settings.Default.LiveAPIGetGCMaximumLogs;
                        if (Core.Settings.Default.LiveAPIMemberTypeId > 1)
                        {
                            SearchForGeocachesRequestProperties.FavoritePoints = new LiveAPI.LiveV6.FavoritePointsFilter();
                            SearchForGeocachesRequestProperties.FavoritePoints.MinFavoritePoints = Core.Settings.Default.LiveAPIGetGCFavMin;
                            SearchForGeocachesRequestProperties.FavoritePoints.MaxFavoritePoints = Core.Settings.Default.LiveAPIGetGCFavMax;
                            SearchForGeocachesRequestProperties.Difficulty = new LiveAPI.LiveV6.DifficultyFilter();
                            SearchForGeocachesRequestProperties.Difficulty.MinDifficulty = Core.Settings.Default.LiveAPIGetGCDifMin;
                            SearchForGeocachesRequestProperties.Difficulty.MaxDifficulty = Core.Settings.Default.LiveAPIGetGCDifMax;
                            SearchForGeocachesRequestProperties.Terrain = new LiveAPI.LiveV6.TerrainFilter();
                            SearchForGeocachesRequestProperties.Terrain.MinTerrain = Core.Settings.Default.LiveAPIGetGCTerMin;
                            SearchForGeocachesRequestProperties.Terrain.MaxTerrain = Core.Settings.Default.LiveAPIGetGCTerMax;

                            if (Core.Settings.Default.LiveAPIGetGCExcludeArchived!=null ||
                                Core.Settings.Default.LiveAPIGetGCExcludeAvailable != null ||
                                Core.Settings.Default.LiveAPIGetGCExcludePMO != null)
                            {
                                SearchForGeocachesRequestProperties.GeocacheExclusions = new LiveAPI.LiveV6.GeocacheExclusionsFilter();
                                SearchForGeocachesRequestProperties.GeocacheExclusions.Archived = Core.Settings.Default.LiveAPIGetGCExcludeArchived;
                                SearchForGeocachesRequestProperties.GeocacheExclusions.Available = Core.Settings.Default.LiveAPIGetGCExcludeAvailable;
                                SearchForGeocachesRequestProperties.GeocacheExclusions.Premium = Core.Settings.Default.LiveAPIGetGCExcludePMO;
                            }
                        }
                        if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCName))
                        {
                            SearchForGeocachesRequestProperties.GeocacheName = new LiveAPI.LiveV6.GeocacheNameFilter();
                            SearchForGeocachesRequestProperties.GeocacheName.GeocacheName = Core.Settings.Default.LiveAPIGetGCName;
                        }
                        if (Core.Settings.Default.LiveAPIMemberTypeId > 1)
                        {
                            long[] gcTypeIds = (from s in cacheTypes.AvailableTypes where s.IsChecked select (long)s.Item.ID).ToArray();
                            if (gcTypeIds.Length < cacheTypes.AvailableTypes.Count)
                            {
                                SearchForGeocachesRequestProperties.GeocacheType = new LiveAPI.LiveV6.GeocacheTypeFilter();
                                SearchForGeocachesRequestProperties.GeocacheType.GeocacheTypeIds = gcTypeIds;
                            }
                            long[] cntTypeIds = (from s in cacheContainers.AvailableTypes where s.IsChecked select (long)s.Item.ID).ToArray();
                            if (cntTypeIds.Length < cacheContainers.AvailableTypes.Count)
                            {
                                SearchForGeocachesRequestProperties.GeocacheContainerSize = new LiveAPI.LiveV6.GeocacheContainerSizeFilter();
                                SearchForGeocachesRequestProperties.GeocacheContainerSize.GeocacheContainerSizeIds = cntTypeIds;
                            }
                            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCExcludeFoundBy))
                            {
                                SearchForGeocachesRequestProperties.NotFoundByUsers = new LiveAPI.LiveV6.NotFoundByUsersFilter();
                                string[] parts = Core.Settings.Default.LiveAPIGetGCExcludeFoundBy.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                SearchForGeocachesRequestProperties.NotFoundByUsers.UserNames = (from s in parts select s.Trim()).ToArray();
                            }
                            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCExcludeHiddenBy))
                            {
                                SearchForGeocachesRequestProperties.NotHiddenByUsers = new LiveAPI.LiveV6.NotHiddenByUsersFilter();
                                string[] parts = Core.Settings.Default.LiveAPIGetGCExcludeHiddenBy.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                SearchForGeocachesRequestProperties.NotHiddenByUsers.UserNames = (from s in parts select s.Trim()).ToArray();
                            }
                        }
                        if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCHiddenBy))
                        {
                            SearchForGeocachesRequestProperties.HiddenByUsers = new LiveAPI.LiveV6.HiddenByUsersFilter();
                            string[] parts = Core.Settings.Default.LiveAPIGetGCHiddenBy.Split(new char[] { ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            SearchForGeocachesRequestProperties.HiddenByUsers.UserNames = (from s in parts select s.Trim()).ToArray();
                        }
                        if (Core.Settings.Default.LiveAPIMemberTypeId > 1)
                        {
                            SearchForGeocachesRequestProperties.TrackableCount = new LiveAPI.LiveV6.TrackableCountFilter();
                            SearchForGeocachesRequestProperties.TrackableCount.MinTrackables = Core.Settings.Default.LiveAPIGetGCTrackMin;
                            SearchForGeocachesRequestProperties.TrackableCount.MaxTrackables = Core.Settings.Default.LiveAPIGetGCTrackMax;
                            SearchForGeocachesRequestProperties.TrackableLogCount = 0;
                            if (Core.Settings.Default.LiveAPIGetGCBetweenDates)
                            {
                                SearchForGeocachesRequestProperties.CachePublishedDate = new LiveAPI.LiveV6.CachePublishedDateFilter();
                                SearchForGeocachesRequestProperties.CachePublishedDate.Range = new LiveAPI.LiveV6.DateRange();
                                SearchForGeocachesRequestProperties.CachePublishedDate.Range.StartDate = Core.Settings.Default.LiveAPIGetGCMinDate < Core.Settings.Default.LiveAPIGetGCMaxDate ? Core.Settings.Default.LiveAPIGetGCMinDate : Core.Settings.Default.LiveAPIGetGCMaxDate;
                                SearchForGeocachesRequestProperties.CachePublishedDate.Range.EndDate = Core.Settings.Default.LiveAPIGetGCMaxDate > Core.Settings.Default.LiveAPIGetGCMinDate ? Core.Settings.Default.LiveAPIGetGCMaxDate : Core.Settings.Default.LiveAPIGetGCMinDate;
                            }
                        }
                        LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, SearchForGeocachesRequestProperties, Core.Settings.Default.LiveAPIGetGCTotalMaximum);
                    }
                    catch (Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.Default.LiveAPIGetGCLocation = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Core.Data.Location l;
            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIGetGCLocation))
            {
                l = Utils.Conversion.StringToLocation(Core.Settings.Default.LiveAPIGetGCLocation);
            }
            else{
                l = Core.ApplicationData.Instance.CenterLocation;
            }
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(l);
            if (dlg.ShowDialog()==true)
            {
                Core.Settings.Default.LiveAPIGetGCLocation = dlg.Location.ToString();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dlg = new ImportByRadiusWindow();
            if (dlg.ShowDialog()==true)
            {
                Core.Settings.Default.LiveAPIGetGCLocation = Utils.Conversion.GetCoordinatesPresentation(dlg.Center);
                Core.Settings.Default.LiveAPIGetGCRadius = Math.Min(dlg.Radius, 50);
                Core.Settings.Default.LiveAPIGetGCLocationKm = UIControls.GeocacheFilter.BooleanEnum.True;
            }
        }

    }
}
