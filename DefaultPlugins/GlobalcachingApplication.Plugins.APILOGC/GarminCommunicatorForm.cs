using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public partial class GarminCommunicatorForm : Form
    {
        public const string STR_TITLE = "Get geocache_visits from Garmin Device";
        public const string STR_START = "Start";

        private TemporaryFile tmpFile = null;
        private bool _running = false;
        public string FileContents { private set; get; }

        public GarminCommunicatorForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            tmpFile = new TemporaryFile(true);
            using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.APILOGC.GarminCommunicator.html")))
            {
                File.WriteAllText(tmpFile.Path, textStreamReader.ReadToEnd());
            }
            webBrowser1.Navigate(string.Format("file://{0}", tmpFile.Path));
        }

        private object executeScript(string script, object[] pars)
        {
            if (pars == null)
            {
                return webBrowser1.Document.InvokeScript(script);
            }
            else
            {
                return webBrowser1.Document.InvokeScript(script, pars);
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Visible)
            {
                try
                {
                    bool ready = (bool)executeScript("isReady", null);
                    if (ready)
                    {
                        if (!_running)
                        {
                            if (!button1.Enabled)
                            {
                                button1.Enabled = true;
                            }
                        }
                        else
                        {
                            string FileContents = (string)executeScript("getGeocacheVisitsFileContent", null);
                            timer1.Enabled = false;
                            DialogResult = System.Windows.Forms.DialogResult.OK;
                            Close();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            _running = true;
            executeScript("getGeocacheVisitsFile", null);
        }

    }
}
