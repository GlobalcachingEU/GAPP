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

namespace GAPPSF.UIControls.GeocacheFilter
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable
    {
        public Control()
        {
            InitializeComponent();
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("GeocacheFilter") as string;
        }

        public void Dispose()
        {
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowWidth;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowHeight;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowLeft;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowTop;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowTop = value;
            }
        }

    }
}
