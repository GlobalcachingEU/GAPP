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
            if (GAPPSF.Properties.Settings.Default.UpgradeNeeded)
            {
                GAPPSF.Properties.Settings.Default.Upgrade();
                GAPPSF.Properties.Settings.Default.UpgradeNeeded = false;
                GAPPSF.Properties.Settings.Default.Save();
            }
            string sc = Core.Settings.Default.SelectedCulture;
            if (sc==null)
            {
                Core.Settings.Default.SelectedCulture = "en-US";
            }
            TranslationManager.Instance.TranslationProvider = new ResxTranslationProvider("GAPPSF.Resources.Resources", Assembly.GetExecutingAssembly());
            base.OnStartup(e);
        }
    }
}
