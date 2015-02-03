using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.HtmlEditor
{
    public partial class HtmlEditorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "HTML Editor";
        public const string STR_EDITOR = "Editor";
        public const string STR_HTML = "HTML";
        public const string STR_COPYTOCLIPBOARD = "Copy to clipboard";
        public const string STR_CKEDITORONLINE = "CKEditor (Online)";

        private bool _ckeditorLoaded = false;
        private Globalcaching.HtmlEditor.HtmlEditorControl htmlEditorControl1;
        private WebBrowser webBrowser1;

        public HtmlEditorForm()
        {
            InitializeComponent();
        }

        public HtmlEditorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            var p = PluginSettings.Instance.WindowPos;
            if (p != null && !p.IsEmpty)
            {
                this.Bounds = p;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        public void UpdateView()
        {
            htmlEditorControl1 = new Globalcaching.HtmlEditor.HtmlEditorControl();
            htmlEditorControl1.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(htmlEditorControl1);
            tabControl1.SelectedTab = tabPage1;
            tabControl1_SelectedIndexChanged(this, EventArgs.Empty);
            webBrowser1 = new WebBrowser();
            webBrowser1.Dock = DockStyle.Fill;
            tabPage3.Controls.Add(webBrowser1);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.tabPage1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EDITOR);
            this.tabPage2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HTML);
            this.tabPage3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CKEDITORONLINE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPYTOCLIPBOARD);
        }

        private void HtmlEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ckeditorLoaded = false;
            tabPage1.Controls.Remove(htmlEditorControl1);
            htmlEditorControl1.Dispose();
            tabPage3.Controls.Remove(webBrowser1);
            webBrowser1.Dispose();
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                htmlEditorControl1.Text = textBox1.Text;
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                textBox1.Text = htmlEditorControl1.Text;
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                if (!_ckeditorLoaded)
                {
                    _ckeditorLoaded = true;
                    webBrowser1.Navigate("http://application.globalcaching.eu/ckeditor/ckeditor.html");
                }
            }
        }

        private void HtmlEditorForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void HtmlEditorForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }

    }
}
