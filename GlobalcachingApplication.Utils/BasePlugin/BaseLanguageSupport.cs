using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseLanguageSupport: Plugin, Framework.Interfaces.ILanguageSupport
    {
        protected class LanguageInfo
        {
            public CultureInfo CultureInfo;
            public string Action;
        }
        private List<LanguageInfo> _supportedLanguages = new List<LanguageInfo>();

        protected override void InitUIMainWindow(Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin)
        {
            //add menu items
            foreach (LanguageInfo li in _supportedLanguages)
            {
                mainWindowPlugin.AddAction(this, li.Action);
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                foreach (LanguageInfo li in _supportedLanguages)
                {
                    if (action == li.Action)
                    {
                        Core.SelectedLanguage = li.CultureInfo;
                        break;
                    }
                }
            }
            return result;
        }

        public List<CultureInfo> GetSupportedCultures()
        {
            return (from a in _supportedLanguages select a.CultureInfo).ToList();
        }

        protected List<LanguageInfo> SupportedLanguages
        {
            get { return _supportedLanguages; }
        }

        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.LanguageSupport; }
        }

        public virtual string GetTranslation(CultureInfo targetCulture, string text)
        {
            return null;
        }

        public virtual string GetTranslation(CultureInfo sourceCulture, CultureInfo targetCulture, string text)
        {
            return null;
        }
    }
}
