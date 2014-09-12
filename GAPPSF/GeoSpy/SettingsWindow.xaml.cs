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

namespace GAPPSF.GeoSpy
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
            Utils.DataAccess.GetGeocacheType(97001).GPXTag = Core.Settings.Default.GeoSpyGPXTagCivil;
            Utils.DataAccess.GetGeocacheType(97002).GPXTag = Core.Settings.Default.GeoSpyGPXTagHistoricAndReligious;
            Utils.DataAccess.GetGeocacheType(97003).GPXTag = Core.Settings.Default.GeoSpyGPXTagNatural;
            Utils.DataAccess.GetGeocacheType(97004).GPXTag = Core.Settings.Default.GeoSpyGPXTagTechnical;
            Utils.DataAccess.GetGeocacheType(97005).GPXTag = Core.Settings.Default.GeoSpyGPXTagMilitary;
        }
    }
}
