using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace GAPPSF.OV2
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private List<Core.Data.Geocache> _gcList;

        public ExportWindow()
        {
            InitializeComponent();
        }

        public ExportWindow(List<Core.Data.Geocache> gcList)
            : this()
        {
            _gcList = gcList;

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".ov2"; // Default file extension
            dlg.Filter = "TomTom (.ov2)|*.ov2"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                Core.Settings.Default.OV2FileName = dlg.FileName;
            }
        }

        private AsyncDelegateCommand _exportCommand;
        public AsyncDelegateCommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new AsyncDelegateCommand(param => PerformExportAsync(), param => canExport());
                }
                return _exportCommand;
            }
        }
        private async Task PerformExportAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    Export.ExportToFile(_gcList,Core.Settings.Default.OV2FileName);
                }
                catch
                {

                }
            });
        }

        private bool canExport()
        {
            bool result = false;
            if (!string.IsNullOrEmpty(Core.Settings.Default.OV2FileName))
            {
                return true;
            }
            return result;
        }
    }
}
