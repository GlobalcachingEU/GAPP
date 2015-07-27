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

namespace GAPPSF.LiveAPI
{
    /// <summary>
    /// Interaction logic for ManualAuthorizationWindow.xaml
    /// </summary>
    public partial class ManualAuthorizationWindow : Window
    {
        private string _token;
        public string Token
        {
            get { return _token; }
            set
            {
                _token = value;
                bconfirm.IsEnabled = !string.IsNullOrEmpty(_token);
            }
        }


        public ManualAuthorizationWindow()
        {
            InitializeComponent();
            bconfirm.IsEnabled = false;

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://application.globalcaching.eu/TokenRequest.aspx");
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeocachingLiveV6 client = new GeocachingLiveV6())
                {
                    var resp = client.Client.GetYourUserProfile(new LiveV6.GetYourUserProfileRequest()
                    {
                        AccessToken = Token,
                        DeviceInfo = new LiveV6.DeviceData()
                        {
                            DeviceName = "GlobalcachingApplication",
                            DeviceUniqueId = "internal",
                            ApplicationSoftwareVersion = "V1.0.0.0"
                        }
                    });
                    if (resp.Status.StatusCode == 0)
                    {
                        Core.Settings.Default.LiveAPIToken = Token;
                        Core.Data.AccountInfo ai = new Core.Data.AccountInfo("GC", resp.Profile.User.UserName);
                        Core.ApplicationData.Instance.AccountInfos.Add(ai);
                        Core.Settings.Default.LiveAPIMemberType = resp.Profile.User.MemberType.MemberTypeName;
                        Core.Settings.Default.LiveAPIMemberTypeId = (int)resp.Profile.User.MemberType.MemberTypeId;
                        if (resp.Profile.User.HomeCoordinates != null)
                        {
                            Core.Settings.Default.HomeLocationLat = resp.Profile.User.HomeCoordinates.Latitude;
                            Core.Settings.Default.HomeLocationLon = resp.Profile.User.HomeCoordinates.Longitude;
                        }
                        Close();
                    }
                    else
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }
    }
}
