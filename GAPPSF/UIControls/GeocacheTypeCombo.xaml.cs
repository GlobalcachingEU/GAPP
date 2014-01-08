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
    /// Interaction logic for GeocacheTypeCombo.xaml
    /// </summary>
    public partial class GeocacheTypeCombo : UserControl, INotifyPropertyChanged
    {
        private static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(Core.Data.GeocacheType), typeof(GeocacheTypeCombo), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(DependencyObject source,
                DependencyPropertyChangedEventArgs e)
        {
            GeocacheTypeCombo control = source as GeocacheTypeCombo;
            Core.Data.GeocacheType t = e.NewValue as Core.Data.GeocacheType;
            control.SelectedComboItem = t;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Core.Data.GeocacheType> _availableTypes;
        public Core.Data.GeocacheType SelectedItem
        {
            get { return GetValue(SelectedItemProperty) as Core.Data.GeocacheType; }
            set { SetValue(SelectedItemProperty, value); }
        }

        private Core.Data.GeocacheType _selectedComboItem;
        public Core.Data.GeocacheType SelectedComboItem
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

        public GeocacheTypeCombo()
        {
            InitializeComponent();

            _availableTypes = new ObservableCollection<Core.Data.GeocacheType>();
            if (Core.ApplicationData.Instance.GeocacheTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.GeocacheTypes)
                {
                    _availableTypes.Add(c);
                }
            }
            SelectedComboItem = SelectedItem;
            DataContext = this;
        }

        public ObservableCollection<Core.Data.GeocacheType> AvailableTypes
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
