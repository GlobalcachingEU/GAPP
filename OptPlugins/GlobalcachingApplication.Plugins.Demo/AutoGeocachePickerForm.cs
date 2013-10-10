using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.Demo
{
    public partial class AutoGeocachePickerForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public AutoGeocachePickerForm()
        {
            InitializeComponent();
        }

        public const string STR_TITLE = "Random geocache selector";

        private SynchronizationContext _context = null;
        private Thread _thrd = null;
        private AutoResetEvent _checkTrigger = null;

        public AutoGeocachePickerForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Location = Properties.Settings.Default.WindowPos.Location;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
        }

        public void UpdateView()
        {
            if (this.Visible)
            {
                if (_thrd == null)
                {
                    _checkTrigger = new AutoResetEvent(true);
                    _thrd = new Thread(new ThreadStart(this.pickRandomGeocacheMethod));
                    _thrd.IsBackground = true;
                    _thrd.Start();
                }
                timer1.Interval = Properties.Settings.Default.Interval * 1000;
                timer1.Enabled = true;
            }
        }

        private void pickRandomGeocacheMethod()
        {
            Random rnd = new Random();
            Framework.Data.Geocache gc = null;
            while (true)
            {
                if (_checkTrigger.WaitOne())
                {
                    try
                    {
                        string loggerAvatar = null;
                        gc = null;

                        if (Properties.Settings.Default.SelectedGeocachesOnly)
                        {
                            List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                            if (gcList.Count > 2)
                            {
                                //just pick a random one
                                int index = rnd.Next(gcList.Count - 1);
                                gc = gcList[index];
                            }
                        }
                        else if (Core.Geocaches.Count > 2)
                        {
                            //just pick a random one
                            int index = rnd.Next(Core.Geocaches.Count - 1);
                            gc = (Framework.Data.Geocache)Core.Geocaches[index];
                        }

                        if (gc != null)
                        {
                            Framework.Data.Log log = Utils.DataAccess.GetLogs(Core.Logs, gc.Code).FirstOrDefault();

                            if (Properties.Settings.Default.GetLoggerAvatar && log != null && !string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken))
                            {
                                try
                                {
                                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                                    {
                                        var req = new Utils.API.LiveV6.GetAnotherUsersProfileRequest();
                                        req.AccessToken = client.Token;
                                        req.ProfileOptions = new Utils.API.LiveV6.UserProfileOptions();
                                        req.ProfileOptions.PublicProfileData = true;
                                        if (log != null)
                                        {
                                            if (log.FinderId.StartsWith("G"))
                                            {
                                                req.UserID = Utils.Conversion.GetCacheIDFromCacheCode(log.FinderId);
                                            }
                                            else
                                            {
                                                req.UserID = int.Parse(log.FinderId);
                                            }
                                        }
                                        var resp = client.Client.GetAnotherUsersProfile(req);
                                        if (resp != null && resp.Status.StatusCode == 0)
                                        {
                                            loggerAvatar = resp.Profile.User.AvatarUrl;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                            _context.Send(new SendOrPostCallback(delegate(object state)
                            {
                                try
                                {
                                    if (!this.IsDisposed && this.Visible)
                                    {
                                        Core.ActiveGeocache = gc;

                                        label1.Text = DateTime.Now.ToString();

                                        linkLabelGC.Links.Clear();
                                        pictureBoxGC.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, gc.GeocacheType);
                                        linkLabelGC.Text = string.Format("{0}, {1}", gc.Code, gc.Name);
                                        linkLabelGC.Links.Add(0, gc.Code.Length, gc.Url);
                                        if (!string.IsNullOrEmpty(loggerAvatar))
                                        {
                                            pictureBox1.Visible = true;
                                            pictureBox1.ImageLocation = loggerAvatar;
                                        }
                                        else
                                        {
                                            pictureBox1.Visible = false;
                                        }


                                        if (log != null)
                                        {
                                            label1.Text = log.Date.ToLongDateString();
                                            label3.Text = log.Finder ?? "";
                                            label5.Text = log.Text ?? "";
                                        }
                                        else
                                        {
                                            label3.Text = "";
                                            label5.Text = "";
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }), null);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void AutoGeocachePickerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void AutoGeocachePickerForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void AutoGeocachePickerForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _checkTrigger.Set();
        }

    }
}
