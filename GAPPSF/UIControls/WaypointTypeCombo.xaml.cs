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

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for WaypointTypeCombo.xaml
    /// </summary>
    public partial class WaypointTypeCombo : UserControl, INotifyPropertyChanged
    {
        private static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(Core.Data.WaypointType), typeof(WaypointTypeCombo), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(DependencyObject source,
                DependencyPropertyChangedEventArgs e)
        {
            WaypointTypeCombo control = source as WaypointTypeCombo;
            Core.Data.WaypointType t = e.NewValue as Core.Data.WaypointType;
            control.SelectedComboItem = t;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Core.Data.WaypointType> _availableTypes;
        public Core.Data.WaypointType SelectedItem
        {
            get { return GetValue(SelectedItemProperty) as Core.Data.WaypointType; }
            set { SetValue(SelectedItemProperty, value); }
        }

        private Core.Data.WaypointType _selectedComboItem;
        public Core.Data.WaypointType SelectedComboItem
        {
            get { return _selectedComboItem; }
            set
            {
                if (value != _selectedComboItem)
                {
                    SetProperty(ref _selectedComboItem, value);
                    SelectedItem = value;
                }
            }
        }

        public WaypointTypeCombo()
        {
            InitializeComponent();

            _availableTypes = new ObservableCollection<Core.Data.WaypointType>();
            if (Core.ApplicationData.Instance.WaypointTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.WaypointTypes)
                {
                    _availableTypes.Add(c);
                }
            }
            SelectedComboItem = SelectedItem;
            DataContext = this;

        }

        public ObservableCollection<Core.Data.WaypointType> AvailableTypes
        {
            get { return _availableTypes; }
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

    }
}
