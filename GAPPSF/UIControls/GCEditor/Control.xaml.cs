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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.GCEditor
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Core.Data.GeocacheData _geocacheData = null;
        public Core.Data.GeocacheData GeocacheData
        {
            get { return _geocacheData; }
            set { SetProperty(ref _geocacheData, value); }
        }

        private string _geocacheCoordinate;
        public string GeocacheCoordinate
        {
            get { return _geocacheCoordinate; }
            set { SetProperty(ref _geocacheCoordinate, value); }
        }

        private string _geocacheCustomCoordinate;
        public string GeocacheCustomCoordinate
        {
            get { return _geocacheCustomCoordinate; }
            set { SetProperty(ref _geocacheCustomCoordinate, value); }
        }

        private double[] _difTerOptions = new double[] { 1, 1.0, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5 };
        public double[] DifTerOptions
        {
            get { return _difTerOptions; }
        }

        private Visibility _warningVisibility;
        public Visibility WarningVisibility
        {
            get { return _warningVisibility; }
            set { SetProperty(ref _warningVisibility, value); }
        }


        public Control()
        {
            InitializeComponent();
            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
            UpdateView();
            WarningVisibility = Core.Settings.Default.GCEditorEditActiveOnly ? Visibility.Collapsed : Visibility.Visible;
            DataContext = this;
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GCEditorEditActiveOnly")
            {
                WarningVisibility = Core.Settings.Default.GCEditorEditActiveOnly ? Visibility.Collapsed : Visibility.Visible;               
            }
        }

        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGeocache")
            {
                UpdateView();
            }
        }

        private void UpdateView()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache == null)
            {
                GeocacheData = null;
                GeocacheCoordinate = null;
                geocacheTypeCombo.SelectedItem = null;
                gcDistance.Text = "";
            }
            else
            {
                Core.Data.GeocacheData gd = new Core.Data.GeocacheData();
                Core.Data.GeocacheData.Copy(Core.ApplicationData.Instance.ActiveGeocache, gd);
                GeocacheData = gd;
                GeocacheCoordinate = Utils.Conversion.GetCoordinatesPresentation(gd.Lat, gd.Lon);
                geocacheTypeCombo.SelectedItem = gd.GeocacheType;
                if (Core.ApplicationData.Instance.ActiveGeocache.GeocacheDistance == null)
                {
                    gcDistance.Text = "";
                }
                else
                {
                    gcDistance.Text = Core.ApplicationData.Instance.ActiveGeocache.GeocacheDistance.ToString();
                }
            }
        }

        private AsyncDelegateCommand _updateCommand;
        public AsyncDelegateCommand UpdateCommand
        {
            get
            {
                if (_updateCommand==null)
                {
                    _updateCommand = new AsyncDelegateCommand(param => UpdateGeocache(param as string),
                        param => canUpdate());
                }
                return _updateCommand;
            }
        }
        private bool canUpdate()
        {
            bool result = false;
            if (Core.ApplicationData.Instance.ActiveDatabase!=null)
            {
                if (Core.Settings.Default.GCEditorEditActiveOnly)
                {
                    result = Core.ApplicationData.Instance.ActiveGeocache != null;
                }
                else
                {
                    result = Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount > 0;
                }
            }
            return result;
        }
        public async Task UpdateGeocache(string param)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                List<Core.Data.Geocache> gcList = null;
                if (Core.Settings.Default.GCEditorEditActiveOnly)
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache!=null)
                    {
                        gcList = new List<Core.Data.Geocache>();
                        gcList.Add(Core.ApplicationData.Instance.ActiveGeocache);
                    }
                }
                else
                {
                    gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                }
                if (gcList!=null)
                {
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                    {
                        GeocacheData.GeocacheType = geocacheTypeCombo.SelectedItem;
                        double? dist = null;
                        if (!string.IsNullOrEmpty(gcDistance.Text))
                        {
                            dist = Utils.Conversion.StringToDouble(gcDistance.Text);
                        }
                        await Task.Run(() =>
                        {
                            try
                            {
                                DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                                int index= 0;
                                using (Utils.ProgressBlock prog = new Utils.ProgressBlock("SavingGeocaches", "SavingGeocaches", gcList.Count, 0))
                                {
                                    foreach (var gc in gcList)
                                    {
                                        if (param=="Name")
                                        {
                                            gc.Name = GeocacheData.Name;
                                        }
                                        else if (param == "PublishedTime")
                                        {
                                            gc.PublishedTime = GeocacheData.PublishedTime;
                                        }
                                        else if (param == "Coordinate")
                                        {
                                            Core.Data.Location l = Utils.Conversion.StringToLocation(GeocacheCoordinate);
                                            if (l != null)
                                            {
                                                gc.Lat = l.Lat;
                                                gc.Lon = l.Lon;
                                            }
                                        }
                                        else if (param == "CustomCoordinate")
                                        {
                                            Core.Data.Location l = Utils.Conversion.StringToLocation(GeocacheCustomCoordinate);
                                            if (l != null)
                                            {
                                                gc.CustomLat = l.Lat;
                                                gc.CustomLon = l.Lon;
                                            }
                                            else
                                            {
                                                gc.CustomLat = null;
                                                gc.CustomLon = null;
                                            }
                                        }
                                        else if (param == "Locked")
                                        {
                                            gc.Locked = GeocacheData.Locked;
                                        }
                                        else if (param == "Available")
                                        {
                                            gc.Available = GeocacheData.Available;
                                        }
                                        else if (param == "Archived")
                                        {
                                            gc.Archived = GeocacheData.Archived;
                                        }
                                        else if (param == "MemberOnly")
                                        {
                                            gc.MemberOnly = GeocacheData.MemberOnly;
                                        }
                                        else if (param == "Found")
                                        {
                                            gc.Found = GeocacheData.Found;
                                        }
                                        else if (param == "Country")
                                        {
                                            gc.Country = GeocacheData.Country;
                                        }
                                        else if (param == "State")
                                        {
                                            gc.State = GeocacheData.State;
                                        }
                                        else if (param == "Municipality")
                                        {
                                            gc.Municipality = GeocacheData.Municipality;
                                        }
                                        else if (param == "City")
                                        {
                                            gc.City = GeocacheData.City;
                                        }
                                        else if (param == "GeocacheType")
                                        {
                                            gc.GeocacheType = GeocacheData.GeocacheType;
                                        }
                                        else if (param == "PlacedBy")
                                        {
                                            gc.PlacedBy = GeocacheData.PlacedBy;
                                        }
                                        else if (param == "Owner")
                                        {
                                            gc.Owner = GeocacheData.Owner;
                                        }
                                        else if (param == "OwnerId")
                                        {
                                            gc.OwnerId = GeocacheData.OwnerId;
                                        }
                                        else if (param == "Terrain")
                                        {
                                            gc.Terrain = GeocacheData.Terrain;
                                        }
                                        else if (param == "Difficulty")
                                        {
                                            gc.Difficulty = GeocacheData.Difficulty;
                                        }
                                        else if (param == "EncodedHints")
                                        {
                                            gc.EncodedHints = GeocacheData.EncodedHints;
                                        }
                                        else if (param == "Url")
                                        {
                                            gc.Url = GeocacheData.Url;
                                        }
                                        else if (param == "Favorites")
                                        {
                                            gc.Favorites = GeocacheData.Favorites;
                                        }
                                        else if (param == "PersonalNote")
                                        {
                                            gc.PersonalNote = GeocacheData.PersonalNote;
                                        }
                                        else if (param == "GeocacheDistance")
                                        {
                                            gc.GeocacheDistance = dist;
                                        }
                                        else
                                        {
                                            //oeps
                                        }

                                        index++;
                                        if (DateTime.Now >= nextUpdate)
                                        {
                                            prog.Update("SavingGeocaches", gcList.Count, index);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                            }
                        });
                    }
                }
            }
        }


        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("GeocacheEditor") as string;
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

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowWidth;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowHeight;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowLeft;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowTop;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Core.Data.Location l = null;
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                l = new Core.Data.Location(Core.ApplicationData.Instance.ActiveGeocache.Lat, Core.ApplicationData.Instance.ActiveGeocache.Lon);
            }
            if (l == null)
            {
                l = Core.ApplicationData.Instance.CenterLocation;
            }
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(l);
            if (dlg.ShowDialog() == true)
            {
                GeocacheCoordinate = dlg.Location.ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Core.Data.Location l = null;
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                if (Core.ApplicationData.Instance.ActiveGeocache.ContainsCustomLatLon)
                {
                    l = new Core.Data.Location((double)Core.ApplicationData.Instance.ActiveGeocache.CustomLat, (double)Core.ApplicationData.Instance.ActiveGeocache.CustomLon);
                }
            }
            if (l == null)
            {
                l = Core.ApplicationData.Instance.CenterLocation;
            }
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(l);
            if (dlg.ShowDialog() == true)
            {
                GeocacheCustomCoordinate = dlg.Location.ToString();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            GeocacheCustomCoordinate = null;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                Core.Data.GeocacheData gc = new Core.Data.GeocacheData();

                int maxId = int.MaxValue - 411120 - 100;
                bool found = true;
                string gcCode = "";
                while (found)
                {
                    maxId--;
                    gcCode = Utils.Conversion.GetCacheCodeFromCacheID(maxId);
                    found = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(gcCode) != null;
                }
                gc.Code = gcCode;
                gc.Archived = false;
                gc.Available = true;
                gc.City = "";
                gc.Container = Utils.DataAccess.GetGeocacheContainer(3);
                gc.Country = "";
                gc.Difficulty = 1.0;
                gc.EncodedHints = "";
                gc.Favorites = 0;
                gc.Flagged = false;
                gc.Found = false;
                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(2);
                gc.Lat = Core.ApplicationData.Instance.CenterLocation.Lat;
                gc.Locked = false;
                gc.Lon = Core.ApplicationData.Instance.CenterLocation.Lon;
                gc.LongDescription = "";
                gc.LongDescriptionInHtml = false;
                gc.MemberOnly = false;
                gc.Municipality = "";
                gc.Name = Localization.TranslationManager.Instance.Translate("DefaultName") as string;
                gc.Notes = "";
                gc.Owner = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName;
                gc.OwnerId = "0";
                gc.PersonalNote = "";
                gc.PlacedBy = gc.Owner;
                gc.PublishedTime = DateTime.Now;
                gc.ShortDescription = "";
                gc.ShortDescriptionInHtml = false;
                gc.State = "";
                gc.Terrain = 1.0;
                gc.Url = "";

                Utils.DataAccess.AddGeocache(Core.ApplicationData.Instance.ActiveDatabase, gc);
                Core.ApplicationData.Instance.ActiveGeocache = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(gcCode);
            }

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            gcDistance.Text = "";
        }

    }
}
