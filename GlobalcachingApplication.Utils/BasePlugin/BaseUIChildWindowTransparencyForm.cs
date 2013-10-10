using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public partial class BaseUIChildWindowTransparencyForm : Form
    {
        public const string STR_TITLE = "Set Opacity";
        public const string STR_OPACITY = "Opacity";

        public BaseUIChildWindowTransparencyForm()
        {
            InitializeComponent();

            trackBar1.Value = Properties.Settings.Default.TopMostOpaque;
            trackBar1.ValueChanged += new EventHandler(trackBar1_ValueChanged);

            this.Text = LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = LanguageSupport.Instance.GetTranslation(STR_OPACITY);
        }

        void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TopMostOpaque = trackBar1.Value;
            Properties.Settings.Default.Save();
            foreach (BaseUIChildWindowForm frm in BaseUIChildWindowForm.AllUIChildForms)
            {
                frm.OpaqueChanged();
            }
        }

        private void BaseUIChildWindowTransparencyForm_Deactivate(object sender, EventArgs e)
        {
            Close();
        }
    }
}
