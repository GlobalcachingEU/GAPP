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
using System.Windows.Shapes;

namespace GAPPSF.UIControls.FormulaSolver
{
    /// <summary>
    /// Interaction logic for WaypointSelectorWindow.xaml
    /// </summary>
    public partial class WaypointSelectorWindow : Window, INotifyPropertyChanged
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
                IsWaypointSelected = _selectedWaypoint != null;
            }
        }

        private bool _isWaypointSelected;
        public bool IsWaypointSelected
        {
            get { return _isWaypointSelected; }
            set { SetProperty(ref _isWaypointSelected, value); }
        }


        public WaypointSelectorWindow()
        {
            InitializeComponent();

            Waypoints = new ObservableCollection<Core.Data.Waypoint>();
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                List<Core.Data.Waypoint> wpts = Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                foreach (var w in wpts)
                {
                    Waypoints.Add(w);
                }
            }
            DataContext = this;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = SelectedWaypoint!=null;
            Close();
        }

    }
}
