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
    public partial class BaseUIChildWindowForm : Form
    {
        public const string STR_DECOUPLE_WINDOW = "Decouple from main screen";
        public const string STR_DOCK_WINDOW = "Couple to main screen";
        public const string STR_TOPMOST_WINDOW = "Keep window in front";
        public const string STR_NOTTOPMOST_WINDOW = "Do not keep window in front";
        public const string STR_OPAQUEWHENIACTIVE = "Set transparency when inactive...";

        private const int DECOUPLE_WINDOW_ID = 0x100;
        private const int DOCK_WINDOW_ID = 0x101;
        private const int TOPMOST_WINDOW_ID = 0x102;
        private const int NOTTOPMOST_WINDOW_ID = 0x103;
        private const int OPAQUEWHENIACTIVE_WINDOW_ID = 0x104;

        private Framework.Interfaces.ICore _core = null;
        private Framework.Interfaces.IPlugin _owner = null;
        private bool _updateSystemMenu = false;
        private static List<BaseUIChildWindowForm> _allUIChildForms = new List<BaseUIChildWindowForm>();

        public BaseUIChildWindowForm()
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            _allUIChildForms.Add(this);
        }

        public static List<BaseUIChildWindowForm> AllUIChildForms
        {
            get { return _allUIChildForms; }
        }

        public static int TopMostOpaque
        {
            get { return Properties.Settings.Default.TopMostOpaque; }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            //not critical
            try
            {
                ShowIcon = true;
                reloadSystemMenu();
            }
            catch
            {
            }
        }

        private void Decouple()
        {
            this.MdiParent = null;
            if (Properties.Settings.Default.DecoupledChildWindows == null)
            {
                Properties.Settings.Default.DecoupledChildWindows = new System.Collections.Specialized.StringCollection();
            }
            if (!Properties.Settings.Default.DecoupledChildWindows.Contains(this.GetType().FullName))
            {
                Properties.Settings.Default.DecoupledChildWindows.Add(this.GetType().FullName);
            }
            Properties.Settings.Default.Save();
            CouplingToMainScreenChanged();
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == (int)Utils.SystemMenu.WindowMessages.wmSysCommand)
            {
                switch (msg.WParam.ToInt32())
                {
                    case DECOUPLE_WINDOW_ID:
                        Decouple();
                        reloadSystemMenu();
                        break;
                    case DOCK_WINDOW_ID:
                        Framework.Interfaces.IPluginUIMainWindow mainPlugin = (from Framework.Interfaces.IPlugin a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                        if (mainPlugin != null)
                        {
                            this.MdiParent = mainPlugin.MainForm;
                        }
                        if (Properties.Settings.Default.DecoupledChildWindows == null)
                        {
                            Properties.Settings.Default.DecoupledChildWindows = new System.Collections.Specialized.StringCollection();
                        }
                        if (Properties.Settings.Default.DecoupledChildWindows.Contains(this.GetType().FullName))
                        {
                            Properties.Settings.Default.DecoupledChildWindows.Remove(this.GetType().FullName);
                        }
                        Properties.Settings.Default.Save();
                        reloadSystemMenu();
                        CouplingToMainScreenChanged();
                        break;
                    case TOPMOST_WINDOW_ID:
                        //can only be topmost if decoupled
                        Decouple();
                        this.TopMost = true;
                        if (Properties.Settings.Default.TopMostWindows == null)
                        {
                            Properties.Settings.Default.TopMostWindows = new System.Collections.Specialized.StringCollection();
                        }
                        if (!Properties.Settings.Default.TopMostWindows.Contains(this.GetType().FullName))
                        {
                            Properties.Settings.Default.TopMostWindows.Add(this.GetType().FullName);
                        }
                        Properties.Settings.Default.Save();
                        reloadSystemMenu();
                        break;
                    case NOTTOPMOST_WINDOW_ID:
                        this.TopMost = false;
                        if (Properties.Settings.Default.TopMostWindows == null)
                        {
                            Properties.Settings.Default.TopMostWindows = new System.Collections.Specialized.StringCollection();
                        }
                        if (Properties.Settings.Default.TopMostWindows.Contains(this.GetType().FullName))
                        {
                            Properties.Settings.Default.TopMostWindows.Remove(this.GetType().FullName);
                        }
                        Properties.Settings.Default.Save();
                        reloadSystemMenu();
                        break;
                    case OPAQUEWHENIACTIVE_WINDOW_ID:
                        BaseUIChildWindowTransparencyForm dlg = new BaseUIChildWindowTransparencyForm();
                        dlg.TopMost = true;
                        dlg.Show();
                        break;
                }
            }
            base.WndProc(ref msg);
        }

        protected virtual void CouplingToMainScreenChanged()
        {
        }

        protected Framework.Interfaces.ICore Core
        {
            get { return _core; }
        }

        public void OpaqueChanged()
        {
            if (this.MdiParent == null && this.TopMost && Form.ActiveForm!=this)
            {
                this.Opacity = (double)Properties.Settings.Default.TopMostOpaque / 100.0;
            }
        }

        public BaseUIChildWindowForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;
            _owner = owner;
            LanguageSupport.Instance.UpdateLanguageSupportPlugins(core);
            core.SelectedLanguageChanged += new EventHandler(core_SelectedLanguageChanged);
            Utils.ImageSupport.Instance.UpdateImageSupportPlugins(core);
            Utils.GeometrySupport.Instance.UpdateGeometrySupportPlugins(core);
        }

        public Framework.Interfaces.IPlugin OwnerPlugin
        {
            get { return _owner; }
        }

        void core_SelectedLanguageChanged(object sender, EventArgs e)
        {
            SelectedLanguageChanged(sender, e);
        }

        private void reloadSystemMenu()
        {
            //not critical
            try
            {
                Utils.SystemMenu.ResetSystemMenu(this);
                Utils.SystemMenu sm = Utils.SystemMenu.FromForm(this);
                if (sm != null)
                {
                    if (sm.AppendSeparator())
                    {
                        sm.Append(DECOUPLE_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_DECOUPLE_WINDOW));
                        sm.Append(DOCK_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_DOCK_WINDOW));
                        sm.Append(TOPMOST_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_TOPMOST_WINDOW));
                        sm.Append(NOTTOPMOST_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_NOTTOPMOST_WINDOW));
                        if (Properties.Settings.Default.DecoupledChildWindows != null && Properties.Settings.Default.DecoupledChildWindows.Contains(this.GetType().FullName) &&
                            Properties.Settings.Default.TopMostWindows != null && Properties.Settings.Default.TopMostWindows.Contains(this.GetType().FullName))
                        {
                            sm.Append(OPAQUEWHENIACTIVE_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_OPAQUEWHENIACTIVE));
                        }
                    }
                    else
                    {
                        ShowIcon = false;
                    }
                }
                else
                {
                    ShowIcon = false;
                }
            }
            catch
            {
            }
        }

        protected virtual void SelectedLanguageChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                reloadSystemMenu();
            }
            else
            {
                _updateSystemMenu = true;
            }
        }

        void plugin_Closing(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            Core.SelectedLanguageChanged -= new EventHandler(core_SelectedLanguageChanged);
        }

        private void BaseUIChildWindowForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_core != null)
            {
                _core.SelectedLanguageChanged -= new EventHandler(core_SelectedLanguageChanged);
            }
        }

        private void BaseUIChildWindowForm_LocationChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void BaseUIChildWindowForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && _updateSystemMenu)
            {
                reloadSystemMenu();
            }
        }

        private void BaseUIChildWindowForm_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1.0;
        }

        private void BaseUIChildWindowForm_Deactivate(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.DecoupledChildWindows != null && Properties.Settings.Default.DecoupledChildWindows.Contains(this.GetType().FullName) &&
                Properties.Settings.Default.TopMostWindows != null && Properties.Settings.Default.TopMostWindows.Contains(this.GetType().FullName))
            {
                this.Opacity = (double)Properties.Settings.Default.TopMostOpaque / 100.0;
            }
        }

    }
}
