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
using System.Windows.Shapes;

namespace GAPPSF
{
    /// <summary>
    /// Interaction logic for FeatureWindow.xaml
    /// </summary>
    public partial class FeatureWindow : Window
    {
        public FeatureWindow()
        {
            InitializeComponent();
        }

        public FeatureWindow(UserControl feature)
            : this()
        {
            featureContainer.FeatureControl = feature;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            featureContainer.FeatureControl = null;
        }
    }
}
