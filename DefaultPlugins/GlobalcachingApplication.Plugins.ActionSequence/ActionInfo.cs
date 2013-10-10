using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GlobalcachingApplication.Plugins.ActionSequence
{
    public class SequenceInfo
    {
        public string Name { get; set; }

        public class ActionInfo
        {
            public string Plugin { get; set; }
            public string Action { get; set; }
            public string SubAction { get; set; }
        }
        public List<ActionInfo> Actions { get; private set; }

        public SequenceInfo()
        {
            Actions = new List<ActionInfo>();
        }

        public override string ToString()
        {
            return Name ?? "";
        }

        public static List<SequenceInfo> GetActionSequences(string filename)
        {
            List<SequenceInfo> result = new List<SequenceInfo>();
            try
            {
                if (System.IO.File.Exists(filename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("sequence");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            SequenceInfo bm = new SequenceInfo();
                            bm.Name = n.SelectSingleNode("Name").InnerText;

                            XmlNodeList anl = n.SelectSingleNode("actions").SelectNodes("action");
                            if (anl != null)
                            {
                                foreach (XmlNode an in anl)
                                {
                                    ActionInfo ai = new ActionInfo();
                                    ai.Plugin = an.SelectSingleNode("Plugin").InnerText;
                                    ai.Action = an.SelectSingleNode("Action").InnerText;
                                    ai.SubAction = an.SelectSingleNode("SubAction").InnerText;
                                    bm.Actions.Add(ai);
                                }
                            }
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

        public static void SaveActionSequences(string filename, List<SequenceInfo> aiList)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("sequences");
                doc.AppendChild(root);
                foreach (SequenceInfo bmi in aiList)
                {
                    XmlElement bm = doc.CreateElement("sequence");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(bmi.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("actions");
                    bm.AppendChild(el);

                    foreach (ActionInfo ai in bmi.Actions)
                    {
                        XmlElement ael = doc.CreateElement("action");
                        el.AppendChild(ael);

                        XmlElement subel = doc.CreateElement("Plugin");
                        txt = doc.CreateTextNode(ai.Plugin);
                        subel.AppendChild(txt);
                        ael.AppendChild(subel);

                        subel = doc.CreateElement("Action");
                        txt = doc.CreateTextNode(ai.Action);
                        subel.AppendChild(txt);
                        ael.AppendChild(subel);

                        subel = doc.CreateElement("SubAction");
                        txt = doc.CreateTextNode(ai.SubAction);
                        subel.AppendChild(txt);
                        ael.AppendChild(subel);
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
