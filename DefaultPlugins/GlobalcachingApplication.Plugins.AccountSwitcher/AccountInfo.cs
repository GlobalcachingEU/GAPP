using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public class AccountInfo
    {
        public string Name { get; set; }
        public Framework.Data.GeocachingComAccountInfo GeocachingComAccount { get; set; }
        public Framework.Data.InternalStorageDestination InternalStorageInfo { get; set; }
        public bool SwitchGeocachingComAccount { get; set; }
        public bool SwitchDatabase { get; set; }

        public override string ToString()
        {
            return Name ?? "";
        }

        public void SaveSettings(Framework.Interfaces.ICore core)
        {
            GeocachingComAccount = new Framework.Data.GeocachingComAccountInfo();
            GeocachingComAccount.AccountName = core.GeocachingComAccount.AccountName;
            GeocachingComAccount.APIToken = core.GeocachingComAccount.APIToken;
            GeocachingComAccount.MemberType = core.GeocachingComAccount.MemberType;
            GeocachingComAccount.MemberTypeId = core.GeocachingComAccount.MemberTypeId;
            Framework.Interfaces.IPluginInternalStorage storage = (from Framework.Interfaces.IPluginInternalStorage a in core.GetPlugin(Framework.PluginType.InternalStorage) select a).FirstOrDefault();
            InternalStorageInfo = storage.ActiveStorageDestination;
        }

        public void RestoreSettings(Framework.Interfaces.ICore core)
        {
            if (SwitchGeocachingComAccount && GeocachingComAccount != null)
            {
                core.GeocachingComAccount.AccountName = GeocachingComAccount.AccountName;
                core.GeocachingComAccount.APIToken = GeocachingComAccount.APIToken;
                core.GeocachingComAccount.MemberType = GeocachingComAccount.MemberType;
                core.GeocachingComAccount.MemberTypeId = GeocachingComAccount.MemberTypeId;
            }
            if (SwitchDatabase && InternalStorageInfo != null)
            {
                Framework.Interfaces.IPluginInternalStorage storage = (from Framework.Interfaces.IPluginInternalStorage a in core.GetPlugin(Framework.PluginType.InternalStorage) select a).FirstOrDefault();
                if (!InternalStorageInfo.SameDestination(storage.ActiveStorageDestination))
                {
                    storage.SetStorageDestination(InternalStorageInfo);
                }
            }
        }

        public static List<AccountInfo> GetAccountInfos(string filename)
        {
            List<AccountInfo> result = new List<AccountInfo>();
            try
            {
                if (System.IO.File.Exists(filename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("account");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            AccountInfo bm = new AccountInfo();
                            bm.Name = n.SelectSingleNode("Name").InnerText;
                            bm.SwitchGeocachingComAccount = bool.Parse(n.SelectSingleNode("SwitchGeocachingComAccount").InnerText);
                            bm.SwitchDatabase = bool.Parse(n.SelectSingleNode("SwitchDatabase").InnerText);

                            XmlNode gca = n.SelectSingleNode("GeocachingComAccount");
                            if (gca != null)
                            {
                                bm.GeocachingComAccount = new Framework.Data.GeocachingComAccountInfo();
                                bm.GeocachingComAccount.AccountName = gca.SelectSingleNode("AccountName").InnerText;
                                bm.GeocachingComAccount.APIToken = gca.SelectSingleNode("APIToken").InnerText;
                                bm.GeocachingComAccount.MemberType = gca.SelectSingleNode("MemberType").InnerText;
                                bm.GeocachingComAccount.MemberTypeId = int.Parse(gca.SelectSingleNode("MemberTypeId").InnerText);
                            }

                            gca = n.SelectSingleNode("InternalStorageInfo");
                            if (gca != null)
                            {
                                bm.InternalStorageInfo = new Framework.Data.InternalStorageDestination();
                                bm.InternalStorageInfo.Name = gca.SelectSingleNode("Name").InnerText;
                                bm.InternalStorageInfo.PluginType = gca.SelectSingleNode("PluginType").InnerText;

                                XmlNodeList subs = gca.SelectNodes("StorageInfo");
                                bm.InternalStorageInfo.StorageInfo = new string[subs.Count];
                                for (int i=0; i<subs.Count; i++)
                                {
                                    bm.InternalStorageInfo.StorageInfo[i] = subs[i].InnerText;
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

        public static void SetAccountInfos(string filename, List<AccountInfo> ail)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("accounts");
                doc.AppendChild(root);
                foreach (AccountInfo bmi in ail)
                {
                    XmlElement bm = doc.CreateElement("account");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(bmi.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("SwitchGeocachingComAccount");
                    txt = doc.CreateTextNode(bmi.SwitchGeocachingComAccount.ToString());
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("SwitchDatabase");
                    txt = doc.CreateTextNode(bmi.SwitchDatabase.ToString());
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    if (bmi.GeocachingComAccount != null)
                    {
                        el = doc.CreateElement("GeocachingComAccount");
                        bm.AppendChild(el);

                        XmlElement subel = doc.CreateElement("AccountName");
                        txt = doc.CreateTextNode(bmi.GeocachingComAccount.AccountName??"");
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                        subel = doc.CreateElement("APIToken");
                        txt = doc.CreateTextNode(bmi.GeocachingComAccount.APIToken??"");
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                        subel = doc.CreateElement("MemberType");
                        txt = doc.CreateTextNode(bmi.GeocachingComAccount.MemberType ?? "");
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                        subel = doc.CreateElement("MemberTypeId");
                        txt = doc.CreateTextNode(bmi.GeocachingComAccount.MemberTypeId.ToString());
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                    }

                    if (bmi.InternalStorageInfo != null)
                    {
                        el = doc.CreateElement("InternalStorageInfo");
                        bm.AppendChild(el);

                        XmlElement subel = doc.CreateElement("Name");
                        txt = doc.CreateTextNode(bmi.InternalStorageInfo.Name ?? "");
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                        subel = doc.CreateElement("PluginType");
                        txt = doc.CreateTextNode(bmi.InternalStorageInfo.PluginType ?? "");
                        subel.AppendChild(txt);
                        el.AppendChild(subel);

                        subel = doc.CreateElement("StorageInfo");
                        el.AppendChild(subel);

                        if (bmi.InternalStorageInfo.StorageInfo != null)
                        {
                            foreach (string s in bmi.InternalStorageInfo.StorageInfo)
                            {
                                XmlElement subel2 = doc.CreateElement("info");
                                txt = doc.CreateTextNode(s?? "");
                                subel2.AppendChild(txt);
                                subel.AppendChild(subel2);
                            }
                        }
                    }
                }
                using (TextWriter sw = new StreamWriter(filename, false, Encoding.UTF8)) //Set encoding
                {
                    doc.Save(sw);
                }
            }
            catch
            {
            }
        }
    }
}
