using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace GAPPSF.LiveAPI
{
    public class GeocachingLiveV6 : IDisposable
    {
        public const string STR_PMREQUIRED = "Premium membership of geocaching.com is required in order to execute this plugin";
        public const string STR_ERROR = "Error";
        public const string STR_MUSTAUTHORIZE = "You must authorize this application in order to execute this plugin";

        private LiveV6.LiveClient _client = null;
        private string _token = "";

        public GeocachingLiveV6()
            : this(false)
        {
        }

        public GeocachingLiveV6(bool testSite)
        {

            BinaryMessageEncodingBindingElement binaryMessageEncoding = new BinaryMessageEncodingBindingElement()
            {
                ReaderQuotas = new XmlDictionaryReaderQuotas()
                {
                    MaxStringContentLength = int.MaxValue,
                    MaxBytesPerRead = int.MaxValue,
                    MaxDepth = int.MaxValue,
                    MaxArrayLength = int.MaxValue
                }
            };

            HttpTransportBindingElement httpTransport = new HttpsTransportBindingElement()
            {
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = false,
            };

            // add the binding elements into a Custom Binding
            CustomBinding binding = new CustomBinding(binaryMessageEncoding, httpTransport);

            EndpointAddress endPoint;
            if (testSite)
            {
                endPoint = new EndpointAddress("https://staging.api.groundspeak.com/Live/V6Beta/geocaching.svc/Silverlightsoap");
                _token = Core.Settings.Default.LiveAPITokenStaging;
            }
            else
            {
                endPoint = new EndpointAddress("https://api.groundspeak.com/LiveV6/Geocaching.svc/Silverlightsoap");
                _token = Core.Settings.Default.LiveAPIToken;
            }

            try
            {
                _client = new LiveV6.LiveClient(binding, endPoint);
            }
            catch
            {
            }

        }

        public LiveV6.LiveClient Client
        {
            get { return _client; }
        }

        public string Token
        {
            get { return _token; }
        }


        public static bool CheckAPIAccessAvailable(bool pmRequired)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIToken) || !string.IsNullOrEmpty(Core.Settings.Default.LiveAPITokenStaging))
            {
                if (!pmRequired || Core.Settings.Default.LiveAPIMemberTypeId > 1)
                {
                    result = true;
                }
                else
                {
                    //System.Windows.Forms.MessageBox.Show(STR_PMREQUIRED, STR_ERROR, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_MUSTAUTHORIZE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }

        public static bool Authorize(bool syncAll)
        {
            return Authorize(syncAll, false);
        }
        public static bool Authorize(bool syncAll, bool testSite)
        {
            bool result = false;
            try
            {
                AuthorizationWindow dlg = new AuthorizationWindow(testSite);
                if (dlg.ShowDialog()==true)
                {
                    if (testSite)
                    {
                        Core.Settings.Default.LiveAPITokenStaging = dlg.Token;
                    }
                    else
                    {
                        Core.Settings.Default.LiveAPIToken = dlg.Token;
                    }
                    using (GeocachingLiveV6 client = new GeocachingLiveV6(testSite))
                    {
                        LiveV6.GetUserProfileResponse resp = client.Client.GetYourUserProfile(new LiveV6.GetYourUserProfileRequest()
                        {
                            AccessToken = client.Token,
                            DeviceInfo = new LiveV6.DeviceData()
                            {
                                DeviceName = "GlobalcachingApplication",
                                DeviceUniqueId = "internal",
                                ApplicationSoftwareVersion = "V1.0.0.0"
                            }
                        });
                        if (resp.Status.StatusCode == 0)
                        {
                            result = true;
                            Core.Data.AccountInfo ai = new Core.Data.AccountInfo("GC", resp.Profile.User.UserName);
                            Core.ApplicationData.Instance.AccountInfos.Add(ai);
                            Core.Settings.Default.LiveAPIMemberType = resp.Profile.User.MemberType.MemberTypeName;
                            Core.Settings.Default.LiveAPIMemberTypeId = (int)resp.Profile.User.MemberType.MemberTypeId;
                            if (syncAll)
                            {
                                if (resp.Profile.User.HomeCoordinates != null)
                                {
                                    Core.Settings.Default.HomeLocationLat = resp.Profile.User.HomeCoordinates.Latitude;
                                    Core.Settings.Default.HomeLocationLon = resp.Profile.User.HomeCoordinates.Longitude;
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public void Dispose()
        {
            if (_client != null)
            {
                try
                {
                    _client.Close();
                }
                catch
                {
                }
                _client = null;
            }
        }

    }
}
