using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.IO;

namespace GlobalcachingApplication.Core
{
    public class PortableSettings
    {
        private static object _lockObject = new object();

        public static void CopySettings(string basePath, string targetPath, System.Configuration.ApplicationSettingsBase settings, bool useCurrentSettings)
        {
            lock (_lockObject)
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);
                XmlElement root = doc.CreateElement("settings");
                doc.AppendChild(root);
                if (useCurrentSettings)
                {
                    foreach (SettingsPropertyValue bmi in settings.PropertyValues)
                    {
                        //filter PluginDataPath and other global settings!
                        if (bmi.Name == "PluginDataPath" ||
                            bmi.Name == "AvailablePluginDataPaths" ||
                            bmi.Name == "EnablePluginDataPathAtStartup")
                        {
                            if (settings.GetType().ToString() == "GlobalcachingApplication.Core.Properties.Settings")
                            {
                                continue;
                            }
                        }

                        XmlElement bm = doc.CreateElement("setting");
                        root.AppendChild(bm);

                        XmlElement el = doc.CreateElement("Name");
                        XmlText txt = doc.CreateTextNode(bmi.Name);
                        el.AppendChild(txt);
                        bm.AppendChild(el);

                        el = doc.CreateElement("Value");
                        object o = bmi.SerializedValue;
                        if (o != null)
                        {
                            txt = doc.CreateTextNode(o.ToString().Replace(basePath, targetPath));
                            el.AppendChild(txt);
                        }
                        bm.AppendChild(el);
                    }
                }
                else
                {
                    foreach (SettingsProperty bmi in settings.Properties)
                    {
                        //filter PluginDataPath and other global settings!
                        if (bmi.Name == "PluginDataPath" ||
                            bmi.Name == "AvailablePluginDataPaths" ||
                            bmi.Name == "EnablePluginDataPathAtStartup")
                        {
                            if (settings.GetType().ToString() == "GlobalcachingApplication.Core.Properties.Settings")
                            {
                                continue;
                            }
                        }

                        XmlElement bm = doc.CreateElement("setting");
                        root.AppendChild(bm);

                        XmlElement el = doc.CreateElement("Name");
                        XmlText txt = doc.CreateTextNode(bmi.Name);
                        el.AppendChild(txt);
                        bm.AppendChild(el);

                        el = doc.CreateElement("Value");
                        if (bmi.Name == "UpgradeNeeded")
                        {
                            txt = doc.CreateTextNode("False");
                            el.AppendChild(txt);
                        }
                        else
                        {
                            SettingsPropertyValue sp = new SettingsPropertyValue(bmi);
                            sp.PropertyValue = bmi.DefaultValue;
                            object o = sp.SerializedValue;
                            if (o != null)
                            {
                                txt = doc.CreateTextNode(sp.SerializedValue.ToString());
                                el.AppendChild(txt);
                            }
                        }
                        bm.AppendChild(el);
                    }
                }
                using (TextWriter sw = new StreamWriter(Path.Combine(targetPath, string.Format("{0}.xml", settings.GetType().ToString())), false, Encoding.UTF8)) //Set encoding
                {
                    doc.Save(sw);
                }
            }
        }

        public static void SaveSettings(string basePath, System.Configuration.ApplicationSettingsBase settings)
        {
            lock (_lockObject)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                    doc.AppendChild(dec);
                    XmlElement root = doc.CreateElement("settings");
                    doc.AppendChild(root);
                    foreach (SettingsPropertyValue bmi in settings.PropertyValues)
                    {
                        //filter PluginDataPath and other global settings!
                        if (bmi.Name == "PluginDataPath" ||
                            bmi.Name == "AvailablePluginDataPaths" ||
                            bmi.Name == "EnablePluginDataPathAtStartup")
                        {
                            if (settings.GetType().ToString() == "GlobalcachingApplication.Core.Properties.Settings")
                            {
                                continue;
                            }
                        }

                        XmlElement bm = doc.CreateElement("setting");
                        root.AppendChild(bm);

                        XmlElement el = doc.CreateElement("Name");
                        XmlText txt = doc.CreateTextNode(bmi.Name);
                        el.AppendChild(txt);
                        bm.AppendChild(el);

                        el = doc.CreateElement("Value");
                        object o = bmi.SerializedValue;
                        if (o != null)
                        {
                            txt = doc.CreateTextNode(o.ToString());
                            el.AppendChild(txt);
                        }
                        bm.AppendChild(el);
                    }
                    using (TextWriter sw = new StreamWriter(Path.Combine(basePath, string.Format("{0}.xml", settings.GetType().ToString())), false, Encoding.UTF8)) //Set encoding
                    {
                        doc.Save(sw);
                    }
                }
                catch
                {
                }
            }
        }
        public static void LoadSettings(string basePath, System.Configuration.ApplicationSettingsBase settings)
        {
            lock (_lockObject)
            {
                try
                {
                    //first set all to default first
                    //well, almost all, otherwise we could use reset
                    //exception: if the settings file does not exists AND the path is the system default
                    //then we don't set everything to default first, because that would erase all settings for existing user

                    string settingsFile = Path.Combine(basePath, string.Format("{0}.xml", settings.GetType().ToString()));
                    bool resetToDefault = true;

                    if (!File.Exists(settingsFile))
                    {
                        string p = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GlobalcachingApplication" }).ToLower().TrimEnd(new char[] { '\\', '/' });
                        string p2 = basePath.ToLower().TrimEnd(new char[] { '\\', '/' });
                        if (p == p2)
                        {
                            resetToDefault = false;
                        }
                    }

                    if (resetToDefault)
                    {
                        foreach (SettingsProperty bmi in settings.Properties)
                        {
                            //filter PluginDataPath and other global settings!
                            if (bmi.Name == "PluginDataPath" ||
                                bmi.Name == "AvailablePluginDataPaths" ||
                                bmi.Name == "EnablePluginDataPathAtStartup")
                            {
                                if (settings.GetType().ToString() == "GlobalcachingApplication.Core.Properties.Settings")
                                {
                                    continue;
                                }
                            }
                            else if (bmi.Name == "UpgradeNeeded")
                            {
                                continue;
                            }

                            SettingsPropertyValue pv = settings.PropertyValues[bmi.Name];
                            if (pv == null)
                            {
                                pv = new SettingsPropertyValue(bmi);
                                pv.SerializedValue = bmi.DefaultValue;
                                settings.PropertyValues.Add(pv);
                            }
                            else
                            {
                                pv.SerializedValue = bmi.DefaultValue;
                            }
                        }
                    }

                    if (File.Exists(settingsFile))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(Path.Combine(basePath, string.Format("{0}.xml", settings.GetType().ToString())));
                        XmlElement root = doc.DocumentElement;

                        XmlNodeList bmNodes = root.SelectNodes("setting");
                        if (bmNodes != null)
                        {
                            foreach (XmlNode n in bmNodes)
                            {
                                string name = n.SelectSingleNode("Name").InnerText;
                                SettingsPropertyValue bmi = settings.PropertyValues[name];
                                if (bmi == null)
                                {
                                    SettingsProperty p = settings.Properties[name];
                                    if (p != null)
                                    {
                                        bmi = new SettingsPropertyValue(p);
                                        settings.PropertyValues.Add(bmi);
                                    }
                                }
                                if (bmi != null)
                                {
                                    bmi.SerializedValue = n.SelectSingleNode("Value").InnerText;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
