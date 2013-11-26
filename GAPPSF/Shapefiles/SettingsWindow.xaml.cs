using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GAPPSF.Shapefiles
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public ObservableCollection<ShapeFileInfo> _listData;

        public SettingsWindow()
        {
            InitializeComponent();

            _listData = new ObservableCollection<ShapeFileInfo>();

            dataGrid.ItemsSource = _listData;
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            int i = dataGrid.SelectedIndex;
            _listData[i].TableName = (sender as ComboBox).SelectedItem as string;
        }

        private void ComboBox_DropDownClosed_1(object sender, EventArgs e)
        {
            int i = dataGrid.SelectedIndex;
            _listData[i].TCoord = (ShapeFile.CoordType)Enum.Parse(typeof(ShapeFile.CoordType), (sender as ComboBox).SelectedItem as string);

        }

        private void ComboBox_DropDownClosed_2(object sender, EventArgs e)
        {
            int i = dataGrid.SelectedIndex;
            _listData[i].TArea = (Core.Data.AreaType)Enum.Parse(typeof(Core.Data.AreaType), (sender as ComboBox).SelectedItem as string);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
