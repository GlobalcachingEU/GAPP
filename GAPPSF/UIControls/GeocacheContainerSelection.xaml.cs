using System;
using System.Collections.Generic;
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
    public partial class GeocacheContainerSelection : UserControl
    {
        private List<CheckedListItem<Core.Data.GeocacheContainer>> _availableTypes;

        public GeocacheContainerSelection()
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheContainer>>();
            if (Core.ApplicationData.Instance.GeocacheTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.GeocacheContainers)
                {
                    _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheContainer>(c));
                }
            }
            DataContext = this;

        }

        public GeocacheContainerSelection(List<Core.Data.GeocacheContainer> available)
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheContainer>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheContainer>(c));
            }

            DataContext = this;
        }

        public GeocacheContainerSelection(List<Core.Data.GeocacheContainer> available, List<Core.Data.GeocacheContainer> selection)
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheContainer>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheContainer>(c, selection != null && selection.Contains(c)));
            }

            DataContext = this;
        }

        public List<CheckedListItem<Core.Data.GeocacheContainer>> AvailableTypes
        {
            get { return _availableTypes; }
        }

    }
}
