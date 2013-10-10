using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace GlobalcachingApplication.Plugins.Munzee
{
    public class MunzeeDfxAt : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_ERROR = "Error";
        public const string STR_IMPORTING = "Importing Munzees...";
        public const string STR_DOWNLOADINGDATA = "Downloading data...";

        public const string ACTION_IMPORT = "Import Munzees from munzee.dfx.at";

        private string _selectedUrl = null;
        private string _errorMessage = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DOWNLOADINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_COMMENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_INFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_MUNZEECOMACCOUNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_SELECTURL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_URL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MunzeeDfxAtForm.STR_URLS));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            return base.Initialize(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    using (MunzeeDfxAtForm dlg = new MunzeeDfxAtForm(Core))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _selectedUrl = dlg.SelectedUrl;
                            PerformImport();
                            if (!string.IsNullOrEmpty(_errorMessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errorMessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                            }
                        }
                    }
                }
            }
            return result;
        }


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
        public class MunzeeDataList: List<MunzeeData>
        {
        }

        protected override void ImportMethod()
        {
            /*
             * [{"latitude":"52.383607",
             * "longitude":"4.641646",
             * "created_at":"2012-05-15 07:02:53",
             * "deployed_at":"2013-03-16 05:15:56",
             * "notes":"hint: bankje",
             * "friendly_name":"NN# 147. Molen de Adriaan",
             * "location":",  ",
             * "username":"no-name",
             * "code":"147",
             * "capture_type_id":"http:\/\/www.munzee.com\/images\/pins\/munzee.png",
             * "special_logo":"",
             * "updated":"1365761293",
             * "archived":"0",
             * "type":"",
             * "country":"The Netherlands",
             * "region":"",
             * "country_2":null,
             * "region_2":null,
             * "munzee_id":"233744",
             * "owned":"0",
             * "captured":"0",
             * "captured_at":null,
             * "special":"0"}
             * 
             * 
             * 
             * {"latitude":"52.234168",
             * "longitude":"5.185743",
             * "created_at":"2012-11-17 15:41:03",
             * "deployed_at":"2012-11-18 06:06:29",
             * "notes":"Hint: geel\n\nHeilig Hartkerk - Heilig Joseph kerk \nDeze kerk werd in 1928 ontworpen door H.W. Valk, die behoorde tot de traditionalistische stroming in de architectuur. Het gebouw kenmerkt zich door zijn robuuste bakstenen bouw en de combinatie van gotische en romaanse stijlelementen.\n\nHet was de eerste rooms-katholieke kerk in de arbeiderswijk Over het Spoor. Dat leidde in het begin nog wel eens tot uitingen van ongenoegen zoals het bekladden van de muren. Het interieur is sober. De imposante apostelkoppen van J. Cantre vormen de belangrijkste decoraties. Hij ontwierp ook het H. Hartbeeld aan de achterzijde. De gebrandschilderde ramen zijn van J. Nicolas.",
             * "friendly_name":"Heilig Hartkerk",
             * "location":"Hilversum, North Holland The Netherlands",
             * "username":"ass3755",
             * "code":"65",
             * "capture_type_id":"http:\/\/www.munzee.com\/images\/pins\/munzee.png",
             * "special_logo":"",
             * "updated":"1365761293",
             * "archived":"0",
             * "type":"", //virtual
             * "country":"The Netherlands",
             * "region":"",
             * "country_2":null,
             * "region_2":null,
             * "munzee_id":"486901",
             * "owned":"0",
             * "captured":"0", //"captured":"1"
             * "captured_at":null, //"captured_at":"1364119020"
             * "special":"0"}
             * */

            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock(this, STR_IMPORTING, STR_DOWNLOADINGDATA, 1, 0))
            {
                int index = 0;
                MunzeeDataList munzl;
                try
                {
                    System.Net.HttpWebRequest webRequest;
                    webRequest = System.Net.WebRequest.Create(_selectedUrl) as System.Net.HttpWebRequest;
                    webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.17 Safari/533.4";
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MunzeeDataList));
                    using (System.IO.Stream stream = webRequest.GetResponse().GetResponseStream())
                    {
                        munzl = ser.ReadObject(stream) as MunzeeDataList;
                    }
                    DateTime nextProgUpdate = DateTime.MinValue;
                    string usrname = Properties.Settings.Default.AccountName.ToLower();
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTING, munzl.Count, 0))
                    { 
                        foreach (MunzeeData md in munzl)
                        {
                            Framework.Data.Geocache gc = new Framework.Data.Geocache();

                            gc.Archived = md.archived == "1";
                            gc.Available = !gc.Archived;
                            //gc.City = md.location.Split(new char[]{','})[0];
                            gc.City = "";
                            gc.Code = string.Format("MZ{0}", int.Parse(md.munzee_id).ToString("X4"));
                            gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, 1);
                            gc.Country = md.country;
                            gc.DataFromDate = DateTime.Now;
                            gc.Difficulty = 1.0;
                            gc.Found = md.captured != "0";
                            if (md.type == "")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95342);
                            }
                            else if (md.type == "virtual")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95343);
                            }
                            else if (md.type == "maintenance")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95344);
                            }
                            else if (md.type == "business")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95345);
                            }
                            else if (md.type == "mystery")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95346);
                            }
                            else if (md.type == "nfc")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95347);
                            }
                            else if (md.type == "premium")
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95348);
                            }
                            else
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 95342);
                            }
                            gc.ID = md.munzee_id;
                            gc.Lat = Utils.Conversion.StringToDouble(md.latitude);
                            gc.Lon = Utils.Conversion.StringToDouble(md.longitude);
                            gc.LongDescription = md.notes == null ? "" : md.notes.Replace("\r", "").Replace("\n", "\r\n");
                            gc.LongDescriptionInHtml = false;
                            gc.MemberOnly = false;
                            gc.Municipality = "";
                            gc.Name = md.friendly_name;
                            if (md.owned=="1" || md.username.ToLower() == usrname)
                            {
                                gc.Owner = Core.GeocachingComAccount.AccountName;
                            }
                            else
                            {
                                gc.Owner = md.username;
                            }
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
                            gc.Title = gc.Name;
                            gc.Url = string.Format("http://www.munzee.com/m/{0}/{1}/", md.username, md.code);

                            Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);

                            if (AddGeocache(gc, null))
                            {
                                //check if found and if so, if log present
                                if (gc.Found && !string.IsNullOrEmpty(md.captured_at))
                                {
                                    //foud and no log, add log
                                    bool foundLogPresent = false;
                                    List<Framework.Data.Log> lgs = Core.Logs.GetLogs(gc.Code);
                                    if (lgs != null)
                                    {
                                        Framework.Data.Log l = (from Framework.Data.Log lg in lgs where lg.LogType.AsFound && lg.Finder == Core.GeocachingComAccount.AccountName select lg).FirstOrDefault();
                                        foundLogPresent = (l != null);
                                    }
                                    if (!foundLogPresent)
                                    {
                                        Framework.Data.Log l = new Framework.Data.Log();
                                        l.DataFromDate = DateTime.Now;
                                        l.Date = new DateTime(1970, 1, 1);
                                        l.Date = l.Date.AddSeconds(long.Parse(md.captured_at));
                                        l.Encoded = false;
                                        l.Finder = Core.GeocachingComAccount.AccountName;
                                        l.FinderId = "0";
                                        l.GeocacheCode = gc.Code;
                                        l.ID = string.Format("ML{0}", gc.Code.Substring(2));
                                        l.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, 2);
                                        l.Text = "Captured";
                                        AddLog(l);
                                    }
                                }
                            }

                            index++;
                            if (DateTime.Now >= nextProgUpdate)
                            {
                                prog.UpdateProgress(STR_IMPORTING, STR_IMPORTING, munzl.Count, index);
                                nextProgUpdate = DateTime.Now.AddSeconds(0.5);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    _errorMessage = e.Message;
                }
            }
        }
    }
}
