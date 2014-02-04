using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Munzee
{
    public class Import
    {
        [Serializable]
        public class MunzeeData
        {
            public string latitude;
            public string longitude;
            public string created_at;
            public string deployed_at;
            public string notes;
            public string friendly_name;
            public string location;
            public string username;
            public string code;
            public string capture_type_id;
            public string special_logo;
            public string updated;
            public string archived;
            public string type;
            public string country;
            public string region;
            public string country_2;
            public string region_2;
            public string munzee_id;
            public string owned;
            public string captured;
            public string captured_at;
            public string special;
        }
        [Serializable]
        public class MunzeeDataList : List<MunzeeData>
        {
        }

        public async Task ImportMunzeesFromDfxAtAsync(Core.Storage.Database db, string url)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(new Action(() => ImportMunzeesFromDfxAt(db, url)));
            }
        }

        public void ImportMunzeesFromDfxAt(Core.Storage.Database db, string url)
        {
            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock("ImportingMunzees", "DownloadingData", 2, 0))
            {
                int index = 0;
                MunzeeDataList munzl;
                try
                {
                    System.Net.HttpWebRequest webRequest;
                    webRequest = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
                    webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.17 Safari/533.4";
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MunzeeDataList));
                    using (System.IO.Stream stream = webRequest.GetResponse().GetResponseStream())
                    {
                        munzl = ser.ReadObject(stream) as MunzeeDataList;
                    }
                    fixpr.Update("DownloadingData", 2, 1);
                    DateTime nextProgUpdate = DateTime.MinValue;
                    string usrname = (Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("MZ").AccountName ?? "").ToLower();
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("SavingGeocaches", "SavingGeocaches", munzl.Count, 0))
                    {
                        foreach (MunzeeData md in munzl)
                        {
                            string code = string.Format("MZ{0}", int.Parse(md.munzee_id).ToString("X4"));
                            Core.Data.IGeocacheData gc = db.GeocacheCollection.GetGeocache(code);
                            if (gc==null)
                            {
                                gc = new Core.Data.GeocacheData();
                            }

                            gc.Archived = md.archived == "1";
                            gc.Available = !gc.Archived;
                            //gc.City = md.location.Split(new char[]{','})[0];
                            gc.City = "";
                            gc.Code = code;
                            gc.Container = Utils.DataAccess.GetGeocacheContainer(1);
                            gc.Country = md.country;
                            gc.DataFromDate = DateTime.Now;
                            gc.Difficulty = 1.0;
                            gc.Found = md.captured != "0";
                            if (md.type == "")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95342);
                            }
                            else if (md.type == "virtual")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95343);
                            }
                            else if (md.type == "maintenance")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95344);
                            }
                            else if (md.type == "business")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95345);
                            }
                            else if (md.type == "mystery")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95346);
                            }
                            else if (md.type == "nfc")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95347);
                            }
                            else if (md.type == "premium")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95348);
                            }
                            else
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(95342);
                            }
                            gc.Lat = Utils.Conversion.StringToDouble(md.latitude);
                            gc.Lon = Utils.Conversion.StringToDouble(md.longitude);
                            gc.LongDescription = md.notes == null ? "" : md.notes.Replace("\r", "").Replace("\n", "\r\n");
                            gc.LongDescriptionInHtml = false;
                            gc.MemberOnly = false;
                            gc.Municipality = "";
                            gc.Name = md.friendly_name;
                            gc.Owner = md.username;
                            gc.OwnerId = "";
                            gc.PlacedBy = md.username;
                            try
                            {
                                gc.PublishedTime = DateTime.ParseExact(md.deployed_at, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                gc.PublishedTime = DateTime.ParseExact(md.created_at, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            gc.ShortDescription = "";
                            gc.ShortDescriptionInHtml = false;
                            gc.State = "";
                            gc.Terrain = 1.0;
                            gc.Url = string.Format("http://www.munzee.com/m/{0}/{1}/", md.username, md.code);

                            bool gcAdded = true;
                            if (gc is Core.Data.GeocacheData)
                            {
                                gcAdded = Utils.DataAccess.AddGeocache(Core.ApplicationData.Instance.ActiveDatabase, gc as Core.Data.GeocacheData);
                            }
                            if (gcAdded)
                            {
                                gc = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Code == code select a).FirstOrDefault();
                                Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc as Core.Data.Geocache, Core.ApplicationData.Instance.CenterLocation);

                                //check if found and if so, if log present
                                if (gc.Found && !string.IsNullOrEmpty(md.captured_at))
                                {
                                    //foud and no log, add log
                                    bool foundLogPresent = false;
                                    List<Core.Data.Log> lgs = db.LogCollection.GetLogs(gc.Code);
                                    if (lgs != null)
                                    {
                                        Core.Data.Log l = (from Core.Data.Log lg in lgs where lg.LogType.AsFound && string.Compare(usrname, lg.Finder, true)==0 select lg).FirstOrDefault();
                                        foundLogPresent = (l != null);
                                    }
                                    if (!foundLogPresent)
                                    {
                                        Core.Data.LogData l = new Core.Data.LogData();

                                        l.DataFromDate = DateTime.Now;
                                        l.Date = new DateTime(1970, 1, 1);
                                        l.Date = l.Date.AddSeconds(long.Parse(md.captured_at.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0]));
                                        l.Encoded = false;
                                        l.Finder = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("MZ").AccountName ?? "";
                                        l.FinderId = "0";
                                        l.GeocacheCode = gc.Code;
                                        l.ID = string.Format("ML{0}", gc.Code.Substring(2));
                                        l.LogType = Utils.DataAccess.GetLogType(2);
                                        l.Text = "Captured";

                                        Utils.DataAccess.AddLog(db, l);
                                    }
                                }
                            }

                            index++;
                            if (DateTime.Now >= nextProgUpdate)
                            {
                                prog.Update("SavingGeocaches", munzl.Count, index);
                                nextProgUpdate = DateTime.Now.AddSeconds(0.5);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            }
        }
    }
}
