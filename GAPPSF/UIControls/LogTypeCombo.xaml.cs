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
    /// Interaction logic for LogTypeCombo.xaml
    /// </summary>
    public partial class LogTypeCombo : UserControl, INotifyPropertyChanged
    {
        private static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(Core.Data.LogType), typeof(LogTypeCombo), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(DependencyObject source,
                DependencyPropertyChangedEventArgs e)
        {
            LogTypeCombo control = source as LogTypeCombo;
            Core.Data.LogType t = e.NewValue as Core.Data.LogType;
            control.SelectedComboItem = t;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Core.Data.LogType> _availableTypes;
        public Core.Data.LogType SelectedItem
        {
            get { return GetValue(SelectedItemProperty) as Core.Data.LogType; }
            set { SetValue(SelectedItemProperty, value); }
        }

        private Core.Data.LogType _selectedComboItem;
        public Core.Data.LogType SelectedComboItem
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

        public LogTypeCombo()
        {
            InitializeComponent();

            _availableTypes = new ObservableCollection<Core.Data.LogType>();
            if (Core.ApplicationData.Instance.LogTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.LogTypes)
                {
                    _availableTypes.Add(c);
                }
            }
            SelectedComboItem = SelectedItem;
            DataContext = this;
        }

        public ObservableCollection<Core.Data.LogType> AvailableTypes
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
