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
    /// Interaction logic for GeocacheTypeSelection.xaml
    /// </summary>
    public partial class GeocacheTypeSelection : UserControl
    {
        private List<CheckedListItem<Core.Data.GeocacheType>> _availableTypes;

        public GeocacheTypeSelection()
        {
            InitializeComponent();

            _availableTypes = new  List<CheckedListItem<Core.Data.GeocacheType>>();
            if (Core.ApplicationData.Instance.GeocacheTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.GeocacheTypes)
                {
                    _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheType>(c));
                }
            }
            DataContext = this;
        }

        public GeocacheTypeSelection(List<Core.Data.GeocacheType> available)
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheType>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheType>(c));
            }

            DataContext = this;
        }

        public GeocacheTypeSelection(List<Core.Data.GeocacheType> available, List<Core.Data.GeocacheType> selection)
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheType>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheType>(c, selection!=null && selection.Contains(c)));
            }

            DataContext = this;
        }

        public  List<CheckedListItem<Core.Data.GeocacheType>> AvailableTypes
        {
            get { return _availableTypes; }
        }
    }
}
