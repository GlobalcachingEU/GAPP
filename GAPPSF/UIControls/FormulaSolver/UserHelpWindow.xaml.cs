using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.FormulaSolver
{
    /// <summary>
    /// Interaction logic for UserHelpWindow.xaml
    /// </summary>
    public partial class UserHelpWindow : Window
    {
        public UserHelpWindow()
        {
            InitializeComponent();

            string defaultLanguage = "en";
            if (Localization.TranslationManager.Instance.CurrentLanguage != null)
            {
                if (Localization.TranslationManager.Instance.CurrentLanguage.Name.Length > 1)
                {
                    defaultLanguage = Localization.TranslationManager.Instance.CurrentLanguage.Name.Substring(0, 2).ToLower();
                    if (defaultLanguage != "de" &&
                        defaultLanguage != "nl" &&
                        defaultLanguage != "fr")
                    {
                        defaultLanguage = "en";
                    }
                }
            }
            try
            {
                string tmpFolder = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, "FormulaSolverHelp");
                if (!System.IO.Directory.Exists(tmpFolder))
                {
                    System.IO.Directory.CreateDirectory(tmpFolder);
                }
                if (ExtractHelpFiles(defaultLanguage, tmpFolder))
                {
                    Uri uri = new System.Uri(System.IO.Path.Combine(tmpFolder, "formulasolverdocs.html"));
                    webHelp.Navigate(uri);
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }

        }

        private bool ExtractHelpFiles(string language, string folder)
        {
            string prefix = "GAPPSF.UIControls.FormulaSolver.Documentation." + language.Replace('-', '_') + ".";

            var resourceNames = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(prefix));

            bool written = true;
            foreach (string r in resourceNames)
            {

                written = written && WriteResourceToFile(r, System.IO.Path.Combine(folder, r.Substring(prefix.Length)));
            }

            return written;
        }

        private bool WriteResourceToFile(string resourceName, string fileName)
        {
            using (System.IO.Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (s == null)
                {
                    throw new Exception("Cannot find embedded resource '" + resourceName + "'");
                }
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                using (System.IO.BinaryWriter sw = new System.IO.BinaryWriter(System.IO.File.Open(fileName, System.IO.FileMode.Create)))
                {
                    sw.Write(buffer);
                }
            }
            return true;
        }

    }
}
