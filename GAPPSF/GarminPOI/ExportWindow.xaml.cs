using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace GAPPSF.GarminPOI
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Core.Data.Geocache> _gcList = null;
        private Core.Storage.Database _db = null;

        public ExportWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public ExportWindow(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
            : this()
        {
            _db = db;
            _gcList = gcList;
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.Filter = "Exe files (*.exe)|*.exe|Batch files (*.cmd)|*.cmd|Batch files (*.bat)|*.bat"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                Core.Settings.Default.GarminPOIPOILoaderFilename = dlg.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new GAPPSF.Dialogs.FolderPickerDialog();
            if (dlg.ShowDialog() == true)
            {
                Core.Settings.Default.GarminPOIExportPath = dlg.SelectedPath;
            }
        }


        private AsyncDelegateCommand _performExportCommand;
        public AsyncDelegateCommand PerformExportCommand
        {
            get
            {
                if (_performExportCommand==null)
                {
                    _performExportCommand = new AsyncDelegateCommand(param => PerformExport(),
                        param => !string.IsNullOrEmpty(Core.Settings.Default.GarminPOIExportPath) && (!Core.Settings.Default.GarminPOIRunPOILoader || !string.IsNullOrEmpty(Core.Settings.Default.GarminPOIPOILoaderFilename)));
                }
                return _performExportCommand;
            }
        }
        public async Task PerformExport()
        {
            Export exp = new Export();
            await exp.ExportToGarminPOIAsync(_db, _gcList);
            Close();
        }
    }
}
