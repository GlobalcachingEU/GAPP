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
    /// Interaction logic for ApplicationDataInfo.xaml
    /// </summary>
    public partial class ApplicationDataInfo : UserControl, IUIControl
    {
        public ApplicationDataInfo()
        {
            InitializeComponent();
        }

        public override string ToString()
        {
            return "Application info";
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowWidth;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowHeight;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowLeft;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowTop;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowTop = value;
            }
        }
    }
}
