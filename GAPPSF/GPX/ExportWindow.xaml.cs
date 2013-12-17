using GAPPSF.Commands;
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

namespace GAPPSF.GPX
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private List<Core.Data.Geocache> _gcList;
        private Devices.GarminMassStorage _garminStorage = null;
        public ObservableCollection<Devices.GarminMassStorage.DeviceInfo> GarminDevices { get; set; }

        public ExportWindow()
        {
            InitializeComponent();
        }

        public ExportWindow(List<Core.Data.Geocache> gcList):this()
        {
            _gcList = gcList;
            GarminDevices = new ObservableCollection<Devices.GarminMassStorage.DeviceInfo>();
            _garminStorage = new Devices.GarminMassStorage();
            _garminStorage.DeviceAddedEvent += _garminStorage_DeviceAddedEvent;
            _garminStorage.DeviceRemovedEvent += _garminStorage_DeviceRemovedEvent;

            DataContext = this;
        }

        void _garminStorage_DeviceRemovedEvent(object sender, Devices.GarminMassStorage.DeviceInfoEventArgs e)
        {
            GarminDevices.Remove(e.Device);
        }

        void _garminStorage_DeviceAddedEvent(object sender, Devices.GarminMassStorage.DeviceInfoEventArgs e)
        {
            GarminDevices.Add(e.Device);
        }

        private AsyncDelegateCommand _exportCommand;
        public AsyncDelegateCommand ExportCommand
        {
            get
            {
                if (_exportCommand==null)
                {
                    _exportCommand = new AsyncDelegateCommand(param=>PerformExportAsync());
                }
                return _exportCommand;
            }
        }
        private async Task PerformExportAsync()
        {
            await Task.Run(() =>
                {
                    //todo
                });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_garminStorage!=null)
            {
                _garminStorage.Dispose();
                _garminStorage = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //select folder
            var dlg = new GAPPSF.Dialogs.FolderPickerDialog();
            if (dlg.ShowDialog()==true)
            {

            }
        }

    }
}
