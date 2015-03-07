using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
//using System.Security

namespace GlobalcachingApplication.Utils.API
{
    public class GeocachingLiveV6: IDisposable
    {
        public const string STR_PMREQUIRED = "Premium membership of geocaching.com is required in order to execute this plugin";
        public const string STR_ERROR = "Error";
        public const string STR_MUSTAUTHORIZE = "You must authorize this application in order to execute this plugin";

        private LiveV6.LiveClient _client = null;
        private string _token = "";
        private Framework.Interfaces.ICore _core = null;

        private GeocachingLiveV6() { }

        public GeocachingLiveV6(Framework.Interfaces.ICore core)
            : this(core, false)
        {
        }

        public GeocachingLiveV6(Framework.Interfaces.ICore core, bool testSite)
        {
            _core = core;

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
                _token = core.GeocachingComAccount.APITokenStaging;
            }
            else
            {
                endPoint = new EndpointAddress("https://api.groundspeak.com/LiveV6/Geocaching.svc/Silverlightsoap");
                _token = core.GeocachingComAccount.APIToken;
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


        public static bool CheckAPIAccessAvailable(Framework.Interfaces.ICore core, bool pmRequired)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(core.GeocachingComAccount.APIToken) || !string.IsNullOrEmpty(core.GeocachingComAccount.APITokenStaging))
            {
                if (!pmRequired || core.GeocachingComAccount.MemberTypeId > 1)
                {
                    result = true;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_PMREQUIRED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_MUSTAUTHORIZE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }

        public static bool Authorize(Framework.Interfaces.ICore core, bool syncAll)
        {
            bool result = false;
            try
            {
                if (core.RetrieveAPIKey(Framework.Data.APIKey.GeocachingLive))
                {
                    using (GeocachingLiveV6 client = new GeocachingLiveV6(core))
                    {
                        var resp = client.Client.GetYourUserProfile(new LiveV6.GetYourUserProfileRequest()
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
                            core.GeocachingComAccount.AccountName = resp.Profile.User.UserName;
                            core.GeocachingComAccount.MemberType = resp.Profile.User.MemberType.MemberTypeName;
                            core.GeocachingComAccount.MemberTypeId = (int)resp.Profile.User.MemberType.MemberTypeId;
                            if (syncAll)
                            {
                                if (resp.Profile.User.HomeCoordinates != null)
                                {
                                    core.HomeLocation.SetLocation(resp.Profile.User.HomeCoordinates.Latitude, resp.Profile.User.HomeCoordinates.Longitude);
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
