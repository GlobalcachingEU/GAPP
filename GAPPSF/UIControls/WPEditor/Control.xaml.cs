using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GAPPSF.UIControls.WPEditor
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Core.Data.Waypoint> Waypoints { get; private set; }
        private Core.Data.Waypoint _selectedWaypoint;
        public Core.Data.Waypoint SelectedWaypoint
        {
            get { return _selectedWaypoint; }
            set
            {
                SetProperty(ref _selectedWaypoint, value);
                if (_selectedWaypoint == null)
                {
                    WaypointData = null;
                    WaypointLocation = null;
                }
                else
                {
                    Core.Data.WaypointData wd = new Core.Data.WaypointData();
                    Core.Data.WaypointData.Copy(_selectedWaypoint, wd);
                    WaypointData = wd;
                    if (wd.Lat!=null && wd.Lon!=null)
                    {
                        WaypointLocation = Utils.Conversion.GetCoordinatesPresentation((double)wd.Lat, (double)wd.Lon);
                    }
                    else
                    {
                        WaypointLocation = null;
                    }
                    waypointTypeCombo.SelectedItem = wd.WPType;
                }
                IsWaypointSelected = _selectedWaypoint != null;
            }
        }

        private Core.Data.WaypointData _waypointData;
        public Core.Data.WaypointData WaypointData
        {
            get { return _waypointData; }
            set { SetProperty(ref _waypointData, value); }
        }

        private string _waypointLocation;
        public string WaypointLocation
        {
            get { return _waypointLocation; }
            set { SetProperty(ref _waypointLocation, value); }
        }

        private bool _isWaypointSelected;
        public bool IsWaypointSelected
        {
            get { return _isWaypointSelected; }
            set { SetProperty(ref _isWaypointSelected, value); }
        }

        private bool _isGeocacheSelected;
        public bool IsGeocacheSelected
        {
            get { return _isGeocacheSelected; }
            set { SetProperty(ref _isGeocacheSelected, value); }
        }

        public Control()
        {
            InitializeComponent();
            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Waypoints = new ObservableCollection<Core.Data.Waypoint>();
            UpdateView();
            DataContext = this;
        }

        public void Dispose()
        {
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
            Waypoints.Clear();
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                List<Core.Data.Waypoint> wpts = Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                foreach(var w in wpts)
                {
                    Waypoints.Add(w);
                }
            }
            IsGeocacheSelected = Core.ApplicationData.Instance.ActiveGeocache != null;
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("WaypointEditor") as string;
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
                return Core.Settings.Default.WPEditorWindowWidth;
            }
            set
            {
                Core.Settings.Default.WPEditorWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.WPEditorWindowHeight;
            }
            set
            {
                Core.Settings.Default.WPEditorWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.WPEditorWindowLeft;
            }
            set
            {
                Core.Settings.Default.WPEditorWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.WPEditorWindowTop;
            }
            set
            {
                Core.Settings.Default.WPEditorWindowTop = value;
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
                WaypointLocation = dlg.Location.ToString();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            WaypointLocation = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                int index = 1;
                string code = string.Format("{0:00}{1}", index, Core.ApplicationData.Instance.ActiveGeocache.Code.Substring(2));
                while (Core.ApplicationData.Instance.ActiveGeocache.Database.WaypointCollection.GetWaypoint(code) != null)
                {
                    index++;
                    code = string.Format("{0:00}{1}", index, Core.ApplicationData.Instance.ActiveGeocache.Code.Substring(2));
                }
                Core.Data.WaypointData wpi = new Core.Data.WaypointData();
                wpi.Code = code;
                wpi.Comment = "";
                wpi.DataFromDate = DateTime.Now;
                wpi.Description = "";
                wpi.GeocacheCode = Core.ApplicationData.Instance.ActiveGeocache.Code;
                wpi.ID = code;
                wpi.Lat = null;
                wpi.Lon = null;
                wpi.Name = "";
                wpi.Time = DateTime.Now;
                wpi.Url = "";
                wpi.UrlName = "";
                wpi.WPType = Core.ApplicationData.Instance.WaypointTypes[1];

                Utils.DataAccess.AddWaypoint(Core.ApplicationData.Instance.ActiveGeocache.Database, wpi);
                var wp = Core.ApplicationData.Instance.ActiveGeocache.Database.WaypointCollection.GetWaypoint(code);
                Waypoints.Add(wp);
                SelectedWaypoint = wp;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (SelectedWaypoint != null && Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                var wp = SelectedWaypoint;
                wp.DeleteRecord();
                Core.ApplicationData.Instance.ActiveGeocache.Database.WaypointCollection.Remove(wp);
                Waypoints.Remove(wp);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (SelectedWaypoint != null && WaypointData!=null)
            {
                SelectedWaypoint.BeginUpdate();
                try
                {
                    SelectedWaypoint.WPType = waypointTypeCombo.SelectedItem;

                    SelectedWaypoint.Comment = WaypointData.Comment;
                    SelectedWaypoint.Description = WaypointData.Description;
                    SelectedWaypoint.Lat = WaypointData.Lat;
                    SelectedWaypoint.Lon = WaypointData.Lon;
                    SelectedWaypoint.Name = WaypointData.Name;
                    SelectedWaypoint.Url = WaypointData.Url;
                    SelectedWaypoint.UrlName = WaypointData.UrlName;
                }
                catch(Exception ex)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                }
                SelectedWaypoint.EndUpdate();
                wpListControl.Items.Refresh();
            }
        }

    }
}
