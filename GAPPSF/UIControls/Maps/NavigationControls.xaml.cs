using System.Windows;
using System.Windows.Controls;
using GAPPSF.MapProviders;

namespace GAPPSF.UIControls.Maps
{
    /// <summary>Displays panning and zoom controls.</summary>
    public sealed partial class NavigationControls : UserControl
    {
        /// <summary>Identifies the Map dependency property.</summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(MapCanvas), typeof(NavigationControls));

        /// <summary>Initializes a new instance of the NavigationControls class.</summary>
        public NavigationControls()
        {
            this.InitializeComponent();
        }

        /// <summary>Gets or sets the map control which will be used as a CommandTarget.</summary>
        public MapCanvas Map
        {
            get { return (MapCanvas)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }
    }
}
