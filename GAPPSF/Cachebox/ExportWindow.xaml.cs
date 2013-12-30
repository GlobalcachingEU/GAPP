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

namespace GAPPSF.Cachebox
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

            DataContext = this;
        }
        public ExportWindow(List<Core.Data.Geocache> gcList)
            : this()
        {
            _gcList = gcList;
        }

        private AsyncDelegateCommand _exportCommand;
        public AsyncDelegateCommand ExportCommand
        {
            get
            {
                if (_exportCommand==null)
                {
                    _exportCommand = new AsyncDelegateCommand(param => PerformExport(),
                        param => !string.IsNullOrEmpty(Core.Settings.Default.CacheboxTargetFolder));
                }
                return _exportCommand;
            }
        }

        public async Task PerformExport()
        {
            Export ex = new Export();
            await ex.ExportToCachebox(_gcList,
                Core.Settings.Default.CacheboxTargetFolder,
                Core.Settings.Default.CacheboxMaxLogCount);
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new GAPPSF.Dialogs.FolderPickerDialog();
            if (dlg.ShowDialog() == true)
            {
                Core.Settings.Default.CacheboxTargetFolder = dlg.SelectedPath;
            }
        }
    }
}
