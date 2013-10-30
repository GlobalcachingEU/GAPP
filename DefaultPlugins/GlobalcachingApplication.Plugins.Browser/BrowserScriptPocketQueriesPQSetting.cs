using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptPocketQueriesPQSetting
    {
        public class AttributeState
        {
            public string ID { get; set; }
            public string Value { get; set; }
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public List<int> GenerateOnDays { get; set; }
        public int ExecutionScheme { get; set; }
        public string MaximumCaches { get; set; }
        public List<int> GeocacheTypes { get; set; }
        public List<int> GeocacheContainer { get; set; }
        public List<int> WhichOptions { get; set; }
        public string DifficultyCondition { get; set; }
        public string DifficultyValue { get; set; }
        public string TerrainCondition { get; set; }
        public string TerrainValue { get; set; }
        public string WithinSelection { get; set; }
        public List<string> Countries { get; set; }
        public List<string> States { get; set; }
        public string FromPointSelection { get; set; }
        public string FromPointGCCode { get; set; }
        public string FromPointPostalCode { get; set; }
        public string LatLonFormatValue { get; set; }
        public List<string> NorthSouthValues { get; set; }
        public List<string> EastWestValues { get; set; }
        public string WithinRadius { get; set; }
        public string WithinRadiusUnitType { get; set; }
        public string PlacedDuring { get; set; }
        public string PlacedDuringFixedInterval { get; set; }
        public DateTime PlacedDuringFromDate { get; set; }
        public DateTime PlacedDuringToDate { get; set; }
        public List<AttributeState> IncludeAttributes { get; set; }
        public List<AttributeState> ExcludeAttributes { get; set; }
        public string EMail { get; set; }
        public string FileFormat { get; set; }
        public bool CompressFile { get; set; }
        public bool PQNameInFileName { get; set; }

        public override string ToString()
        {
            return Name ?? "";
        }

        public static void Save(string filename, List<BrowserScriptPocketQueriesPQSetting> settings)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("PQSettings");
                doc.AppendChild(root);
                foreach (BrowserScriptPocketQueriesPQSetting pq in settings)
                {
                    XmlElement bm = doc.CreateElement("Setting");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("ID");
                    XmlText txt = doc.CreateTextNode(pq.ID);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Name");
                    txt = doc.CreateTextNode(pq.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("GenerateOnDays");
                    bm.AppendChild(el);
                    foreach (int i in pq.GenerateOnDays)
                    {
                        XmlElement subel = doc.CreateElement("Day");
                        txt = doc.CreateTextNode(i.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("ExecutionScheme");
                    txt = doc.CreateTextNode(pq.ExecutionScheme.ToString());
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("MaximumCaches");
                    txt = doc.CreateTextNode(pq.MaximumCaches);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("GeocacheTypes");
                    bm.AppendChild(el);
                    foreach (int i in pq.GeocacheTypes)
                    {
                        XmlElement subel = doc.CreateElement("Type");
                        txt = doc.CreateTextNode(i.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("GeocacheContainer");
                    bm.AppendChild(el);
                    foreach (int i in pq.GeocacheContainer)
                    {
                        XmlElement subel = doc.CreateElement("Container");
                        txt = doc.CreateTextNode(i.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("WhichOptions");
                    bm.AppendChild(el);
                    foreach (int i in pq.WhichOptions)
                    {
                        XmlElement subel = doc.CreateElement("Option");
                        txt = doc.CreateTextNode(i.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("DifficultyCondition");
                    txt = doc.CreateTextNode(pq.DifficultyCondition);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("DifficultyValue");
                    txt = doc.CreateTextNode(pq.DifficultyValue);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("TerrainCondition");
                    txt = doc.CreateTextNode(pq.TerrainCondition);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("TerrainValue");
                    txt = doc.CreateTextNode(pq.TerrainValue);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("WithinSelection");
                    txt = doc.CreateTextNode(pq.WithinSelection);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Countries");
                    bm.AppendChild(el);
                    foreach (string i in pq.Countries)
                    {
                        XmlElement subel = doc.CreateElement("Country");
                        txt = doc.CreateTextNode(i);
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("States");
                    bm.AppendChild(el);
                    foreach (string i in pq.States)
                    {
                        XmlElement subel = doc.CreateElement("State");
                        txt = doc.CreateTextNode(i);
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("FromPointSelection");
                    txt = doc.CreateTextNode(pq.FromPointSelection);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("FromPointGCCode");
                    txt = doc.CreateTextNode(pq.FromPointGCCode);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("FromPointPostalCode");
                    txt = doc.CreateTextNode(pq.FromPointPostalCode);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("LatLonFormatValue");
                    txt = doc.CreateTextNode(pq.LatLonFormatValue);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("NorthSouthValues");
                    bm.AppendChild(el);
                    foreach (string i in pq.NorthSouthValues)
                    {
                        XmlElement subel = doc.CreateElement("Value");
                        txt = doc.CreateTextNode(i);
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("EastWestValues");
                    bm.AppendChild(el);
                    foreach (string i in pq.EastWestValues)
                    {
                        XmlElement subel = doc.CreateElement("Value");
                        txt = doc.CreateTextNode(i);
                        subel.AppendChild(txt);
                        el.AppendChild(subel);
                    }

                    el = doc.CreateElement("WithinRadius");
                    txt = doc.CreateTextNode(pq.WithinRadius);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("WithinRadiusUnitType");
                    txt = doc.CreateTextNode(pq.WithinRadiusUnitType);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("PlacedDuring");
                    txt = doc.CreateTextNode(pq.PlacedDuring);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("PlacedDuringFixedInterval");
                    txt = doc.CreateTextNode(pq.PlacedDuringFixedInterval);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("PlacedDuringFromDate");
                    txt = doc.CreateTextNode(pq.PlacedDuringFromDate.ToString("s"));
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("PlacedDuringToDate");
                    txt = doc.CreateTextNode(pq.PlacedDuringToDate.ToString("s"));
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("IncludeAttributes");
                    bm.AppendChild(el);
                    foreach (AttributeState i in pq.IncludeAttributes)
                    {
                        XmlElement attrel = doc.CreateElement("Attr");
                        el.AppendChild(attrel);

                        XmlElement subel = doc.CreateElement("ID");
                        txt = doc.CreateTextNode(i.ID);
                        subel.AppendChild(txt);
                        attrel.AppendChild(subel);

                        subel = doc.CreateElement("Value");
                        txt = doc.CreateTextNode(i.Value);
                        subel.AppendChild(txt);
                        attrel.AppendChild(subel);
                    }

                    el = doc.CreateElement("ExcludeAttributes");
                    bm.AppendChild(el);
                    foreach (AttributeState i in pq.ExcludeAttributes)
                    {
                        XmlElement attrel = doc.CreateElement("Attr");
                        el.AppendChild(attrel);

                        XmlElement subel = doc.CreateElement("ID");
                        txt = doc.CreateTextNode(i.ID);
                        subel.AppendChild(txt);
                        attrel.AppendChild(subel);

                        subel = doc.CreateElement("Value");
                        txt = doc.CreateTextNode(i.Value);
                        subel.AppendChild(txt);
                        attrel.AppendChild(subel);
                    }

                    el = doc.CreateElement("EMail");
                    txt = doc.CreateTextNode(pq.EMail);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("FileFormat");
                    txt = doc.CreateTextNode(pq.FileFormat);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("CompressFile");
                    txt = doc.CreateTextNode(pq.CompressFile.ToString());
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("PQNameInFileName");
                    txt = doc.CreateTextNode(pq.PQNameInFileName.ToString());
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                }
                doc.Save(filename);
            }
            catch
            {
            }
        }

        public static List<BrowserScriptPocketQueriesPQSetting> Load(string filename)
        {
            List<BrowserScriptPocketQueriesPQSetting> result = new List<BrowserScriptPocketQueriesPQSetting>();

            if (System.IO.File.Exists(filename))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlElement root = doc.DocumentElement;

                XmlNodeList bmNodes = root.SelectNodes("Setting");
                if (bmNodes != null)
                {
                    foreach (XmlNode n in bmNodes)
                    {
                        BrowserScriptPocketQueriesPQSetting pq = new BrowserScriptPocketQueriesPQSetting();
                        pq.ID = n.SelectSingleNode("ID").InnerText;
                        pq.Name = n.SelectSingleNode("Name").InnerText;
                        pq.GenerateOnDays = new List<int>();
                        XmlNode subn = n.SelectSingleNode("GenerateOnDays");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Day");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.GenerateOnDays.Add(int.Parse(sn.InnerText));
                                }
                            }
                        }
                        pq.ExecutionScheme = int.Parse(n.SelectSingleNode("ExecutionScheme").InnerText);
                        pq.MaximumCaches = n.SelectSingleNode("MaximumCaches").InnerText;
                        pq.GeocacheTypes = new List<int>();
                        subn = n.SelectSingleNode("GeocacheTypes");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Type");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.GeocacheTypes.Add(int.Parse(sn.InnerText));
                                }
                            }
                        }
                        pq.GeocacheContainer = new List<int>();
                        subn = n.SelectSingleNode("GeocacheContainer");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Container");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.GeocacheContainer.Add(int.Parse(sn.InnerText));
                                }
                            }
                        }
                        pq.WhichOptions = new List<int>();
                        subn = n.SelectSingleNode("WhichOptions");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Option");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.WhichOptions.Add(int.Parse(sn.InnerText));
                                }
                            }
                        }
                        pq.DifficultyCondition = n.SelectSingleNode("DifficultyCondition").InnerText;
                        pq.DifficultyValue = n.SelectSingleNode("DifficultyValue").InnerText;
                        pq.TerrainCondition = n.SelectSingleNode("TerrainCondition").InnerText;
                        pq.TerrainValue = n.SelectSingleNode("TerrainValue").InnerText;
                        pq.WithinSelection = n.SelectSingleNode("WithinSelection").InnerText;
                        pq.Countries = new List<string>();
                        subn = n.SelectSingleNode("Countries");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Country");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.Countries.Add(sn.InnerText);
                                }
                            }
                        }
                        pq.States = new List<string>();
                        subn = n.SelectSingleNode("States");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("State");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.States.Add(sn.InnerText);
                                }
                            }
                        }
                        pq.FromPointSelection = n.SelectSingleNode("FromPointSelection").InnerText;
                        pq.FromPointGCCode = n.SelectSingleNode("FromPointGCCode").InnerText;
                        pq.FromPointPostalCode = n.SelectSingleNode("FromPointPostalCode").InnerText;
                        pq.LatLonFormatValue = n.SelectSingleNode("LatLonFormatValue").InnerText;
                        pq.NorthSouthValues = new List<string>();
                        subn = n.SelectSingleNode("NorthSouthValues");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Value");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.NorthSouthValues.Add(sn.InnerText);
                                }
                            }
                        }
                        pq.EastWestValues = new List<string>();
                        subn = n.SelectSingleNode("EastWestValues");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Value");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    pq.EastWestValues.Add(sn.InnerText);
                                }
                            }
                        }
                        pq.WithinRadius = n.SelectSingleNode("WithinRadius").InnerText;
                        pq.WithinRadiusUnitType = n.SelectSingleNode("WithinRadiusUnitType").InnerText;
                        pq.PlacedDuring = n.SelectSingleNode("PlacedDuring").InnerText;
                        pq.PlacedDuringFixedInterval = n.SelectSingleNode("PlacedDuringFixedInterval").InnerText;
                        pq.PlacedDuringFromDate = DateTime.Parse(n.SelectSingleNode("PlacedDuringFromDate").InnerText);
                        pq.PlacedDuringToDate = DateTime.Parse(n.SelectSingleNode("PlacedDuringToDate").InnerText);
                        pq.IncludeAttributes = new List<AttributeState>();
                        subn = n.SelectSingleNode("IncludeAttributes");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Attr");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    AttributeState attr = new AttributeState();
                                    attr.ID = sn.SelectSingleNode("ID").InnerText;
                                    attr.Value = sn.SelectSingleNode("Value").InnerText;
                                    pq.IncludeAttributes.Add(attr);
                                }
                            }
                        }
                        pq.ExcludeAttributes = new List<AttributeState>();
                        subn = n.SelectSingleNode("ExcludeAttributes");
                        if (subn != null)
                        {
                            XmlNodeList subs = subn.SelectNodes("Attr");
                            if (subs != null)
                            {
                                foreach (XmlNode sn in subs)
                                {
                                    AttributeState attr = new AttributeState();
                                    attr.ID = sn.SelectSingleNode("ID").InnerText;
                                    attr.Value = sn.SelectSingleNode("Value").InnerText;
                                    pq.ExcludeAttributes.Add(attr);
                                }
                            }
                        }
                        pq.EMail = n.SelectSingleNode("EMail").InnerText;
                        pq.FileFormat = n.SelectSingleNode("FileFormat").InnerText;
                        pq.CompressFile = bool.Parse(n.SelectSingleNode("CompressFile").InnerText);
                        pq.PQNameInFileName = bool.Parse(n.SelectSingleNode("PQNameInFileName").InnerText);

                        result.Add(pq);
                    }
                }
            }

            return result;
        }

        public bool SetPQPage(WebBrowser wb)
        {
            return SetPQPage(wb,-1);
        }
        public bool SetPQPage(WebBrowser wb, int activateForDay)
        {
            bool result = false;
            try
            {
                HtmlDocument doc = wb.Document;
                HtmlElement head = doc.GetElementsByTagName("head")[0];
                if (head.InnerHtml.IndexOf("setOptionSelected1234") < 0)
                {
                    HtmlElement s = doc.CreateElement("script");
                    s.SetAttribute("text", "function setOptionSelected1234(name, index, sel) { document.getElementById(name).options[index].selected=sel; }");
                    head.AppendChild(s);
                }

                wb.Document.GetElementById("ctl00_ContentBody_tbName").SetAttribute("value", Name);
                for (int i = 0; i < 7; i++)
                {
                    if ((GenerateOnDays.Contains(i) || i==activateForDay) != checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbDays_{0}", i)))
                    {
                        wb.Document.GetElementById(string.Format("ctl00_ContentBody_cbDays_{0}", i)).InvokeMember("Click");
                    }
                }
                wb.Document.GetElementById(string.Format("ctl00_ContentBody_rbRunOption_{0}", ExecutionScheme)).InvokeMember("Click");
                wb.Document.GetElementById("ctl00_ContentBody_tbResults").SetAttribute("value", MaximumCaches);
                if (GeocacheTypes.Count == 0)
                {
                    wb.Document.GetElementById("ctl00_ContentBody_rbTypeAny").InvokeMember("Click");
                }
                else
                {
                    wb.Document.GetElementById("ctl00_ContentBody_rbTypeSelect").InvokeMember("Click");
                    for (int i = 0; i < 11; i++)
                    {
                        if (GeocacheTypes.Contains(i) != checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbTaxonomy_{0}", i)))
                        {
                            wb.Document.GetElementById(string.Format("ctl00_ContentBody_cbTaxonomy_{0}", i)).InvokeMember("Click");
                        }
                    }
                }
                if (GeocacheContainer.Count == 0)
                {
                    wb.Document.GetElementById("ctl00_ContentBody_rbContainerAny").InvokeMember("Click");
                }
                else
                {
                    wb.Document.GetElementById("ctl00_ContentBody_rbContainerSelect").InvokeMember("Click");
                    for (int i = 0; i < 7; i++)
                    {
                        if (GeocacheContainer.Contains(i) != checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbContainers_{0}", i)))
                        {
                            wb.Document.GetElementById(string.Format("ctl00_ContentBody_cbContainers_{0}", i)).InvokeMember("Click");
                        }
                    }
                }
                for (int i = 0; i < 14; i++)
                {
                    if (WhichOptions.Contains(i) != checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbOptions_{0}", i)))
                    {
                        wb.Document.GetElementById(string.Format("ctl00_ContentBody_cbOptions_{0}", i)).InvokeMember("Click");
                    }
                }
                if (!string.IsNullOrEmpty(DifficultyCondition) != checkBoxChecked(wb, "ctl00_ContentBody_cbDifficulty"))
                {
                    wb.Document.GetElementById("ctl00_ContentBody_cbDifficulty").InvokeMember("Click");
                }
                if (!string.IsNullOrEmpty(DifficultyCondition))
                {
                    wb.Document.GetElementById("ctl00_ContentBody_ddDifficulty").SetAttribute("value", DifficultyCondition);
                }
                wb.Document.GetElementById("ctl00_ContentBody_ddDifficultyScore").SetAttribute("value", DifficultyValue);
                if (!string.IsNullOrEmpty(TerrainCondition) != checkBoxChecked(wb, "ctl00_ContentBody_cbTerrain"))
                {
                    wb.Document.GetElementById("ctl00_ContentBody_cbTerrain").InvokeMember("Click");
                }
                if (!string.IsNullOrEmpty(TerrainCondition))
                {
                    wb.Document.GetElementById("ctl00_ContentBody_ddTerrain").SetAttribute("value", TerrainCondition);
                }
                wb.Document.GetElementById("ctl00_ContentBody_ddTerrainScore").SetAttribute("value", TerrainValue);
                wb.Document.GetElementById(WithinSelection).InvokeMember("Click");
                HtmlElement listel = wb.Document.GetElementById("ctl00_ContentBody_lbCountries");
                int index = 0;
                foreach (HtmlElement el in listel.Children)
                {
                    string cid = el.GetAttribute("value");

                    if (el.GetAttribute("selected") != null &&
                        (el.GetAttribute("selected").ToLower() == "selected" ||
                        el.GetAttribute("selected").ToLower() == "true"))
                    {
                        if (!Countries.Contains(cid))
                        {
                            wb.Document.InvokeScript("setOptionSelected1234", new object[] { "ctl00_ContentBody_lbCountries", index , false});
                        }
                    }
                    else
                    {
                        if (Countries.Contains(cid))
                        {
                            el.SetAttribute("selected", "selected");
                        }
                    }
                    index++;
                }
                listel = wb.Document.GetElementById("ctl00_ContentBody_lbStates");
                index = 0;
                foreach (HtmlElement el in listel.Children)
                {
                    string cid = el.GetAttribute("value");

                    if (el.GetAttribute("selected") != null &&
                        (el.GetAttribute("selected").ToLower() == "selected" ||
                        el.GetAttribute("selected").ToLower() == "true"))
                    {
                        if (!States.Contains(cid))
                        {
                            wb.Document.InvokeScript("setOptionSelected1234", new object[] { "ctl00_ContentBody_lbStates", index, false });
                        }
                    }
                    else
                    {
                        if (States.Contains(cid))
                        {
                            el.SetAttribute("selected", "selected");
                        }
                    }
                    index++;
                }
                wb.Document.GetElementById(FromPointSelection).InvokeMember("Click");
                wb.Document.GetElementById("ctl00_ContentBody_tbGC").SetAttribute("value", FromPointGCCode);
                wb.Document.GetElementById("ctl00_ContentBody_tbPostalCode").SetAttribute("value", FromPointPostalCode);
                GetElementByName(wb.Document.Body, "ctl00$ContentBody$LatLong").SetAttribute("value", LatLonFormatValue);
                wb.Document.GetElementById("ctl00_ContentBody_LatLong:_selectNorthSouth").SetAttribute("value", NorthSouthValues[0]);
                wb.Document.GetElementById("ctl00_ContentBody_LatLong:_selectEastWest").SetAttribute("value", EastWestValues[0]);
                if (LatLonFormatValue == "0")
                {
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatDegs").SetAttribute("value", NorthSouthValues[1]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongDegs").SetAttribute("value", EastWestValues[1]);
                }
                else if (LatLonFormatValue == "1")
                {
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatDegs").SetAttribute("value", NorthSouthValues[1]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatMins").SetAttribute("value", NorthSouthValues[2]);

                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongDegs").SetAttribute("value", EastWestValues[1]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongMins").SetAttribute("value", EastWestValues[2]);
                }
                else if (LatLonFormatValue == "2")
                {
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatDegs").SetAttribute("value", NorthSouthValues[1]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatMins").SetAttribute("value", NorthSouthValues[2]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatSecs").SetAttribute("value", NorthSouthValues[3]);

                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongDegs").SetAttribute("value", EastWestValues[1]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongMins").SetAttribute("value", EastWestValues[2]);
                    wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongSecs").SetAttribute("value", EastWestValues[3]);
                }
                wb.Document.GetElementById("ctl00_ContentBody_tbRadius").SetAttribute("value", WithinRadius);
                wb.Document.GetElementById(WithinRadiusUnitType).InvokeMember("Click");
                wb.Document.GetElementById(PlacedDuring).InvokeMember("Click");
                wb.Document.GetElementById("ctl00_ContentBody_ddLastPlaced").SetAttribute("value", PlacedDuringFixedInterval);
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Year").SetAttribute("value", PlacedDuringFromDate.Year.ToString());
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Year").RaiseEvent("onchange");
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Month").SetAttribute("value", PlacedDuringFromDate.Month.ToString());
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Month").RaiseEvent("onchange");
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Day").SetAttribute("value", PlacedDuringFromDate.Day.ToString());
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Day").RaiseEvent("onchange");
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Year").SetAttribute("value", PlacedDuringToDate.Year.ToString());
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Year").RaiseEvent("onchange");
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Month").SetAttribute("value", PlacedDuringToDate.Month.ToString());
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Month").RaiseEvent("onchange");
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Day").SetAttribute("value", PlacedDuringToDate.Day.ToString());
                wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Day").RaiseEvent("onchange");
                for (int i = 0; i < 70; i++)
                {
                    HtmlElement el = wb.Document.GetElementById(string.Format("ctl00_ContentBody_ctlAttrInclude_dtlAttributeIcons_ctl{0}_hidInput", i.ToString("00")));
                    if (el != null)
                    {
                        string aid = el.GetAttribute("AttID");
                        string aval = el.GetAttribute("value");
                        string tval = (from x in IncludeAttributes where x.ID == aid select x.Value).FirstOrDefault();
                        if (string.IsNullOrEmpty(tval))
                        {
                            tval = "0";
                        }
                        int tries = 0;
                        while (el.GetAttribute("value") != tval)
                        {
                            wb.Document.GetElementById(string.Format("ctl00_ContentBody_ctlAttrInclude_dtlAttributeIcons_ctl{0}_imgAttribute",i.ToString("00"))).InvokeMember("Click");
                            tries++;
                            if (tries > 3)
                            {
                                Exception ex = new Exception("error");
                                throw ex;
                            }
                        }
                    }
                }
                for (int i = 0; i < 70; i++)
                {
                    HtmlElement el = wb.Document.GetElementById(string.Format("ctl00_ContentBody_ctlAttrExclude_dtlAttributeIcons_ctl{0}_hidInput", i.ToString("00")));
                    if (el != null)
                    {
                        string aid = el.GetAttribute("AttID");
                        string aval = el.GetAttribute("value");
                        string tval = (from x in ExcludeAttributes where x.ID == aid select x.Value).FirstOrDefault();
                        if (string.IsNullOrEmpty(tval))
                        {
                            tval = "0";
                        }
                        int tries = 0;
                        while (el.GetAttribute("value") != tval)
                        {
                            wb.Document.GetElementById(string.Format("ctl00_ContentBody_ctlAttrExclude_dtlAttributeIcons_ctl{0}_imgAttribute", i.ToString("00"))).InvokeMember("Click");
                            tries++;
                            if (tries > 3)
                            {
                                Exception ex = new Exception("error");
                                throw ex;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(EMail))
                {
                    try
                    {
                        wb.Document.GetElementById("ctl00_ContentBody_ddlAltEmails").SetAttribute("value", EMail);
                    }
                    catch
                    {
                    }
                }
                wb.Document.GetElementById("ctl00_ContentBody_ddFormats").SetAttribute("value", FileFormat);
                if (CompressFile != checkBoxChecked(wb, "ctl00_ContentBody_cbZip"))
                {
                    wb.Document.GetElementById("ctl00_ContentBody_cbZip").InvokeMember("Click");
                }
                if (PQNameInFileName != checkBoxChecked(wb, "ctl00_ContentBody_cbIncludePQNameInFileName"))
                {
                    wb.Document.GetElementById("ctl00_ContentBody_cbIncludePQNameInFileName").InvokeMember("Click");
                }

                //javascript might have been triggered and therefore the document completed
                //wait a sec
                for (int i=0; i<10; i++)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                }

                result = true;
            }
            catch
            {
            }
            return result;
        }

        public static BrowserScriptPocketQueriesPQSetting FromPQPage(Framework.Interfaces.ICore core, Framework.Interfaces.IPlugin plugin, WebBrowser wb)
        {
            BrowserScriptPocketQueriesPQSetting result = new BrowserScriptPocketQueriesPQSetting();
            try
            {
                result.ID = Guid.NewGuid().ToString("N");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_tbName");
                result.Name = wb.Document.GetElementById("ctl00_ContentBody_tbName").GetAttribute("value");
                result.GenerateOnDays = new List<int>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbDays_");
                for (int i = 0; i < 7; i++)
                {
                    if (checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbDays_{0}", i)))
                    {
                        result.GenerateOnDays.Add(i);
                    }
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbRunOption_");
                for (int i = 0; i < 3; i++)
                {
                    if (checkBoxChecked(wb, string.Format("ctl00_ContentBody_rbRunOption_{0}", i)))
                    {
                        result.ExecutionScheme = i;
                        break;
                    }
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_tbResults");
                result.MaximumCaches = wb.Document.GetElementById("ctl00_ContentBody_tbResults").GetAttribute("value");
                result.GeocacheTypes = new List<int>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbTypeAny");
                if (!radioButtonChecked(wb, "ctl00_ContentBody_rbTypeAny"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbTaxonomy_");
                    for (int i = 0; i < 11; i++)
                    {
                        if (checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbTaxonomy_{0}", i)))
                        {
                            result.GeocacheTypes.Add(i);
                        }
                    }
                }
                result.GeocacheContainer = new List<int>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbContainerAny");
                if (!radioButtonChecked(wb, "ctl00_ContentBody_rbContainerAny"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbContainers_");
                    for (int i = 0; i < 7; i++)
                    {
                        if (checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbContainers_{0}", i)))
                        {
                            result.GeocacheContainer.Add(i);
                        }
                    }
                }
                result.WhichOptions = new List<int>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbOptions_");
                for (int i = 0; i < 14; i++)
                {
                    if (checkBoxChecked(wb, string.Format("ctl00_ContentBody_cbOptions_{0}", i)))
                    {
                        result.WhichOptions.Add(i);
                    }
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbDifficulty");
                if (checkBoxChecked(wb, "ctl00_ContentBody_cbDifficulty"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddDifficulty");
                    result.DifficultyCondition = wb.Document.GetElementById("ctl00_ContentBody_ddDifficulty").GetAttribute("value");
                }
                else
                {
                    result.DifficultyCondition = "";
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddDifficultyScore");
                result.DifficultyValue = wb.Document.GetElementById("ctl00_ContentBody_ddDifficultyScore").GetAttribute("value");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbTerrain");
                if (checkBoxChecked(wb, "ctl00_ContentBody_cbTerrain"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddTerrain");
                    result.TerrainCondition = wb.Document.GetElementById("ctl00_ContentBody_ddTerrain").GetAttribute("value");
                }
                else
                {
                    result.TerrainCondition = "";
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddTerrainScore");
                result.TerrainValue = wb.Document.GetElementById("ctl00_ContentBody_ddTerrainScore").GetAttribute("value");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbNone");
                if (radioButtonChecked(wb, "ctl00_ContentBody_rbNone"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbNone");
                    result.WithinSelection = "ctl00_ContentBody_rbNone";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbCountries"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbCountries");
                    result.WithinSelection = "ctl00_ContentBody_rbCountries";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbStates"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbStates");
                    result.WithinSelection = "ctl00_ContentBody_rbStates";
                }
                else
                {
                    result.WithinSelection = "";
                }
                result.Countries = new List<string>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_lbCountries");
                HtmlElement listel = wb.Document.GetElementById("ctl00_ContentBody_lbCountries");
                foreach (HtmlElement el in listel.Children)
                {
                    if (el.GetAttribute("selected") != null &&
                        (el.GetAttribute("selected").ToLower() == "selected" ||
                        el.GetAttribute("selected").ToLower() == "true"))
                    {
                        result.Countries.Add(el.GetAttribute("value"));
                    }
                }
                result.States = new List<string>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_lbStates");
                listel = wb.Document.GetElementById("ctl00_ContentBody_lbStates");
                foreach (HtmlElement el in listel.Children)
                {
                    if (el.GetAttribute("selected") != null &&
                        (el.GetAttribute("selected").ToLower() == "selected" ||
                        el.GetAttribute("selected").ToLower() == "true"))
                    {
                        result.States.Add(el.GetAttribute("value"));
                    }
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbOriginNone");
                if (radioButtonChecked(wb, "ctl00_ContentBody_rbOriginNone"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbOriginNone");
                    result.FromPointSelection = "ctl00_ContentBody_rbOriginNone";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbOriginHome"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbOriginHome");
                    result.FromPointSelection = "ctl00_ContentBody_rbOriginHome";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbOriginGC"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbOriginGC");
                    result.FromPointSelection = "ctl00_ContentBody_rbOriginGC";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbPostalCode"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbPostalCode");
                    result.FromPointSelection = "ctl00_ContentBody_rbPostalCode";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbOriginWpt"))
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbOriginWpt");
                    result.FromPointSelection = "ctl00_ContentBody_rbOriginWpt";
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_tbGC");
                result.FromPointGCCode = wb.Document.GetElementById("ctl00_ContentBody_tbGC").GetAttribute("value");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_tbPostalCode");
                result.FromPointPostalCode = wb.Document.GetElementById("ctl00_ContentBody_tbPostalCode").GetAttribute("value");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00$ContentBody$LatLong");
                result.LatLonFormatValue = GetElementByName(wb.Document.Body, "ctl00$ContentBody$LatLong").GetAttribute("value");
                result.NorthSouthValues = new List<string>();
                result.EastWestValues = new List<string>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong:_selectNorthSouth");
                result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong:_selectNorthSouth").GetAttribute("value"));
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong:_selectEastWest");
                result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong:_selectEastWest").GetAttribute("value"));
                if (result.LatLonFormatValue == "0")
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLatDegs");
                    result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatDegs").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLongDegs");
                    result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongDegs").GetAttribute("value"));                   
                }
                else if (result.LatLonFormatValue == "1")
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLatDegs");
                    result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatDegs").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLatMins");
                    result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatMins").GetAttribute("value"));

                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLongDegs");
                    result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongDegs").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLongMins");
                    result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongMins").GetAttribute("value"));
                }
                else if (result.LatLonFormatValue == "2")
                {
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLatDegs");
                    result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatDegs").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLatMins");
                    result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatMins").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLatSecs");
                    result.NorthSouthValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLatSecs").GetAttribute("value"));

                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLongDegs");
                    result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongDegs").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLongMins");
                    result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongMins").GetAttribute("value"));
                    core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_LatLong__inputLongSecs");
                    result.EastWestValues.Add(wb.Document.GetElementById("ctl00_ContentBody_LatLong__inputLongSecs").GetAttribute("value"));
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_tbRadius");
                result.WithinRadius = wb.Document.GetElementById("ctl00_ContentBody_tbRadius").GetAttribute("value");
                if (radioButtonChecked(wb, "ctl00_ContentBody_rbUnitType_0"))
                {
                    result.WithinRadiusUnitType = "ctl00_ContentBody_rbUnitType_0";
                }
                else
                {
                    result.WithinRadiusUnitType = "ctl00_ContentBody_rbUnitType_1";
                }

                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_rbPlacedNone");
                if (radioButtonChecked(wb, "ctl00_ContentBody_rbPlacedNone"))
                {
                    result.PlacedDuring = "ctl00_ContentBody_rbPlacedNone";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbPlacedLast"))
                {
                    result.PlacedDuring = "ctl00_ContentBody_rbPlacedLast";
                }
                else if (radioButtonChecked(wb, "ctl00_ContentBody_rbPlacedBetween"))
                {
                    result.PlacedDuring = "ctl00_ContentBody_rbPlacedBetween";
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddLastPlaced");
                result.PlacedDuringFixedInterval = wb.Document.GetElementById("ctl00_ContentBody_ddLastPlaced").GetAttribute("value");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_DateTimeBegin_Year");
                result.PlacedDuringFromDate = new DateTime(
                    int.Parse(wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Year").GetAttribute("value")),
                    int.Parse(wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Month").GetAttribute("value")),
                    int.Parse(wb.Document.GetElementById("ctl00_ContentBody_DateTimeBegin_Day").GetAttribute("value"))
                    );
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_DateTimeEnd_Year");
                result.PlacedDuringToDate = new DateTime(
                    int.Parse(wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Year").GetAttribute("value")),
                    int.Parse(wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Month").GetAttribute("value")),
                    int.Parse(wb.Document.GetElementById("ctl00_ContentBody_DateTimeEnd_Day").GetAttribute("value"))
                    );
                result.IncludeAttributes = new List<AttributeState>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ctlAttrInclude_dtlAttributeIcons_ctl{0}_hidInput");
                for (int i = 0; i < 70; i++)
                {
                    HtmlElement el = wb.Document.GetElementById(string.Format("ctl00_ContentBody_ctlAttrInclude_dtlAttributeIcons_ctl{0}_hidInput", i.ToString("00")));
                    if (el != null)
                    {
                        AttributeState ats = new AttributeState();
                        ats.ID = el.GetAttribute("AttID");
                        ats.Value = el.GetAttribute("value");
                        result.IncludeAttributes.Add(ats);
                    }
                }
                result.ExcludeAttributes = new List<AttributeState>();
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ctlAttrExclude_dtlAttributeIcons_ctl{0}_hidInput");
                for (int i = 0; i < 70; i++)
                {
                    HtmlElement el = wb.Document.GetElementById(string.Format("ctl00_ContentBody_ctlAttrExclude_dtlAttributeIcons_ctl{0}_hidInput", i.ToString("00")));
                    if (el != null)
                    {
                        AttributeState ats = new AttributeState();
                        ats.ID = el.GetAttribute("AttID");
                        ats.Value = el.GetAttribute("value");
                        result.ExcludeAttributes.Add(ats);
                    }
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddlAltEmails");
                try
                {
                    result.EMail = wb.Document.GetElementById("ctl00_ContentBody_ddlAltEmails").GetAttribute("value");
                }
                catch
                {
                    result.EMail = "";
                }
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbZip");
                result.CompressFile = checkBoxChecked(wb, "ctl00_ContentBody_cbZip");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_ddFormats");
                result.FileFormat = wb.Document.GetElementById("ctl00_ContentBody_ddFormats").GetAttribute("value");
                core.DebugLog(Framework.Data.DebugLogLevel.Info, plugin, null, "ctl00_ContentBody_cbIncludePQNameInFileName");
                result.PQNameInFileName = checkBoxChecked(wb, "ctl00_ContentBody_cbIncludePQNameInFileName");
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private static HtmlElement GetElementByName(HtmlElement root, string elementName)
        {
            HtmlElement result = null;
            if (root != null)
            {
                if (root.GetAttribute("name") != null && root.GetAttribute("name") == elementName)
                {
                    return root;
                }
                else
                {
                    if (root.Children != null)
                    {
                        foreach (HtmlElement el in root.Children)
                        {
                            result = GetElementByName(el, elementName);
                            if (result != null)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static bool checkBoxChecked(WebBrowser wb, string elementId)
        {
            bool result = false;
            if (wb.Document.GetElementById(elementId).GetAttribute("checked") != null &&
                (wb.Document.GetElementById(elementId).GetAttribute("checked").ToLower() == "checked" ||
                wb.Document.GetElementById(elementId).GetAttribute("checked").ToLower() == "true"))
            {
                result = true;
            }
            return result;
        }

        private static bool radioButtonChecked(WebBrowser wb, string elementId)
        {
            bool result = false;
            if (wb.Document.GetElementById(elementId).GetAttribute("checked") != null &&
                (wb.Document.GetElementById(elementId).GetAttribute("checked").ToLower() == "checked" ||
                wb.Document.GetElementById(elementId).GetAttribute("checked").ToLower() == "true"))
            {
                result = true;
            }
            return result;
        }
    }
}
