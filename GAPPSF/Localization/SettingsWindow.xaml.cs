using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Xml;

namespace GAPPSF.Localization
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public class LanguageItem
        {
            public string Item1 { get; set; }
            public string Item2 { get; set; }

            public LanguageItem(string it1, string it2)
            {
                Item1 = it1;
                Item2 = it2;
            }
        }
        public ObservableCollection<LanguageItem> TranslationData { get; set; }
        private string _prevLng = null;
        private Hashtable _overrideData;

        public SettingsWindow()
        {
            _overrideData = new Hashtable();
            TranslationData = new ObservableCollection<LanguageItem>();
            InitializeComponent();

            DataContext = this;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            saveData();

            TranslationData.Clear();
            _overrideData.Clear();
            string lng = null;
            switch (languageSelectCombo.SelectedIndex)
            {
                case 0:
                    lng = "en";
                    break;
                case 1:
                    lng = "de";
                    break;
                case 2:
                    lng = "nl";
                    break;
                case 3:
                    lng = "fr";
                    break;
            }
            if (!string.IsNullOrEmpty(lng))
            {
                string xmlFileContents = null;

                //user file
                string fn = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, string.Format("Language.{0}.xml", lng));
                if (System.IO.File.Exists(fn))
                {
                    xmlFileContents = System.IO.File.ReadAllText(fn);
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
                                TranslationData.Add(new LanguageItem(sn.Attributes["name"].InnerText.ToLower(), sn.Attributes["value"].InnerText));
                            }
                        }
                    }
                }

                //override
                xmlFileContents = null;
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(string.Format("pack://application:,,,/Resources/Language.{0}.xml", lng)));
                if (sri != null)
                {
                    using (System.IO.StreamReader textStreamReader = new System.IO.StreamReader(sri.Stream))
                    {
                        xmlFileContents = textStreamReader.ReadToEnd();
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
                                string s = sn.Attributes["name"].InnerText.ToLower();

                                if (_overrideData[s] == null)
                                {
                                    _overrideData.Add(s, sn.Attributes["value"].InnerText);
                                    if ((from a in TranslationData where string.Compare(a.Item1, s) == 0 select a).Count() == 0)
                                    {
                                        TranslationData.Add(new LanguageItem(s, sn.Attributes["value"].InnerText));
                                    }
                                }
                            }
                        }
                    }
                }

                //resource, but not in override
                System.Resources.ResourceSet rset = Localization.TranslationManager.Instance.TranslationProvider.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, true);
                if (rset!=null)
                {
                    foreach (DictionaryEntry entry in rset)
                    {
                        string s = entry.Value as string;
                        if (s != null)
                        {
                            s = s.ToLower();
                            if ((from a in TranslationData where string.Compare(a.Item1, s) == 0 select a).Count() == 0)
                            {
                                TranslationData.Add(new LanguageItem(s, ""));
                            }
                        }
                    }
                }
            }
            _prevLng = lng;
        }

        private void saveData()
        {
            if (!string.IsNullOrEmpty(_prevLng))
            {
                //save!
                //save to 2 files:
                //1: language.xx.xml -> only user edited entries
                //2: language.xx.full.xml -> for translators, sending to developer to copy into resource
                
                //user
                //todo
                string fn = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, string.Format("Language.{0}.xml", _prevLng));
                foreach (LanguageItem tpl in TranslationData)
                {

                }
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("resources");
                foreach (LanguageItem tpl in TranslationData)
                {
                    if (!string.IsNullOrEmpty(tpl.Item2))
                    {
                        //if not the same as override
                        if (!string.IsNullOrEmpty(_overrideData[tpl.Item1] as string) && string.Compare(_overrideData[tpl.Item1] as string, tpl.Item2) != 0)
                        {
                            XmlElement lngElement = doc.CreateElement("string");
                            lngElement.SetAttribute("name", tpl.Item1);
                            lngElement.SetAttribute("value", tpl.Item2);
                            root.AppendChild(lngElement);
                        }
                    }
                }
                doc.AppendChild(root);
                using (System.IO.TextWriter sw = new System.IO.StreamWriter(fn, false, Encoding.UTF8)) //Set encoding
                {
                    doc.Save(sw);
                }


                //full:
                fn = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, string.Format("Language.{0}.full.xml", _prevLng));
                List<LanguageItem> sortedDict = (from a in TranslationData orderby a.Item1 select a).ToList();
                doc = new XmlDocument();
                root = doc.CreateElement("resources");
                foreach (LanguageItem di in sortedDict)
                {
                    XmlElement lngElement = doc.CreateElement("string");
                    lngElement.SetAttribute("name", di.Item1);
                    lngElement.SetAttribute("value", di.Item2);
                    root.AppendChild(lngElement);
                }
                doc.AppendChild(root);
                using (System.IO.TextWriter sw = new System.IO.StreamWriter(fn, false, Encoding.UTF8)) //Set encoding
                {
                    doc.Save(sw);
                }

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveData();
        }
    }
}
