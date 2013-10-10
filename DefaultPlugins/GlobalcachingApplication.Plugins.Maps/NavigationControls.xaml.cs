using System.Windows;
using System.Windows.Controls;

namespace GlobalcachingApplication.Plugins.Maps
{
    /// <summary>Displays panning and zoom controls.</summary>
    public sealed partial class NavigationControls : UserControl
    {
        /// <summary>Identifies the Map dependency property.</summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(MapControl.MapCanvas), typeof(NavigationControls));

        /// <summary>Initializes a new instance of the NavigationControls class.</summary>
        public NavigationControls()
        {
            this.InitializeComponent();
        }

        /// <summary>Gets or sets the map control which will be used as a CommandTarget.</summary>
        public MapControl.MapCanvas Map
        {
            get { return (MapControl.MapCanvas)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }
    }
}
