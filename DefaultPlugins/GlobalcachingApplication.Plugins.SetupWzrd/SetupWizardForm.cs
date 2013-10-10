using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace GlobalcachingApplication.Plugins.SetupWzrd
{
    public partial class SetupWizardForm : Form
    {
        public const string STR_YES = "Yes";
        public const string STR_NO = "No";
        public const string STR_BACK = "<< Back";
        public const string STR_NEXT = "Next >>";
        public const string STR_FINISH = "Finish";
        public const string STR_ORGTEXT = "Original text";
        public const string STR_SETUPWIZARD = "Setup wizard";
        public const string STR_LANGUAGE = "Language";
        public const string STR_WELCOMEE = "Welcome";
        public const string STR_GEOCACHINGLIVE = "Geocaching Live";
        public const string STR_HOMELOC = "Home location";
        public const string STR_AUTHORIZE = "Authorize";
        public const string STR_GCNAME = "Account name";
        public const string STR_AUTHORIZED = "Authorized";
        public const string STR_MEMBERSHIP = "Membership";
        public const string STR_HOMECOORDS = "Home coordinates";
        public const string STR_CENTERCOORDS = "Center coordinates";

        private SetupWizard _ownerPlugin = null;
        private Framework.Interfaces.ICore _core = null;

        public SetupWizardForm()
        {
            InitializeComponent();
        }

        public SetupWizardForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
        {
            InitializeComponent();
            this.TopMost = true;

            _ownerPlugin = owner as SetupWizard;
            _core = core;

            UpdateTextForLanguage();

            textBoxAccountname.Text = core.GeocachingComAccount.AccountName;

            textBoxHomecoords.Text = Utils.Conversion.GetCoordinatesPresentation(core.HomeLocation);
            textBoxCentercoords.Text = Utils.Conversion.GetCoordinatesPresentation(core.CenterLocation);

            comboBoxLanguage.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_ORGTEXT));
            comboBoxLanguage.Items.AddRange(Utils.LanguageSupport.Instance.GetSupportedCultures().ToArray());
            for (int i = 1; i < comboBoxLanguage.Items.Count; i++)
            {
                if ((comboBoxLanguage.Items[i] as System.Globalization.CultureInfo).LCID == core.SelectedLanguage.LCID)
                {
                    comboBoxLanguage.SelectedIndex = i;
                    break;
                }
            }
            if (comboBoxLanguage.SelectedIndex < 0)
            {
                comboBoxLanguage.SelectedIndex = 0;
            }
        }

        private void UpdateTextForLanguage()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.SetupWzrd.WelcomeText.txt")))
            {
                labelWelcome.Text = Utils.LanguageSupport.Instance.GetTranslation(textStreamReader.ReadToEnd());
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.SetupWzrd.GeocacheLiveText.txt")))
            {
                labelGeocacheLive.Text = Utils.LanguageSupport.Instance.GetTranslation(textStreamReader.ReadToEnd());
            }
            labelAuthorized.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
            labelMembership.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.MemberType) ? "-" : _core.GeocachingComAccount.MemberType;
            if (tabControl1.SelectedIndex < tabControl1.TabCount - 1)
            {
                buttonNext.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEXT);
            }
            else
            {
                buttonNext.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FINISH);
            }
            buttonBack.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACK);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETUPWIZARD);
            label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LANGUAGE);
            tabPage1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WELCOMEE);
            tabPage2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHINGLIVE);
            tabPage3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HOMELOC);
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCNAME);
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTHORIZED);
            label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MEMBERSHIP);
            buttonAuthorize.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTHORIZE);
            label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HOMECOORDS);
            label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CENTERCOORDS);
        }

        private void buttonAuthorize_Click(object sender, EventArgs e)
        {
            if (Utils.API.GeocachingLiveV6.Authorize(_core, true))
            {
                textBoxAccountname.Text = _core.GeocachingComAccount.AccountName;
                labelAuthorized.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
                labelMembership.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.MemberType) ? "-" : _core.GeocachingComAccount.MemberType;
            }
        }

        private void textBoxAccountname_TextChanged(object sender, EventArgs e)
        {
            _core.GeocachingComAccount.AccountName = textBoxAccountname.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, _core.HomeLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _core.HomeLocation.SetLocation(dlg.Result.Lat, dlg.Result.Lon);
                    textBoxHomecoords.Text = Utils.Conversion.GetCoordinatesPresentation(_core.HomeLocation);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, _core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _core.CenterLocation.SetLocation(dlg.Result.Lat, dlg.Result.Lon);
                    textBoxCentercoords.Text = Utils.Conversion.GetCoordinatesPresentation(_core.CenterLocation);
                    _core.Geocaches.BeginUpdate();
                    foreach (Framework.Data.Geocache gc in _core.Geocaches)
                    {
                        Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, _core.CenterLocation);
                    }
                    _core.Geocaches.EndUpdate();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _core.CenterLocation.SetLocation(_core.HomeLocation.Lat, _core.HomeLocation.Lon);
            textBoxCentercoords.Text = Utils.Conversion.GetCoordinatesPresentation(_core.CenterLocation);
            _core.Geocaches.BeginUpdate();
            foreach (Framework.Data.Geocache gc in _core.Geocaches)
            {
                Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, _core.CenterLocation);
            }
            _core.Geocaches.EndUpdate();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.TopMost = false;

            buttonBack.Enabled = tabControl1.SelectedIndex > 0;
            if (tabControl1.SelectedIndex < tabControl1.TabCount - 1)
            {
                buttonNext.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEXT);
            }
            else
            {
                buttonNext.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FINISH);
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex > 0)
            {
                tabControl1.SelectedIndex--;
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex < tabControl1.TabCount - 1)
            {
                tabControl1.SelectedIndex++;
            }
            else
            {
                Close();
            }
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLanguage.SelectedIndex == 0)
            {
                _core.SelectedLanguage = System.Globalization.CultureInfo.InvariantCulture;
            }
            else
            {
                _core.SelectedLanguage = comboBoxLanguage.SelectedItem as System.Globalization.CultureInfo;
            }
            UpdateTextForLanguage();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://application.globalcaching.eu");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.geocaching.com/live/");
        }
    }

    public class SetupWizard : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Setup wizard";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_NO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_YES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_BACK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_FINISH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_NEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_ORGTEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_SETUPWIZARD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_LANGUAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_HOMELOC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_GEOCACHINGLIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_WELCOMEE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_AUTHORIZE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_AUTHORIZED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_GCNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_MEMBERSHIP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_CENTERCOORDS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SetupWizardForm.STR_HOMECOORDS));
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.SetupWzrd.WelcomeText.txt")))
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(textStreamReader.ReadToEnd()));
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.SetupWzrd.GeocacheLiveText.txt")))
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(textStreamReader.ReadToEnd()));
            }

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.PluginManager;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();
            if (Properties.Settings.Default.FirstTimeUse)
            {
                using (SetupWizardForm dlg = new SetupWizardForm(this, Core))
                {
                    dlg.ShowDialog();
                }
                Properties.Settings.Default.FirstTimeUse = false;
                Properties.Settings.Default.Save(); 
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SHOW)
            {
                using (SetupWizardForm dlg = new SetupWizardForm(this, Core))
                {
                    dlg.ShowDialog();
                }
            }
            return result;
        }
    }

}
