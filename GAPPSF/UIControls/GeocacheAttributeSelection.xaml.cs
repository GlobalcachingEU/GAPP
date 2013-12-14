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
    /// Interaction logic for GeocacheAttributeSelection.xaml
    /// </summary>
    public partial class GeocacheAttributeSelection : UserControl
    {
        private List<CheckedListItem<Core.Data.GeocacheAttribute>> _availableTypes;

        public GeocacheAttributeSelection()
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheAttribute>>();
            if (Core.ApplicationData.Instance.GeocacheTypes != null)
            {
                foreach (var c in Core.ApplicationData.Instance.GeocacheAttributes)
                {
                    _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheAttribute>(c));
                }
            }
            DataContext = this;
        }

        public GeocacheAttributeSelection(List<Core.Data.GeocacheAttribute> available)
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheAttribute>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheAttribute>(c));
            }

            DataContext = this;
        }

        public GeocacheAttributeSelection(List<Core.Data.GeocacheAttribute> available, List<Core.Data.GeocacheAttribute> selection)
        {
            InitializeComponent();

            _availableTypes = new List<CheckedListItem<Core.Data.GeocacheAttribute>>();
            foreach (var c in available)
            {
                _availableTypes.Add(new CheckedListItem<Core.Data.GeocacheAttribute>(c, selection != null && selection.Contains(c)));
            }

            DataContext = this;
        }

        public List<CheckedListItem<Core.Data.GeocacheAttribute>> AvailableTypes
        {
            get { return _availableTypes; }
        }

    }
}
