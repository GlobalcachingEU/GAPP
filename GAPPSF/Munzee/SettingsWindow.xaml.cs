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

namespace GAPPSF.Munzee
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public ObservableCollection<string> AvailableTags { get; set; }

        public SettingsWindow()
        {
            AvailableTags = new ObservableCollection<string>();
            var at = from Core.Data.GeocacheType gt in Core.ApplicationData.Instance.GeocacheTypes where gt.ID > 0 && gt.ID < 9000 select gt.GPXTag;
            foreach(var s in at)
            {
                AvailableTags.Add(s);
            }

            InitializeComponent();

            DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utils.DataAccess.GetGeocacheType(95342).GPXTag = Core.Settings.Default.MunzeeGPXTagMunzee;
            Utils.DataAccess.GetGeocacheType(95343).GPXTag = Core.Settings.Default.MunzeeGPXTagVirtual;
            Utils.DataAccess.GetGeocacheType(95344).GPXTag = Core.Settings.Default.MunzeeGPXTagMaintenance;
            Utils.DataAccess.GetGeocacheType(95345).GPXTag = Core.Settings.Default.MunzeeGPXTagBusiness;
            Utils.DataAccess.GetGeocacheType(95346).GPXTag = Core.Settings.Default.MunzeeGPXTagMystery;
            Utils.DataAccess.GetGeocacheType(95347).GPXTag = Core.Settings.Default.MunzeeGPXTagNFC;
            Utils.DataAccess.GetGeocacheType(95348).GPXTag = Core.Settings.Default.MunzeeGPXTagPremium;
        }
    }
}
