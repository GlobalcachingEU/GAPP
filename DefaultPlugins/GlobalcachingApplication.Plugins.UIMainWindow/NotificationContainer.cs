using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    public partial class NotificationContainer : UserControl
    {
        public const string STR_TITLE = "Notifications";

        private FormMain _mainForm = null;
        private bool _minimized = false;

        public NotificationContainer()
        {
            InitializeComponent();
        }

        public NotificationContainer(FormMain mainForm)
            : this()
        {
            _mainForm = mainForm;
            _mainForm.Resize += new EventHandler(_mainForm_Resize);
        }

        private void Reposition()
        {
            //keep bottomright to lower right of client area
            Point br = _mainForm.BottomRightOfClientArea;
            if (_minimized)
            {
                this.Location = new Point(br.X - this.Width, br.Y - this.panel1.Bottom);
            }
            else
            {
                this.Location = new Point(br.X - this.Width, br.Y - this.Height);
            }
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
        }

        void _mainForm_Resize(object sender, EventArgs e)
        {
            Reposition();
        }

        public void AddNotificationMessage(UserControl uc)
        {
            flowLayoutPanel1.Controls.Add(uc);
            Reposition();
            if (flowLayoutPanel1.Controls.Count == 1)
            {
                this.Visible = true;
            }
            _minimized = false;
            uc.VisibleChanged += new EventHandler(uc_VisibleChanged);
        }

        void uc_VisibleChanged(object sender, EventArgs e)
        {
            //removed itself
            (sender as UserControl).VisibleChanged -= new EventHandler(uc_VisibleChanged);
            flowLayoutPanel1.Controls.Remove(sender as UserControl);
            PerformLayout();
            Reposition();
            if (flowLayoutPanel1.Controls.Count == 0)
            {
                this.Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _minimized = !_minimized;
            Reposition();
        }
    }
}
