using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public class Viewer : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Log image viewer";

        public const string STR_GRABBING_IMAGES = "Grabbing images...";

        private List<Framework.Data.LogImage> _logImagesToGrab = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GRABBING_IMAGES));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_BY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_LOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_SLIDESHOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ViewerForm.STR_TEXT));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_CACHEDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DOWNLOADALL));

            return await base.InitializeAsync(core);
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new ViewerForm(this, core));
        }

        public override string FriendlyName
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(this));
            return pnls;
        }

        public void DownloadAllLogImages()
        {
            try
            {
                _logImagesToGrab = (from Framework.Data.LogImage li in Core.LogImages select li).ToList();

                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_GRABBING_IMAGES, STR_GRABBING_IMAGES, _logImagesToGrab.Count, 0, true))
                {
                    int cnt = _logImagesToGrab.Count;
                    int orgListCount = cnt;
                    Thread[] thrd = new Thread[6];
                    for (int i = 0; i < thrd.Length; i++)
                    {
                        thrd[i] = new Thread(new ThreadStart(this.getImagesThreadMethod));
                        thrd[i].Start();
                    }
                    while (cnt > 0)
                    {
                        Thread.Sleep(500);
                        System.Windows.Forms.Application.DoEvents();
                        lock (_logImagesToGrab)
                        {
                            cnt = _logImagesToGrab.Count;
                        }
                        if (!progress.UpdateProgress(STR_GRABBING_IMAGES, STR_GRABBING_IMAGES, orgListCount, orgListCount - cnt))
                        {
                            lock (_logImagesToGrab)
                            {
                                _logImagesToGrab.Clear();
                                cnt = 0;
                            }
                        }
                    }
                    for (int i = 0; i < thrd.Length; i++)
                    {
                        thrd[i].Join();
                    }
                }

            }
            catch
            {
            }
        }

        private void getImagesThreadMethod()
        {
            Framework.Data.LogImage li = null;
            lock (_logImagesToGrab)
            {
                if (_logImagesToGrab.Count > 0)
                {
                    li = _logImagesToGrab[0];
                    _logImagesToGrab.RemoveAt(0);
                }
            }
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                string fnp = System.IO.Path.Combine(Core.PluginDataPath, "LogImageCache");
                while (li != null)
                {
                    try
                    {
                        string fn = System.IO.Path.Combine(fnp, System.IO.Path.GetFileName(li.Url));
                        if (!System.IO.File.Exists(fn))
                        {
                            wc.DownloadFile(li.Url, fn);
                        }
                    }
                    catch
                    {
                    }

                    li = null;
                    lock (_logImagesToGrab)
                    {
                        if (_logImagesToGrab.Count > 0)
                        {
                            li = _logImagesToGrab[0];
                            _logImagesToGrab.RemoveAt(0);
                        }
                    }
                }
            }
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
                            (UIChildWindowForm as ViewerForm).UpdateView();
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
