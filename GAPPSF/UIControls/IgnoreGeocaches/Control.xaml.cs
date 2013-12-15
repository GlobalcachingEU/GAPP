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

namespace GAPPSF.UIControls.IgnoreGeocaches
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable
    {
        public Control()
        {
            InitializeComponent();

            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        public override string ToString()
        {
            return "IgnoreGeocaches";
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="IgnoreGeocachesUpdateCounter")
            {
                //todo: update content
            }
        }

        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowWidth;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowHeight;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowLeft;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowTop;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowTop = value;
            }
        }


    }
}
