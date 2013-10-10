using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;

namespace GlobalcachingApplication.Plugins.Browser
{
    public partial class WebbrowserForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Web browser";
        public const string STR_COMPMODECHANGED = "Web browser compatibility mode has been changed. Please restart GAPP to make it effective.";

        public class BrowserTab : TabPage
        {
            public WebbrowserForm Parent { get; private set; }
            public WebBrowser Browser { get; private set; }
            public List<BrowserScript> Scripts { get; set; }
            public FlowLayoutPanel ControlContainer { get; set; }

            public BrowserTab(WebbrowserForm parent)
            {
                Parent = parent;
                Browser = new ExtendedWebBrowser();
                Browser.Dock = DockStyle.Fill;
                Controls.Add(Browser);
            }
        }

        private List<string> _systemScriptsNames = new List<string>();
        private UserScripts _userScripts = null;

        public WebbrowserForm()
        {
            InitializeComponent();
        }

        public WebbrowserForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.DisabledSystemScripts == null)
            {
                Properties.Settings.Default.DisabledSystemScripts = new System.Collections.Specialized.StringCollection();
            }
            if (Properties.Settings.Default.Favorites == null)
            {
                Properties.Settings.Default.Favorites = new System.Collections.Specialized.StringCollection();
            }
            else
            {

                foreach (string s in Properties.Settings.Default.Favorites)
                {
                    comboBox1.Items.Add(s);
                }
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
        }

        public void UpdateView()
        {
            foreach (BrowserTab bt in tabControl1.TabPages)
            {
                foreach (BrowserScript bs in bt.Scripts)
                {
                    bs.Resume();
                }
            }
        }

        private void WebbrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                foreach (BrowserTab bt in tabControl1.TabPages)
                {
                    foreach (BrowserScript bs in bt.Scripts)
                    {
                        bs.Pause();
                    }
                }

                e.Cancel = true;
                Hide();
            }
        }

        private void SetBrowserStatusText(string s)
        {
            toolStripStatusLabel1.Text = s;
        }

        private void setBrowserEmulationMode(string appName)
        {
            try
            {
                bool changed = false;
                int targetValue = Properties.Settings.Default.CompatibilityMode;
                using (RegistryKey subregstrkey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    if (subregstrkey != null)
                    {
                        //the app
                        object o = subregstrkey.GetValue(appName);
                        if (o != null)
                        {
                            int currentValue = (int)o;

                            if (targetValue != currentValue)
                            {
                                changed = true;
                                subregstrkey.SetValue(appName, targetValue, RegistryValueKind.DWord);
                            }
                        }
                        else
                        {
                            changed = true;
                            subregstrkey.SetValue(appName, targetValue, RegistryValueKind.DWord);
                        }
                    }
                }
                if (changed)
                {
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_COMPMODECHANGED));
                }
            }
            catch
            {
            }
        }

        public void OpenNewBrowser(string url)
        {
            newTab(url, null);
        }

        private BrowserTab newTab(string url, NewWindow2EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
            {
                setBrowserEmulationMode(System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location));
#if DEBUG
                setBrowserEmulationMode(System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location).Replace(".exe",".vshost.exe"));
#endif
            }

            BrowserTab result = new BrowserTab(this);
            if (e != null)
            {
                e.PPDisp = (result.Browser as ExtendedWebBrowser).Application;
            }

            //scripting stuff
            result.ControlContainer = new FlowLayoutPanel();
            result.ControlContainer.Dock = DockStyle.Fill;
            result.ControlContainer.AutoScroll = true;
            result.ControlContainer.FlowDirection = FlowDirection.TopDown;
            result.Scripts = new List<BrowserScript>();
            result.Scripts.AddRange(getBrowserScripts(Assembly.GetExecutingAssembly(), result, true));
            if (_userScripts != null)
            {
                Assembly asm = _userScripts.CompiledAssembly;
                if (asm != null)
                {
                    result.Scripts.AddRange(getBrowserScripts(asm, result, false));
                }
            }
            foreach (BrowserScript bs in result.Scripts)
            {
                if (bs.HasControls)
                {
                    Label l = new Label();
                    l.Dock = DockStyle.Top;
                    l.AutoSize = false;
                    l.TextAlign = ContentAlignment.MiddleCenter;
                    l.BackColor = Color.LightCyan;
                    l.Text = Utils.LanguageSupport.Instance.GetTranslation(bs.Name);
                    result.ControlContainer.Controls.Add(l);
                    bs.CreateControls(result.ControlContainer.Controls);
                }
            }
            tabControl1_Selecting(this, null);
            tabControl1.TabPages.Add(result);
            tabControl1.SelectedIndex = tabControl1.TabPages.Count - 1;
            result.Browser.ScriptErrorsSuppressed = Properties.Settings.Default.ScriptErrorsSuppressed;
            result.Browser.StatusTextChanged += new EventHandler(Browser_StatusTextChanged);
            result.Browser.DocumentTitleChanged += new EventHandler(Browser_DocumentTitleChanged);
            result.Browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Browser_DocumentCompleted);
            result.Browser.CanGoBackChanged += new EventHandler(Browser_CanGoBackChanged);
            result.Browser.CanGoForwardChanged += new EventHandler(Browser_CanGoForwardChanged);
            (result.Browser as ExtendedWebBrowser).NewWindow2 +=new EventHandler<NewWindow2EventArgs>(WebbrowserForm_NewWindow2);
            result.Text = result.Browser.DocumentTitle;
            if (url != null)
            {
                comboBox1.Text = url;
                result.Browser.Navigate(url);
            }
            tabControl1_Selected(this, null);

            int max = 10;
            foreach (Control c in result.ControlContainer.Controls)
            {
                if (c.Width > max)
                {
                    max = c.Width;
                }
            }
            splitContainer1.SplitterDistance = max;
            button6.Enabled = true;

            return result;
        }

        void Browser_CanGoForwardChanged(object sender, EventArgs e)
        {
            foreach (BrowserTab bt in tabControl1.TabPages)
            {
                if (bt.Browser == (sender as WebBrowser))
                {
                    button2.Enabled = bt.Browser.CanGoForward;
                    break;
                }
            }
        }

        void Browser_CanGoBackChanged(object sender, EventArgs e)
        {
            foreach (BrowserTab bt in tabControl1.TabPages)
            {
                if (bt.Browser == (sender as WebBrowser))
                {
                    button1.Enabled = bt.Browser.CanGoBack;
                    break;
                }
            }
        }

        void WebbrowserForm_NewWindow2(object sender, NewWindow2EventArgs e)
        {
            newTab(null, e);
        }


        void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            foreach (BrowserTab bt in tabControl1.TabPages)
            {
                if (bt.Browser == (sender as WebBrowser))
                {
                    comboBox1.Text = bt.Browser.Url.OriginalString;
                    break;
                }
            }
        }

        void Browser_DocumentTitleChanged(object sender, EventArgs e)
        {
            foreach (BrowserTab bt in tabControl1.TabPages)
            {
                if (bt.Browser == (sender as WebBrowser))
                {
                    try
                    {
                        bt.Text = bt.Browser.DocumentTitle;
                    }
                    catch
                    {
                    }
                    break;
                }
            }
        }

        void Browser_StatusTextChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                if (bt != null)
                {
                    if (bt.Browser==(sender as WebBrowser))
                    {
                        try
                        {
                            SetBrowserStatusText(bt.Browser.StatusText);
                        }
                        catch
                        {
                        }
                    }
                }
            }            
        }

        private List<BrowserScript> getBrowserScripts(Assembly asm, BrowserTab bt, bool checkDisabledSystemScripts)
        {
            List<BrowserScript> result = new List<BrowserScript>();
            Type[] types = asm.GetTypes();
            bool initSystemScriptNames = (checkDisabledSystemScripts && _systemScriptsNames.Count == 0);
            foreach (Type t in types)
            {
                if (t.IsClass && (t.BaseType == typeof(BrowserScript)))
                {
                    if (checkDisabledSystemScripts && !Properties.Settings.Default.DisabledSystemScripts.Contains(t.Name))
                    {
                        ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(BrowserTab), typeof(Utils.BasePlugin.Plugin), typeof(WebBrowser), typeof(Framework.Interfaces.ICore) });
                        if (constructor != null)
                        {
                            object[] parameters = new object[] { bt, this.OwnerPlugin, bt.Browser, Core };
                            BrowserScript obj = (BrowserScript)constructor.Invoke(parameters);
                            result.Add(obj);
                        }
                    }
                    if (initSystemScriptNames)
                    {
                        _systemScriptsNames.Add(t.Name);
                    }
                }
            }
            return result;
        }

        private void WebbrowserForm_Shown(object sender, EventArgs e)
        {
            loadUserScripts();
            newTab(Properties.Settings.Default.HomePage, null);
        }

        private void loadUserScripts()
        {
            _userScripts = new UserScripts();

            try
            {
                string p = Core.PluginDataPath;
                _userScripts.FileName = System.IO.Path.Combine(new string[] { p, "webscripts.xml" });

                if (System.IO.File.Exists(_userScripts.FileName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_userScripts.FileName);
                    XmlElement root = doc.DocumentElement;

                    _userScripts.UsingNamespaces = root.SelectSingleNode("UsingNamespaces").InnerText;

                    XmlNodeList sNodes = root.SelectSingleNode("Scripts").SelectNodes("Script");
                    if (sNodes != null)
                    {
                        foreach (XmlNode n in sNodes)
                        {
                            UserScripts.Script scr = new UserScripts.Script();

                            scr.Name = n.SelectSingleNode("Name").InnerText;
                            scr.ClassCode = n.SelectSingleNode("ClassCode").InnerText;
                            scr.Enabled = bool.Parse(n.SelectSingleNode("Enabled").InnerText);

                            _userScripts.Scripts.Add(scr);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void saveUserScripts()
        {
            if (_userScripts != null)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    XmlElement root = doc.CreateElement("userscripts");
                    doc.AppendChild(root);

                    XmlElement el = doc.CreateElement("UsingNamespaces");
                    XmlText txt = doc.CreateTextNode(_userScripts.UsingNamespaces ?? "");
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    XmlElement rootscripts = doc.CreateElement("Scripts");
                    root.AppendChild(rootscripts);

                    foreach (UserScripts.Script scr in _userScripts.Scripts)
                    {
                        XmlElement bm = doc.CreateElement("Script");
                        rootscripts.AppendChild(bm);

                        el = doc.CreateElement("Name");
                        txt = doc.CreateTextNode(scr.Name);
                        el.AppendChild(txt);
                        bm.AppendChild(el);

                        el = doc.CreateElement("ClassCode");
                        txt = doc.CreateTextNode(scr.ClassCode);
                        el.AppendChild(txt);
                        bm.AppendChild(el);

                        el = doc.CreateElement("Enabled");
                        txt = doc.CreateTextNode(scr.Enabled.ToString());
                        el.AppendChild(txt);
                        bm.AppendChild(el);
                    }
                    doc.Save(_userScripts.FileName);
                }
                catch
                {
                }
            }
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            splitContainer1.Panel1.Controls.Clear();
            SetBrowserStatusText("");
            comboBox1.Text = "";
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                if (bt != null)
                {
                    if (!splitContainer1.Panel1.Controls.Contains(bt.ControlContainer))
                    {
                        splitContainer1.Panel1.Controls.Add(bt.ControlContainer);
                    }
                    SetBrowserStatusText(bt.Browser.StatusText);
                    comboBox1.Text = bt.Browser.Url == null ? "" : bt.Browser.Url.OriginalString;
                    button1.Enabled = bt.Browser.CanGoBack;
                    button2.Enabled = bt.Browser.CanGoForward;
                }
                else
                {
                    button1.Enabled = false;
                    button2.Enabled = false;
                }
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                if (tabControl1.SelectedIndex >= 0)
                {
                    BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                    if (bt != null)
                    {
                        string s = comboBox1.Text;
                        if (!s.ToLower().StartsWith("http://"))
                        {
                            s = string.Concat("http://", s);
                        }
                        bt.Browser.Navigate(comboBox1.Text);
                    }
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            newTab(Properties.Settings.Default.HomePage, null);
        }

        private void CloseTab(int index)
        {
            if (index >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[index] as BrowserTab;
                if (bt != null)
                {
                    foreach (BrowserScript bs in bt.Scripts)
                    {
                        bs.Stop();
                    }
                    tabControl1.TabPages.RemoveAt(index);
                    bt.Dispose();
                }
            }
            button6.Enabled = tabControl1.TabPages.Count>0;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CloseTab(tabControl1.SelectedIndex);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                if (bt != null)
                {
                    bt.Browser.Navigate(Properties.Settings.Default.HomePage);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                if (bt != null)
                {
                    if (bt.Browser.IsBusy)
                    {
                        bt.Browser.Stop();
                    }
                    comboBox1_KeyPress(this, new KeyPressEventArgs('\r'));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                if (bt != null)
                {
                    if (bt.Browser.CanGoBack)
                    {
                        bt.Browser.GoBack();
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                BrowserTab bt = tabControl1.TabPages[tabControl1.SelectedIndex] as BrowserTab;
                if (bt != null)
                {
                    if (bt.Browser.CanGoForward)
                    {
                        bt.Browser.GoForward();
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Length > 0)
            {
                string s = comboBox1.Text;
                if (Properties.Settings.Default.Favorites.Contains(s))
                {
                    Properties.Settings.Default.Favorites.Remove(s);
                    comboBox1.Items.Remove(s);
                    button5.Image = Properties.Resources.favorite_no;
                }
                else
                {
                    Properties.Settings.Default.Favorites.Add(s);
                    comboBox1.Items.Add(s);
                    button5.Image = Properties.Resources.favorite_yes;
                }
                Properties.Settings.Default.Save();
            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            string s = comboBox1.Text;
            if (Properties.Settings.Default.Favorites.Contains(s))
            {
                if (button5.Image != Properties.Resources.favorite_yes)
                {
                    button5.Image = Properties.Resources.favorite_yes;
                }
            }
            else
            {
                if (button5.Image != Properties.Resources.favorite_no)
                {
                    button5.Image = Properties.Resources.favorite_no;
                }
            }

        }

        private void WebbrowserForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void WebbrowserForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            using (SettingsForm dlg = new SettingsForm(_systemScriptsNames, _userScripts))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    saveUserScripts();

                    //Close all tabs
                    while (tabControl1.TabPages.Count > 0)
                    {
                        CloseTab(0);
                    }
                    _systemScriptsNames.Clear();

                    //open new default tab
                    newTab(Properties.Settings.Default.HomePage, null);
                }
            }
        }
    }
}
