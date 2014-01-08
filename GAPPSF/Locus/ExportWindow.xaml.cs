using GAPPSF.Commands;
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

namespace GAPPSF.Locus
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
            var dlg = new Dialogs.FolderPickerDialog();
            if (dlg.ShowDialog()==true)
            {
                Core.Settings.Default.LocusFolderName = dlg.SelectedPath;
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
                    Export.ExportToFile(System.IO.Path.Combine(Core.Settings.Default.LocusFolderName, "sqlite.db3"), _gcList);
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
            Close();
        }

        private bool canExport()
        {
            bool result = false;
            if (!string.IsNullOrEmpty(Core.Settings.Default.LocusFolderName))
            {
                return true;
            }
            return result;
        }

    }
}
