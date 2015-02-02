using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Data.Common;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.LanguageGerman
{
    public class LanguageSupport: Utils.BasePlugin.BaseLanguageSupport
    {
        private Hashtable _fixedLookupTable = new Hashtable();
        private Hashtable _customLookupTable = new Hashtable();
        public class DictionaryItem
        {
            public string Key { get; private set; }
            public string Value { get; set; }

            public DictionaryItem(string key, string val)
            {
                Key = key;
                Value = val;
            }
        }

        public class DictionaryPoco
        {
            public string item_name { get; set; }
            public string item_value { get; set; }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            bool result = false;

            if (await base.InitializeAsync(core))
            {
                LanguageInfo li = new LanguageInfo();
                li.Action = "German";
                li.CultureInfo = new System.Globalization.CultureInfo(0x0407);
                li.Action = li.CultureInfo.NativeName;
                SupportedLanguages.Add(li);

                initDatabase();

                result = true;
            }
            return result;
        }

        public override string FriendlyName
        {
            get
            {
                return "German translations";
            }
        }

        private void initDatabase()
        {
            string xmlFileContents;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.LanguageGerman.LookupDictionary.xml")))
            {
                xmlFileContents = textStreamReader.ReadToEnd();
            }
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
                        _fixedLookupTable[sn.Attributes["name"].InnerText.ToLower()] = sn.Attributes["value"].InnerText;
                    }
                }
            }

            try
            {
                lock (Core.SettingsProvider)
                {
                    if (!Core.SettingsProvider.TableExists(Core.SettingsProvider.GetFullTableName("translation_de")))
                    {
                        Core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (item_name text, item_value text)", Core.SettingsProvider.GetFullTableName("translation_de")));
                    }
                    List<DictionaryPoco> pocos = Core.SettingsProvider.Database.Fetch<DictionaryPoco>(string.Format("select * from {0}", Core.SettingsProvider.GetFullTableName("translation_de")));
                    foreach (var item in pocos)
                    {
                        _customLookupTable[item.item_name.ToLower()] = item.item_value;
                    }
                }
            }
            catch
            {
            }
        }

        public override string GetTranslation(System.Globalization.CultureInfo targetCulture, string text)
        {
            string result = null;
            if (targetCulture.LCID == 0x0407)
            {
                result = GetTranslation(text);
            }
            return result;
        }

        private string GetTranslation(string text)
        {
            string result = null;
            result = _customLookupTable[text.ToLower()] as string;
            if (string.IsNullOrEmpty(result))
            {
                result = _fixedLookupTable[text.ToLower()] as string;
            }
            return result;
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    SettingsPanel sp = uc as SettingsPanel;
                    List<DictionaryItem> dict = (List<DictionaryItem>)sp.dictionaryEdit1.dataGrid1.ItemsSource;

                    try
                    {
                        bool changed = false;
                        lock (Core.SettingsProvider)
                        {
                            foreach (DictionaryItem di in dict)
                            {
                                string s = _fixedLookupTable[di.Key.ToLower()] as string;
                                string cs = _customLookupTable[di.Key.ToLower()] as string;
                                if (string.IsNullOrEmpty(di.Value) || di.Value == s)
                                {
                                    //remove from custom
                                    if (!string.IsNullOrEmpty(cs))
                                    {
                                        Core.SettingsProvider.Database.Execute(string.Format("delete from {0} where item_name='{1}'", Core.SettingsProvider.GetFullTableName("translation_de"), di.Key.ToLower().Replace("'", "''")));
                                        _customLookupTable.Remove(di.Key.ToLower());
                                        changed = true;
                                    }
                                }
                                else if (di.Value != s && di.Value != cs)
                                {
                                    _customLookupTable[di.Key.ToLower()] = di.Value;
                                    if (Core.SettingsProvider.Database.Execute(string.Format("update {2} set item_value='{1}' where item_name='{0}'", di.Key.ToLower().Replace("'", "''"), di.Value.Replace("'", "''"), Core.SettingsProvider.GetFullTableName("translation_de"))) == 0)
                                    {
                                        Core.SettingsProvider.Database.Execute(string.Format("insert into {2} (item_name, item_value) values ('{0}', '{1}')", di.Key.ToLower().Replace("'", "''"), di.Value.Replace("'", "''"), Core.SettingsProvider.GetFullTableName("translation_de")));
                                    }
                                    changed = true;
                                }
                            }
                        }
#if DEBUG
                        if (true)
#else
                        if (changed)
#endif
                        {
                            List<DictionaryItem> sortedDict = (from a in dict orderby a.Key select a).ToList();
                            XmlDocument doc = new XmlDocument();
                            XmlElement root = doc.CreateElement("resources");
                            foreach (DictionaryItem di in sortedDict)
                            {
                                XmlElement lngElement = doc.CreateElement("string");
                                lngElement.SetAttribute("name", di.Key);
                                lngElement.SetAttribute("value", GetTranslation(di.Key) ?? "");
                                root.AppendChild(lngElement);
                            }
                            doc.AppendChild(root);
                            using (TextWriter sw = new StreamWriter(System.IO.Path.Combine(Core.PluginDataPath, "LanguageGerman.xml" ), false, Encoding.UTF8)) //Set encoding
                            {
                                doc.Save(sw);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return base.ApplySettings(configPanels);
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            List<DictionaryItem>  data = new List<DictionaryItem>();
            foreach (DictionaryEntry d in _customLookupTable)
            {
                data.Add(new DictionaryItem((string)d.Key, (string)d.Value));
            }
            foreach (DictionaryEntry d in _fixedLookupTable)
            {
                if (!_customLookupTable.Contains(d.Key))
                {
                    data.Add(new DictionaryItem((string)d.Key, (string)d.Value));
                }
            }
            foreach (Framework.Data.LanguageItem li in Core.LanguageItems)
            {
                string txt = li.Text.ToLower();
                if (!_fixedLookupTable.Contains(txt) && !_customLookupTable.Contains(txt))
                {
                    data.Add(new DictionaryItem(txt, ""));
                }
            }
            SettingsPanel sp = new SettingsPanel();
            sp.dictionaryEdit1.dataGrid1.ItemsSource = data;
            pnls.Add(sp);
            return pnls;
        }

    }
}
