using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for GetLocationWindow.xaml
    /// </summary>
    public partial class GetLocationWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Core.Data.Location Location { get; set; }

        private ObjectForScriptingCallback _scriptObj;
        private bool _browserPageLoaded = false;

        public GetLocationWindow():this(Core.ApplicationData.Instance.CenterLocation)
        {
            
        }

        public GetLocationWindow(Core.Data.Location _location)
        {
            _scriptObj = new ObjectForScriptingCallback(this);

            Location = _location;
            if (Location==null)
            {
                CoordText = "";
            }
            else
            {
                CoordText = Location.ToString();
            }
            DataContext = this;

            InitializeComponent();
            webBrowser.ObjectForScripting = _scriptObj;
        }

        private bool _coordIsValid = true;
        public bool CoordIsValid
        {
            get { return _coordIsValid; }
            set { SetProperty(ref _coordIsValid, value); }
        }

        private string _coordText = null;
        public string CoordText
        {
            get { return _coordText; }
            set
            {
                if (_coordText!=value)
                {
                    Location = Utils.Conversion.StringToLocation(value);
                    CoordIsValid = Location != null;
                    SetProperty(ref _coordText, value);
                }
            }
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
            DialogResult = Location!=null;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CoordText = Core.ApplicationData.Instance.HomeLocation.ToString();
            if (_browserPageLoaded)
            {
                try
                {
                    webBrowser.InvokeScript("setCenter", new object[] { Core.ApplicationData.Instance.HomeLocation.Lat, Core.ApplicationData.Instance.HomeLocation.Lon });
                }
                catch
                {

                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CoordText = Core.ApplicationData.Instance.CenterLocation.ToString();
            if (_browserPageLoaded)
            {
                try
                {
                    webBrowser.InvokeScript("setCenter", new object[] { Core.ApplicationData.Instance.CenterLocation.Lat, Core.ApplicationData.Instance.CenterLocation.Lon });
                }
                catch
                {

                }
            }
        }

        public void updateLocation(double lat, double lon)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Core.Data.Location l = new Core.Data.Location(lat, lon);
                CoordText = l.ToString();
            })); 
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string html = Utils.ResourceHelper.GetEmbeddedTextFile("/Dialogs/GetLocationWindow.html");
            Core.Data.Location ll;
            if (Location==null)
            {
                ll = Core.ApplicationData.Instance.CenterLocation;
            }
            else
            {
                ll = Location;
            }
            html = html.Replace("google.maps.LatLng(0.0, 0.0)", string.Format("google.maps.LatLng({0})", ll.SLatLon));
            webBrowser.NavigateToString(html);
            _browserPageLoaded = true;
        }
    }

    [ComVisible(true)]
    public class ObjectForScriptingCallback
    {
        private GetLocationWindow _owner = null;

        public ObjectForScriptingCallback(GetLocationWindow owner)
        {
            _owner = owner;
        }
        public void updateLocation(double lat, double lon)
        {
            _owner.updateLocation(lat, lon);
        }
    }
}
