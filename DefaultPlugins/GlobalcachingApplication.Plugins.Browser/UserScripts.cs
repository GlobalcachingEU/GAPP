using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CSScriptLibrary;
using System.IO;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class UserScripts
    {
        public class Script
        {
            public string Name { get; set; }
            public bool Enabled { get; set; }
            public string ClassCode { get; set; }
        }

        public string FileName { get; set; }
        public string UsingNamespaces { get; set; }
        public List<Script> Scripts { get; private set; }

        private Assembly _compiledAssembly = null;
        private bool _isCompiled = false;

        public UserScripts()
        {
            Scripts = new List<Script>();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Windows.Forms;");
            sb.AppendLine("using GlobalcachingApplication.Framework.Interfaces;");
            sb.AppendLine("using GlobalcachingApplication.Utils;");
            sb.AppendLine("using GlobalcachingApplication.Utils.BasePlugin;");
            UsingNamespaces = sb.ToString();
        }

        public Assembly CompiledAssembly
        {
            get
            {
                if (!_isCompiled)
                {
                    compileScripts();
                }
                return _compiledAssembly;
            }
        }

        public string GetTemplate(string name)
        {
            string result;
            using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.Browser.BrowserScriptTemplate.txt")))
            {
                result = textStreamReader.ReadToEnd();
            }
            string id = Guid.NewGuid().ToString("N");
            result = result.Replace("<name>", name);
            result = result.Replace("<id>", id);
            return result;
        }

        public void Invalidate()
        {
            _isCompiled = false;
            _compiledAssembly = null;
        }

        private void compileScripts()
        {
            if (Scripts.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(UsingNamespaces ?? "");
                sb.AppendLine();
                sb.AppendLine("namespace GlobalcachingApplication.Plugins.Browser");
                sb.AppendLine("{");
                foreach (Script script in Scripts)
                {
                    if (script.Enabled)
                    {
                        sb.AppendLine(script.ClassCode);
                    }
                }
                sb.AppendLine("}");
                try
                {
                    using (TemporaryFile tmpFile = new TemporaryFile(true))
                    {
                        File.WriteAllText(tmpFile.Path, sb.ToString());
                        _compiledAssembly = CSScript.Load(tmpFile.Path);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
                }
            }
            _isCompiled = true;
        }
    }
}
