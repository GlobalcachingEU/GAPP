using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GlobalcachingApplication.Plugins.SimpleCacheList
{
    public class Preset
    {
        public string Name { get; set; }
        public string VisibleColumns { get; set; }
        public string ColumnOrder { get; set; }

        public override string ToString()
        {
            return Name ?? "";
        }

        public static Preset FromCurrentSettings(Preset org, string name)
        {
            Preset result;
            if (org == null)
            {
                result = new Preset();
            }
            else
            {
                result = org;
            }
            result.Name = name;
            result.VisibleColumns = Properties.Settings.Default.VisibleColumns ?? "";
            result.ColumnOrder = Properties.Settings.Default.ColumnOrder ?? "";
            return result;
        }

        public static List<Preset> LoadFromFile(string filename)
        {
            List<Preset> result = new List<Preset>();
            try
            {
                if (System.IO.File.Exists(filename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("preset");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            Preset bm = new Preset();
                            bm.Name = n.SelectSingleNode("Name").InnerText;
                            bm.VisibleColumns = n.SelectSingleNode("VisibleColumns").InnerText;
                            bm.ColumnOrder = n.SelectSingleNode("ColumnOrder").InnerText;
                            result.Add(bm);
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public static void SaveToFile(string filename, List<Preset> presets)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("presets");
                doc.AppendChild(root);
                foreach (Preset bmi in presets)
                {
                    XmlElement bm = doc.CreateElement("preset");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(bmi.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("VisibleColumns");
                    txt = doc.CreateTextNode(bmi.VisibleColumns);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("ColumnOrder");
                    txt = doc.CreateTextNode(bmi.ColumnOrder);
                    el.AppendChild(txt);
                    bm.AppendChild(el);
                }
                doc.Save(filename);
            }
            catch
            {
            }
        }
    }
}
