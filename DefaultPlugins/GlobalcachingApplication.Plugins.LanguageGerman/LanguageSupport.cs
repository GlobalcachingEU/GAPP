using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Data.Common;

namespace GlobalcachingApplication.Plugins.LanguageGerman
{
    public class LanguageSupport: Utils.BasePlugin.BaseLanguageSupport
    {
        private Hashtable _fixedLookupTable = new Hashtable();
        private Hashtable _customLookupTable = new Hashtable();
        private string _customDictionaryDatabaseFile = "";
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

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            bool result = false;

            _customDictionaryDatabaseFile = System.IO.Path.Combine(core.PluginDataPath, "LanguageGerman.db3" );

            if (base.Initialize(core))
            {
                LanguageInfo li = new LanguageInfo();
                li.Action = "German";
                li.CultureInfo = new System.Globalization.CultureInfo(0x0407);
                li.Action = li.CultureInfo.NativeName;
                SupportedLanguages.Add(li);

                try
                {
                    string fld = System.IO.Path.GetDirectoryName(_customDictionaryDatabaseFile);
                    if (!System.IO.Directory.Exists(fld))
                    {
                        System.IO.Directory.CreateDirectory(fld);
                    }
                }
                catch
                {
                }


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
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_customDictionaryDatabaseFile))
                {
                    object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='translation'");
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        dbcon.ExecuteNonQuery("create table 'translation' (item_name text, item_value text)");
                    }
                    DbDataReader dr = dbcon.ExecuteReader("select * from translation");
                    while (dr.Read())
                    {
                        _customLookupTable[dr["item_name"].ToString().ToLower()] = dr["item_value"].ToString();
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
                        using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_customDictionaryDatabaseFile))
                        {
                            foreach (DictionaryItem di in dict)
                            {
                                string s = _fixedLookupTable[di.Key.ToLower()] as string;
                                string cs = _customLookupTable[di.Key.ToLower()] as string;
                                if (string.IsNullOrEmpty(di.Value) || di.Value==s)
                                {
                                    //remove from custom
                                    if (!string.IsNullOrEmpty(cs))
                                    {
                                        dbcon.ExecuteNonQuery(string.Format("delete from translation where item_name='{0}'", di.Key.ToLower().Replace("'", "''")));
                                        _customLookupTable.Remove(di.Key.ToLower());
                                        changed = true;
                                    }
                                }
                                else if (di.Value!=s && di.Value!=cs)
                                {
                                    _customLookupTable[di.Key.ToLower()] = di.Value;
                                    if (dbcon.ExecuteNonQuery(string.Format("update translation set item_value='{1}' where item_name='{0}'", di.Key.ToLower().Replace("'", "''"), di.Value.Replace("'", "''"))) == 0)
                                    {
                                        dbcon.ExecuteNonQuery(string.Format("insert into translation (item_name, item_value) values ('{0}', '{1}')", di.Key.ToLower().Replace("'", "''"), di.Value.Replace("'", "''")));
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
