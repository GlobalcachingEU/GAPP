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

            if (!string.IsNullOrEmpty(Core.Settings.Default.ShapeFiles))
            {
                string[] lines = Core.Settings.Default.ShapeFiles.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string shpFile in lines)
                {
                    //enabled
                    //file name
                    //table name for name of area
                    //Coord type
                    //Area type
                    //name prefix
                    string[] parts = shpFile.Split(new char[] { '|' }, 6);
                    if (parts.Length == 6)
                    {
                        try
                        {
                            ShapeFileInfo sf = new ShapeFileInfo();
                            sf.Enabled = bool.Parse(parts[0]);
                            sf.Filename = parts[1];
                            sf.TableName = parts[2];
                            sf.TCoord = parts[3];
                            sf.TArea = parts[4];
                            sf.Prefix = parts[5];

                            using (ShapeFile s = new ShapeFile(sf.Filename))
                            {
                                sf.TableNames = s.GetFields().ToList();
                            }
                            sf.TAreas = Enum.GetNames(typeof(Core.Data.AreaType)).ToList();
                            sf.TCoords = Enum.GetNames(typeof(ShapeFile.CoordType)).ToList();

                            _listData.Add(sf);
                        }
                        catch
                        {
                        }
                    }
                }
            }

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
            _listData[i].TCoord = (sender as ComboBox).SelectedItem as string;

        }

        private void ComboBox_DropDownClosed_2(object sender, EventArgs e)
        {
            int i = dataGrid.SelectedIndex;
            _listData[i].TArea = (sender as ComboBox).SelectedItem as string;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".shp"; // Default file extension
            dlg.Filter = "Shapefile (.shp)|*.shp"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                ShapeFileInfo sf = new ShapeFileInfo();
                sf.Enabled = true;
                sf.Filename = dlg.FileName;
                sf.TableName = "";
                sf.TCoord = ShapeFile.CoordType.WGS84.ToString();
                sf.TArea = Core.Data.AreaType.Other.ToString();
                sf.Prefix = "";

                using (ShapeFile s = new ShapeFile(sf.Filename))
                {
                    sf.TableNames = s.GetFields().ToList();
                }
                sf.TAreas = Enum.GetNames(typeof(Core.Data.AreaType)).ToList();
                sf.TCoords = Enum.GetNames(typeof(ShapeFile.CoordType)).ToList();

                _listData.Add(sf);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ShapeFileInfo sf in _listData)
            {
                if (sf.TableName.Length>0)
                {
                    sb.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}",sf.Enabled.ToString(), sf.Filename, sf.TableName, sf.TCoord,sf.TArea, sf.Prefix??""));
                }
            }
            Core.Settings.Default.ShapeFiles = sb.ToString();
            ShapeFilesManager.Instance.Initialize();
            DialogResult = true;
            Close();
        }
    }
}
