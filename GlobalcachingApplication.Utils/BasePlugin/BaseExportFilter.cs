using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseExportFilter: Plugin
    {
        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.ExportData; }
        }

        //On UI Context
        async public virtual Task PerformExport()
        {
            await Task.Run(() => 
            {
                try
                {
                    ExportMethod();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            });
        }

        protected virtual void ExportMethod()
        {
        }

    }
}
