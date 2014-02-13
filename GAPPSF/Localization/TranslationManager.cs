using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Resources;

namespace GAPPSF.Localization
{
    public class TranslationManager
    {
        private static TranslationManager _translationManager;
        private Hashtable _overrideTranslation;
        private Hashtable _userTranslation;

        public event EventHandler LanguageChanged;

        private void loadOverrides()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Core.Settings.Default.SelectedCulture);

            _overrideTranslation.Clear();
            string xmlFileContents = null;
            try
            {
                if (CurrentLanguage.TwoLetterISOLanguageName.ToLower().Length == 2 &&
                    CurrentLanguage.TwoLetterISOLanguageName.ToLower()!="iv")
                {
                    StreamResourceInfo sri = Application.GetResourceStream(new Uri(string.Format("pack://application:,,,/Resources/Language.{0}.xml", CurrentLanguage.TwoLetterISOLanguageName.ToLower())));
                    if (sri != null)
                    {
                        using (StreamReader textStreamReader = new StreamReader(sri.Stream))
                        {
                            xmlFileContents = textStreamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                //not avialable
            }
            if (xmlFileContents != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlFileContents);
                XmlElement root = doc.DocumentElement;
                XmlNodeList strngs = root.SelectNodes("string");
                if (strngs != null)
                {
                    foreach (XmlNode sn in strngs)
                    {
                        if (!string.IsNullOrEmpty(sn.Attributes["value"].InnerText))
                        {
                            _overrideTranslation[sn.Attributes["name"].InnerText.ToLower()] = sn.Attributes["value"].InnerText;
                        }
                    }
                }
            }

            //user translations
            _userTranslation.Clear();
            xmlFileContents = null;
            try
            {
                if (CurrentLanguage.TwoLetterISOLanguageName.ToLower().Length == 2 &&
                    CurrentLanguage.TwoLetterISOLanguageName.ToLower() != "iv")
                {
                    string fn = Path.Combine(Core.Settings.Default.SettingsFolder, string.Format("Language.{0}.xml", CurrentLanguage.TwoLetterISOLanguageName.ToLower()));
                    if (File.Exists(fn))
                    {
                        xmlFileContents = File.ReadAllText(fn);
                    }
                }
            }
            catch
            {
                //not available
            }
            if (xmlFileContents != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlFileContents);
                XmlElement root = doc.DocumentElement;
                XmlNodeList strngs = root.SelectNodes("string");
                if (strngs != null)
                {
                    foreach (XmlNode sn in strngs)
                    {
                        if (!string.IsNullOrEmpty(sn.Attributes["value"].InnerText))
                        {
                            _userTranslation[sn.Attributes["name"].InnerText.ToLower()] = sn.Attributes["value"].InnerText;
                        }
                    }
                }
            }
        }

        private TranslationManager() 
        {
            _overrideTranslation = new Hashtable();
            _userTranslation = new Hashtable();
            loadOverrides();
        }

        public CultureInfo CurrentLanguage
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if( value != Thread.CurrentThread.CurrentUICulture)
                {
                    Core.Settings.Default.SelectedCulture = value.ToString();
                    Thread.CurrentThread.CurrentUICulture = value;
                    loadOverrides();
                    OnLanguageChanged();
                }
            }
        }

        public IEnumerable<CultureInfo> Languages
        {
            get
            {
               if( TranslationProvider != null)
               {
                   return TranslationProvider.Languages;
               }
               return Enumerable.Empty<CultureInfo>();
            }
        }

        public static TranslationManager Instance
        {
            get
            {
                if (_translationManager == null)
                    _translationManager = new TranslationManager();
                return _translationManager;
            }
        }

        public ITranslationProvider TranslationProvider { get; set; }

        public void ReloadUserTranslation()
        {
            loadOverrides();
            OnLanguageChanged();
        }

        private void OnLanguageChanged()
        {
            if (LanguageChanged != null)
            {
                LanguageChanged(this, EventArgs.Empty);
            }
        }

        public object Translate(string key)
        {
            if( TranslationProvider!= null)
            {
                object translatedValue =TranslationProvider.Translate(key);
                if (translatedValue != null)
                {
                    //see if there is an override in XML file
                    if (translatedValue is string)
                    {
                        string patchedValue = _userTranslation[(translatedValue as string).ToLower()] as string;
                        if (!string.IsNullOrEmpty(patchedValue))
                        {
                            return patchedValue;
                        }
                        else
                        {
                            patchedValue = _overrideTranslation[(translatedValue as string).ToLower()] as string;
                            if (!string.IsNullOrEmpty(patchedValue))
                            {
                                return patchedValue;
                            }
                        }
                    }
                    return translatedValue;
                }
                else //not in resource file, but maybe in override xml file
                {
                    string patchedValue = _overrideTranslation[(key as string).ToLower()] as string;
                    if (!string.IsNullOrEmpty(patchedValue))
                    {
                        return patchedValue;
                    }
                }
            }
#if DEBUG
            return string.Format("!{0}!", key);
#else
            return string.Format(key);
#endif
        }
    }
}
