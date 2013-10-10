using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Utils.Dialogs
{
    public partial class LiveAPICachesLeftForm : Form
    {
        public const string STR_TITLE = "Live API limits";
        public const string STR_INFO = "The Live API limits the amount of geocaches that can be downloaded per day.";
        public const string STR_MAX = "Limit";
        public const string STR_LEFT = "Left";

        public LiveAPICachesLeftForm()
        {
            InitializeComponent();
        }

        public LiveAPICachesLeftForm(int max, int left)
        {
            InitializeComponent();

            this.labelLeft.Text = left.ToString();
            this.labelLimit.Text = max.ToString();
            this.Text = LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = LanguageSupport.Instance.GetTranslation(STR_INFO);
            this.label2.Text = LanguageSupport.Instance.GetTranslation(STR_MAX);
            this.label3.Text = LanguageSupport.Instance.GetTranslation(STR_LEFT);
        }

        public static void ShowMessage(int max, int left)
        {
            using (LiveAPICachesLeftForm dlg = new LiveAPICachesLeftForm(max, left))
            {
                dlg.ShowDialog();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.geocaching.com/live/");
        }
    }
}
