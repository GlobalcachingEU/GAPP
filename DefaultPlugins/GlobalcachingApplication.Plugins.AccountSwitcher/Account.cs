using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public class Account : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Account switcher|Edit";
        public const string ACTION_SEP = "Account switcher|-";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

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

            return await base.InitializeAsync(core);
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();

            AddAccountsToMenu(AccountInfo.GetAccountInfos());
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
                    List<AccountInfo> ail = AccountInfo.GetAccountInfos();
                    using (AccountsForm dlg = new AccountsForm(Core, AccountInfo.GetAccountInfos()))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                            foreach (AccountInfo ai in ail)
                            {
                                RemoveAction(string.Format("Account switcher|{0}", ai.Name));
                                main.RemoveAction(this, "Account switcher", ai.Name);
                            }
                            AccountInfo.SetAccountInfos(dlg.AccountInfoSettings);
                            AddAccountsToMenu(dlg.AccountInfoSettings);
                        }
                    }
                }
                else
                {
                    string[] parts = action.Split(new char[] { '|' }, 2);
                    if (parts.Length == 2 && parts[0] == "Account switcher")
                    {
                        List<AccountInfo> ail = AccountInfo.GetAccountInfos();
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
