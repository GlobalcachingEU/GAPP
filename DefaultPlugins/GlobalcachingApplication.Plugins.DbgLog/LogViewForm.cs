using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.DbgLog
{
    public partial class LogViewForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Log viewer";
        public const string STR_LEVEL = "Level";
        public const string STR_CLEAR = "Clear";

        private bool _running = false;
        private volatile Framework.Data.DebugLogLevel _dbgLevel = Framework.Data.DebugLogLevel.Info;

        public LogViewForm()
        {
            InitializeComponent();
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.labelLevel.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LEVEL);
            this.buttonClear.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAR);
        }

        public LogViewForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();
            comboBox1.Items.AddRange(Enum.GetNames(typeof(Framework.Data.DebugLogLevel)));
            comboBox1.SelectedItem = Enum.GetName(typeof(Framework.Data.DebugLogLevel), Framework.Data.DebugLogLevel.Info);

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        public void Start()
        {
            if (!_running)
            {
                Core.DebugLogAdded += new Framework.EventArguments.DebugLogEventHandler(Core_DebugLogAdded);
                _running = true;
            }
        }

        void Core_DebugLogAdded(object sender, Framework.EventArguments.DebugLogEventArgs e)
        {
            if (e.Level <= _dbgLevel && _dbgLevel != Framework.Data.DebugLogLevel.None)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Framework.EventArguments.DebugLogEventHandler(this.Core_DebugLogAdded), new object[] { sender, e });
                    return;
                }
                //todo
                textBox1.Text = string.Concat(textBox1.Text, e.Message, "\r\n");
            }
        }

        public void Stop()
        {
            if (_running)
            {
                Core.DebugLogAdded -= new Framework.EventArguments.DebugLogEventHandler(Core_DebugLogAdded);
                _running = false;
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void LogViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dbgLevel = (Framework.Data.DebugLogLevel)Enum.Parse(typeof(Framework.Data.DebugLogLevel), comboBox1.SelectedItem.ToString());
        }
    }

    public class LogView : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Debug log viewer";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewForm.STR_CLEAR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewForm.STR_LEVEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewForm.STR_TITLE));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Debug;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new LogViewForm(this, core));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            UIChildWindowForm.Show();
                            (UIChildWindowForm as LogViewForm).Start();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }
    }

}
