using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public class SettingsFolder : Utils.BasePlugin.Plugin
    {
        public const string STR_ERROR = "Error";
        public const string STR_FOLDERNOTEXIST = "The folder does not exists";
        public const string STR_FOLDERNOTVALID = "The folder is not a valid settings folder";
        public const string STR_ASKSAVEDATA = "The data has been changed. Do you want to save the data first?";
        public const string STR_WARNING = "Warning";

        public const string ACTION_SHOW = "Settings folder|Edit";
        public const string ACTION_SEP = "Settings folder|-";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);
            AddAction(ACTION_SEP);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_FOLDERNOTEXIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_FOLDERNOTVALID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ASKSAVEDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WARNING));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_AVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_CLEANINGFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_COPYCURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_COPYDEFAULT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_COPYINGFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_CURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_ENABLESTARTUP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_FOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_SAMEASCURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_SETTINGSFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_SWITCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_TARGETSETTINGSFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsFolderForm.STR_ASKDELETEFILES));

            return base.Initialize(core);
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

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();

            string[] ail = Core.AvailablePluginDataPaths;
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
            AddAction(string.Format("Settings folder|{0}", folder));
            main.AddAction(this, "Settings folder", folder);
        }
        public void RemoveFromMenu(string folder)
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            RemoveAction(string.Format("Settings folder|{0}", folder));
            main.RemoveAction(this, "Settings folder", folder);
        }

        public bool IsValidSettingsFolder(string folder)
        {
            bool result = false;
            try
            {
                result = System.IO.File.Exists(System.IO.Path.Combine(folder, "GlobalcachingApplication.Core.Properties.Settings.xml"));
            }
            catch
            {
            }
            return result;
        }

        public void SwitchSettingsFolder(string newFolder)
        {
            if (newFolder.ToLower() != Core.PluginDataPath.ToLower())
            {
                try
                {
                    if (System.IO.Directory.Exists(newFolder))
                    {
                        if (IsValidSettingsFolder(newFolder))
                        {
                            bool cancel = false;
                            //check save data
                            if (Core.AutoSaveOnClose)
                            {
                                Framework.Interfaces.IPluginInternalStorage p = (from Framework.Interfaces.IPluginInternalStorage ip in Core.GetPlugin(Framework.PluginType.InternalStorage) select ip).FirstOrDefault();
                                if (p != null)
                                {
                                    cancel = !p.SaveAllData();
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
                                            cancel = !p.SaveAllData();
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
                                //set and... bye bye
                                Core.PluginDataPath = newFolder;
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FOLDERNOTVALID), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FOLDERNOTEXIST), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }

        public override bool Action(string action)
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
                    if (parts.Length == 2 && parts[0] == "Settings folder")
                    {
                        SwitchSettingsFolder(parts[1]);
                    }
                }
            }
            return result;
        }
    }
}
