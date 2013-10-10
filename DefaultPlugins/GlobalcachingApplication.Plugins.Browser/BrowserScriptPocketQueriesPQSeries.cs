using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptPocketQueriesPQSeries
    {
        public string ID { get; set; }
        public string SettingsID { get; set; }
        public string Name { get; set; }
        public string NameTemplate { get; set; }
        public List<DateTime> Dates { get; private set; }

        public BrowserScriptPocketQueriesPQSeries()
        {
            Dates = new List<DateTime>();
        }

        public override string ToString()
        {
            return Name ?? "";
        }

        public static List<BrowserScriptPocketQueriesPQSeries> Load(string filename)
        {
            List<BrowserScriptPocketQueriesPQSeries> result = new List<BrowserScriptPocketQueriesPQSeries>();
            try
            {
                if (System.IO.File.Exists(filename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("Serie");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            BrowserScriptPocketQueriesPQSeries pq = new BrowserScriptPocketQueriesPQSeries();
                            pq.ID = n.SelectSingleNode("ID").InnerText;
                            pq.SettingsID = n.SelectSingleNode("SettingsID").InnerText;
                            pq.Name = n.SelectSingleNode("Name").InnerText;
                            pq.NameTemplate = n.SelectSingleNode("NameTemplate").InnerText;
                            pq.Dates = new List<DateTime>();
                            XmlNode subn = n.SelectSingleNode("Dates");
                            if (subn != null)
                            {
                                XmlNodeList subs = subn.SelectNodes("Date");
                                if (subs != null)
                                {
                                    foreach (XmlNode sn in subs)
                                    {
                                        pq.Dates.Add(DateTime.Parse(sn.InnerText));
                                    }
                                }
                            }
                            result.Add(pq);
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public static void Save(string filename, List<BrowserScriptPocketQueriesPQSeries> lst)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("PQSeries");
                doc.AppendChild(root);
                foreach (BrowserScriptPocketQueriesPQSeries pq in lst)
                {
                    XmlElement bm = doc.CreateElement("Serie");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("ID");
                    XmlText txt = doc.CreateTextNode(pq.ID);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("SettingsID");
                    txt = doc.CreateTextNode(pq.SettingsID);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Name");
                    txt = doc.CreateTextNode(pq.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("NameTemplate");
                    txt = doc.CreateTextNode(pq.NameTemplate);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Dates");
                    bm.AppendChild(el);
                    foreach (DateTime i in pq.Dates)
                    {
                        XmlElement subel = doc.CreateElement("Date");
                        txt = doc.CreateTextNode(i.ToString("s"));
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }
                }
                doc.Save(filename);
            }
            catch
            {
            }
        }
    }
}
