using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public class SettingsFolder : Utils.BasePlugin.Plugin
    {
        public const string STR_ERROR = "Error";
        public const string STR_ASKSAVEDATA = "The data has been changed. Do you want to save the data first?";
        public const string STR_WARNING = "Warning";

        public const string ACTION_SHOW = "Settings scope|Edit";
        public const string ACTION_SEP = "Settings scope|-";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_SHOW);
            AddAction(ACTION_SEP);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ASKSAVEDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WARNING));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_AVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_COPYCURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_COPYDEFAULT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_CURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_ENABLESTARTUP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_FOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_SETTINGSFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_SWITCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_TARGETSETTINGSFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_TITLE));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Account;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();

            string[] ail = Core.SettingsProvider.GetSettingsScopes().ToArray();
            if (ail != null && ail.Length > 0)
            {
                string[] uail = ail.Distinct().ToArray();
                foreach (string ai in uail)
                {
                    AddToMenu(ai);
                }
            }
        }

        public void AddToMenu(string folder)
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            AddAction(string.Format("Settings scope|{0}", folder));
            main.AddAction(this, "Settings scope", folder);
        }
        public void RemoveFromMenu(string folder)
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            RemoveAction(string.Format("Settings scope|{0}", folder));
            main.RemoveAction(this, "Settings scope", folder);
        }

        public async Task SwitchSettingsFolder(string newFolder)
        {
            if (Core.SettingsProvider.GetSettingsScope().ToLower() != newFolder.ToLower())
            {
                bool cancel = false;
                if (Core.AutoSaveOnClose)
                {
                    Framework.Interfaces.IPluginInternalStorage p = (from Framework.Interfaces.IPluginInternalStorage ip in Core.GetPlugin(Framework.PluginType.InternalStorage) select ip).FirstOrDefault();
                    if (p != null)
                    {
                        cancel = !await p.SaveAllData();
                    }
                }
                else
                {
                    int cnt = 0;
                    cnt += (from Framework.Data.Geocache c in Core.Geocaches where !c.Saved select c).Count();
                    cnt += (from Framework.Data.Log c in Core.Logs where !c.Saved select c).Count();
                    cnt += (from Framework.Data.LogImage c in Core.LogImages where !c.Saved select c).Count();
                    cnt += (from Framework.Data.Waypoint c in Core.Waypoints where !c.Saved select c).Count();
                    if (cnt > 0)
                    {
                        System.Windows.Forms.DialogResult res = System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ASKSAVEDATA), Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button3);
                        if (res == System.Windows.Forms.DialogResult.Yes)
                        {
                            Framework.Interfaces.IPluginInternalStorage p = (from Framework.Interfaces.IPluginInternalStorage ip in Core.GetPlugin(Framework.PluginType.InternalStorage) select ip).FirstOrDefault();
                            if (p != null)
                            {
                                cancel = !await p.SaveAllData();
                            }
                        }
                        else if (res == System.Windows.Forms.DialogResult.No)
                        {
                        }
                        else
                        {
                            cancel = true;
                        }
                    }
                }
                if (!cancel)
                {
                    Core.SettingsProvider.SetSettingsScopeForNextStart(newFolder);
                    Core.PrepareClosingApplication();
                    Application.Restart();
                }
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SHOW)
                {
                    using (SettingsFolderForm dlg = new SettingsFolderForm(this, Core))
                    {
                        dlg.ShowDialog();
                    }
                }
                else
                {
                    string[] parts = action.Split(new char[] { '|' }, 2);
                    if (parts.Length == 2 && parts[0] == "Settings scope")
                    {
                        await SwitchSettingsFolder(parts[1]);
                    }
                }
            }
            return result;
        }
    }
}
