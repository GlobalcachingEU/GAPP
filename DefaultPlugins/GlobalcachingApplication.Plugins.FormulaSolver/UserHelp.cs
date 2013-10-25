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
using System.Diagnostics;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public partial class UserHelp : Form
    {
        private ITempDirProvider _tmpDirProvider = null;
        private Framework.Interfaces.ICore _core = null;

        public UserHelp(ITempDirProvider tmpDirProvider, Framework.Interfaces.ICore core)
        {
            _tmpDirProvider = tmpDirProvider;
            _core = core;
            InitializeComponent();
        }

        private bool WriteResourceToFile(string resourceName, string fileName)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (s == null)
                {
                    throw new Exception("Cannot find embedded resource '" + resourceName + "'");
                }
                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                using (BinaryWriter sw = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    sw.Write(buffer);
                }
            }
            return true;
        }

        private bool ExtractHelpFiles(string language)
        {
            if (_tmpDirProvider == null)
            {
                return false;
            }

            string prefix = "GlobalcachingApplication.Plugins.FormulaSolver.Documentation." + language.Replace('-', '_') + ".";

            var resourceNames = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(prefix));

            string tempFileDir = _tmpDirProvider.GetPluginTempDirectory();
            bool written = true;
            foreach (string r in resourceNames)
            {

                written = written && WriteResourceToFile(r, System.IO.Path.Combine(tempFileDir, r.Substring(prefix.Length)));
            }

            return written;
        }

        private void UserHelp_Load(object sender, EventArgs e)
        {
            string defaultLanguage = "en";
            if (_core != null)
            {
                if (_core.SelectedLanguage != null)
                {
                    if (_core.SelectedLanguage.Name.Length > 1)
                    {
                        defaultLanguage = _core.SelectedLanguage.Name.Substring(0, 2).ToLower();
                        if (defaultLanguage != "de" &&
                            defaultLanguage != "nl" &&
                            defaultLanguage != "fr")
                        {
                            defaultLanguage = "en";
                        }
                    }
                }
            }
            if (ExtractHelpFiles(defaultLanguage))
            {
                Uri uri = new System.Uri(System.IO.Path.Combine(
                        _tmpDirProvider.GetPluginTempDirectory(), 
                        "formulasolverdocs.html"));
                webHelp.Url = uri;
            }
        }
    }
}
