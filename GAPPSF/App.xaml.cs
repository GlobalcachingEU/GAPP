using GAPPSF.Localization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace GAPPSF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            WpfSingleInstance.Make("GAPPSF", this);

            if (GAPPSF.Properties.Settings.Default.UpgradeNeeded)
            {
                GAPPSF.Properties.Settings.Default.Upgrade();
                GAPPSF.Properties.Settings.Default.UpgradeNeeded = false;
                GAPPSF.Properties.Settings.Default.Save();
            }
            bool settingsFolderOK = true;
            string[] args = Environment.GetCommandLineArgs();
            if (GAPPSF.Properties.Settings.Default.EnableSettingsFolderAtStartup || (args != null && args.Contains("/f")))
            {
                Dialogs.SettingsFolderWindow dlg = new Dialogs.SettingsFolderWindow();
                if (dlg.ShowDialog()==true)
                {
                    GAPPSF.Properties.Settings.Default.SettingsFolder = dlg.SelectedSettingsPath;
                    GAPPSF.Properties.Settings.Default.Save();
                }
                else
                {
                    settingsFolderOK = false;
                }
            }
            if (settingsFolderOK)
            {
                Core.Settings.Default.SettingsFolder = GAPPSF.Properties.Settings.Default.SettingsFolder;
                string sc = Core.Settings.Default.SelectedCulture;
                if (sc == null)
                {
                    Core.Settings.Default.SelectedCulture = "en-US";
                }
                TranslationManager.Instance.TranslationProvider = new ResxTranslationProvider("GAPPSF.Resources.Resources", Assembly.GetExecutingAssembly());
                base.OnStartup(e);
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}
