using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for GeocacheContainerSelection.xaml
    /// </summary>
    public partial class LogTypeSelection : UserControl
    {
        private ObservableCollection<CheckedListItem<Core.Data.LogType>> _availableTypes;

        public LogTypeSelection()
        {
            InitializeComponent();

            _availableTypes = new ObservableCollection<CheckedListItem<Core.Data.LogType>>();
            if (Core.ApplicationData.Instance.GeocacheTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.LogTypes)
                {
                    _availableTypes.Add(new CheckedListItem<Core.Data.LogType>(c));
                }
            }
            DataContext = this;

        }

        public LogTypeSelection(List<Core.Data.LogType> available)
        {
            InitializeComponent();

            _availableTypes = new ObservableCollection<CheckedListItem<Core.Data.LogType>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.LogType>(c));
            }

            DataContext = this;
        }

        public LogTypeSelection(List<Core.Data.LogType> available, List<Core.Data.LogType> selection)
        {
            InitializeComponent();

            _availableTypes = new ObservableCollection<CheckedListItem<Core.Data.LogType>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.LogType>(c, selection != null && selection.Contains(c)));
            }

            DataContext = this;
        }

        public ObservableCollection<CheckedListItem<Core.Data.LogType>> AvailableTypes
        {
            get { return _availableTypes; }
        }

    }
}
