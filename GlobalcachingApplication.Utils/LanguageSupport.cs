using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GlobalcachingApplication.Utils
{
    public class LanguageSupport
    {
        private static LanguageSupport _uniqueInstance = null;
        private static object _lockObject = new object();
        private CultureInfo _activeTargetCulture; //invariant
        private List<Framework.Interfaces.ILanguageSupport> _languageModules = new List<Framework.Interfaces.ILanguageSupport>();
        private Framework.Interfaces.ICore _core = null;

        private LanguageSupport()
        {
            _activeTargetCulture = System.Globalization.CultureInfo.CurrentCulture;
        }

        public static LanguageSupport Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new LanguageSupport();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public virtual void UpdateLanguageSupportPlugins(Framework.Interfaces.ICore core)
        {
            if (_core == null)
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GetLocationForm.STR_GET_LOCATION));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GetLocationForm.STR_LOCATION));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GetLocationForm.STR_OK));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GetLocationForm.STR_MAP));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GetLocationForm.STR_FROMHOMELOC));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GetLocationForm.STR_FROMCENTERLOC));

                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.LiveAPICachesLeftForm.STR_INFO));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.LiveAPICachesLeftForm.STR_LEFT));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.LiveAPICachesLeftForm.STR_MAX));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.LiveAPICachesLeftForm.STR_TITLE));

                core.LanguageItems.Add(new Framework.Data.LanguageItem(API.GeocachingLiveV6.STR_ERROR));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(API.GeocachingLiveV6.STR_MUSTAUTHORIZE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(API.GeocachingLiveV6.STR_PMREQUIRED));

                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowForm.STR_DECOUPLE_WINDOW));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowForm.STR_DOCK_WINDOW));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowForm.STR_NOTTOPMOST_WINDOW));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowForm.STR_TOPMOST_WINDOW));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowForm.STR_OPAQUEWHENIACTIVE));

                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowTransparencyForm.STR_TITLE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(BasePlugin.BaseUIChildWindowTransparencyForm.STR_OPACITY));

                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GeocachesIgnoredMessageForm.STR_ACTION_EDIT));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GeocachesIgnoredMessageForm.STR_OK));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GeocachesIgnoredMessageForm.STR_WARNING));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(Dialogs.GeocachesIgnoredMessageForm.STR_XIGNORED));

                foreach (var c in core.LogTypes)
                {
                    core.LanguageItems.Add(new Framework.Data.LanguageItem(c.Name));                    
                }
            }

            _core = core;
            List<Framework.Interfaces.IPlugin> p = core.GetPlugin(Framework.PluginType.LanguageSupport);
            if (p != null)
            {
                foreach (Framework.Interfaces.ILanguageSupport mwp in p)
                {
                    if (!_languageModules.Contains(mwp))
                    {
                        _languageModules.Add(mwp);
                    }
                }
                foreach (Framework.Interfaces.IPlugin mwp in _languageModules)
                {
                    if (!p.Contains(mwp))
                    {
                        _languageModules.Remove(mwp as Framework.Interfaces.ILanguageSupport);
                    }
                }
            }
        }

        public virtual CultureInfo TargetCulture
        {
            get { return (_core==null)?_activeTargetCulture:_core.SelectedLanguage; }
        }

        public string GetTranslation(string text)
        {
            string result = null;
            if (TargetCulture != CultureInfo.InvariantCulture)
            {
                foreach (Framework.Interfaces.ILanguageSupport mwp in _languageModules)
                {
                    result = mwp.GetTranslation(TargetCulture, text);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            if (result == null)
            {
                result = text;
            }
            return result;
        }

        public string GetTranslation(CultureInfo sourceCulture, string text)
        {
            string result = null;
            foreach (Framework.Interfaces.ILanguageSupport mwp in _languageModules)
            {
                result = mwp.GetTranslation(sourceCulture, TargetCulture, text);
                if (result != null)
                {
                    break;
                }
            }
            if (result == null)
            {
                result = text;
            }
            return result;
        }


        public List<CultureInfo> GetSupportedCultures()
        {
            List<CultureInfo> result = new List<CultureInfo>();
            foreach (Framework.Interfaces.ILanguageSupport mwp in _languageModules)
            {
                result.AddRange(mwp.GetSupportedCultures());
            }
            return result;
        }


    }
}
