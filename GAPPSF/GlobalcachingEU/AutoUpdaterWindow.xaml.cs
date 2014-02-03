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

namespace GAPPSF.GlobalcachingEU
{
    /// <summary>
    /// Interaction logic for AutoUpdaterWindow.xaml
    /// </summary>
    public partial class AutoUpdaterWindow : Window
    {
        public AutoUpdaterWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                Import imp = new Import();
                await imp.UpdateGeocachesAsync(Core.ApplicationData.Instance.ActiveDatabase);
                Close();
            }
        }
    }
}
