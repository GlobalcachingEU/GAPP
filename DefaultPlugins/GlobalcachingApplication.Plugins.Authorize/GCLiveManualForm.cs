using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Authorize
{
    public partial class GCLiveManualForm : Form
    {
        public const string STR_TITLE = "Manual Live API Authorization";
        public const string STR_ERROR = "Error";
        public const string STR_AUTHORIZE = "Authorize";
        public const string STR_CONFIRM = "Confirm";
        public const string STR_STEP1 = "Step 1: Perform authorization with your standard browser";
        public const string STR_STEP2 = "Step 2: Copy obtained code from step 1";
        public const string STR_STEP3 = "Step 3: Confirm";

        private Framework.Interfaces.ICore _core = null;

        public GCLiveManualForm()
        {
            InitializeComponent();
        }

        public GCLiveManualForm(Framework.Interfaces.ICore core)
            :this()
        {
            _core = core;
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTHORIZE);
            button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONFIRM);
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STEP1);
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STEP2);
            label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STEP3);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox1.Text.Trim().Length > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://application.globalcaching.eu/TokenRequest.aspx");
            }
            catch
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var client = new Utils.API.GeocachingLiveV6(_core))
                {
                    string token = textBox1.Text.Trim();

                    var resp = client.Client.GetYourUserProfile(new Utils.API.LiveV6.GetYourUserProfileRequest()
                    {
                        AccessToken = token,
                        DeviceInfo = new Utils.API.LiveV6.DeviceData()
                        {
                            DeviceName = "GlobalcachingApplication",
                            DeviceUniqueId = "internal",
                            ApplicationSoftwareVersion = "V1.0.0.0"
                        }
                    });
                    if (resp.Status.StatusCode == 0)
                    {
                        _core.GeocachingComAccount.APIToken = token;
                        _core.GeocachingComAccount.AccountName = resp.Profile.User.UserName;
                        _core.GeocachingComAccount.MemberType = resp.Profile.User.MemberType.MemberTypeName;
                        _core.GeocachingComAccount.MemberTypeId = (int)resp.Profile.User.MemberType.MemberTypeId;
                        if (resp.Profile.User.HomeCoordinates != null)
                        {
                            _core.HomeLocation.SetLocation(resp.Profile.User.HomeCoordinates.Latitude, resp.Profile.User.HomeCoordinates.Longitude);
                        }
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
            }
        }
    }
}
