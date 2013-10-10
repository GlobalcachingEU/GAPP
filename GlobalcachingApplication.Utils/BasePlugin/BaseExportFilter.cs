using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseExportFilter: Plugin
    {
        private ManualResetEvent _actionReady = new ManualResetEvent(false);

        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.ExportData; }
        }

        public override void Close()
        {
            if (_actionReady != null)
            {
                _actionReady.Close();
                _actionReady = null;
            }
            base.Close();
        }

        //On UI Context
        public virtual void PerformExport()
        {
            _actionReady.Reset();
            Thread thrd = new Thread(new ThreadStart(this.exportThreadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
        }

        private void exportThreadMethod()
        {
            try
            {
                ExportMethod();
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            //signal finished
            _actionReady.Set();
        }

        protected virtual void ExportMethod()
        {
        }

    }
}
