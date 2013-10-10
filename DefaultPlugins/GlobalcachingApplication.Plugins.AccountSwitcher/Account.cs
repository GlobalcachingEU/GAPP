using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public class Account : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Account switcher|Edit";
        public const string ACTION_SEP = "Account switcher|-";

        private string _filename = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);
            AddAction(ACTION_SEP);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_ACCOUNTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_ACTIVATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_API));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_CLEARAUTHORIZE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_CREATEACCOUNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_ENABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_FROMCURRENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_GCCOMACCOUNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_MENBERSHIP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_NO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_YES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AccountsForm.STR_NEWACCOUNT));

            try
            {
                _filename = System.IO.Path.Combine(core.PluginDataPath, "accounts.xml" );
            }
            catch
            {
            }

            return base.Initialize(core);
        }

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();

            AddAccountsToMenu(AccountInfo.GetAccountInfos(_filename));
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Account;
            }
        }

        private void AddAccountsToMenu(List<AccountInfo> ail)
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            foreach (AccountInfo ai in ail)
            {
                AddAction(string.Format("Account switcher|{0}", ai.Name));
                main.AddAction(this, "Account switcher", ai.Name);
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SHOW)
                {
                    List<AccountInfo> ail = AccountInfo.GetAccountInfos(_filename);
                    using (AccountsForm dlg = new AccountsForm(Core, AccountInfo.GetAccountInfos(_filename)))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                            foreach (AccountInfo ai in ail)
                            {
                                RemoveAction(string.Format("Account switcher|{0}", ai.Name));
                                main.RemoveAction(this, "Account switcher", ai.Name);
                            }
                            AccountInfo.SetAccountInfos(_filename, dlg.AccountInfoSettings);
                            AddAccountsToMenu(dlg.AccountInfoSettings);
                        }
                    }
                }
                else
                {
                    string[] parts = action.Split(new char[] { '|' }, 2);
                    if (parts.Length == 2 && parts[0] == "Account switcher")
                    {
                        List<AccountInfo> ail = AccountInfo.GetAccountInfos(_filename);
                        if (ail != null)
                        {
                            AccountInfo ai = (from a in ail where a.Name == parts[1] select a).FirstOrDefault();
                            if (ai != null)
                            {
                                ai.RestoreSettings(Core);
                            }
                        }
                    }
                }
                result = true;
            }
            return result;
        }

    }
}
