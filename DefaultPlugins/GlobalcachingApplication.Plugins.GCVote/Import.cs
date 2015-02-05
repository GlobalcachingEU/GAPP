using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GCVote
{
    public class Import : Utils.BasePlugin.BaseImportFilter
    {
        public const string ACTION_IMPORT_ACTIVE = "Import GCVote|Active";
        public const string ACTION_IMPORT_SELECTED = "Import GCVote|Selected";
        public const string ACTION_IMPORT_ALL = "Import GCVote|All";

        public const string CUSTOM_ATTRIBUTE = "GCVote";

        public const string STR_IMPORT = "Importing GCVotes...";
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_ERRORDOWNLOADING = "Unable to get the data from GCVote.com";

        public List<Framework.Data.Geocache> _gcList = null;
        public string _errorMessage = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_IMPORT_ACTIVE);
            AddAction(ACTION_IMPORT_SELECTED);
            AddAction(ACTION_IMPORT_ALL);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERRORDOWNLOADING));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_LOADATSTARTUP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_PASSWORD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_USERNAME));

            Repository.Instance.Initialize(core);

            return await base.InitializeAsync(core);
        }

        public override string FriendlyName
        {
            get
            {
                return "GCVote";
            }
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();

            if (PluginSettings.Instance.ActivateAtAtartup)
            {
                await Activate();
            }
        }

        public async Task Activate()
        {
            await Repository.Instance.ActivateGCVote(this);
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT_ACTIVE ||
                    action == ACTION_IMPORT_SELECTED ||
                    action == ACTION_IMPORT_ALL)
                {
                    _gcList = null;
                    if (action == ACTION_IMPORT_ACTIVE)
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    else if (action == ACTION_IMPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else if (action == ACTION_IMPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache g in Core.Geocaches select g).ToList();
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        _errorMessage = null;
                        await Repository.Instance.ActivateGCVote(this);
                        await PerformImport();
                        if (!string.IsNullOrEmpty(_errorMessage))
                        {
                            System.Windows.Forms.MessageBox.Show(_errorMessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                }
            }
            return result;
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_IMPORT_ACTIVE;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(Core));
            return pnls;
        }


        protected override void ImportMethod()
        {
            try
            {
                int max = _gcList.Count;
                int pos = 0;
                int batch;
                StringBuilder wpList = new StringBuilder();
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_IMPORT, STR_IMPORT, max, pos, true))
                {
                    while (string.IsNullOrEmpty(_errorMessage) && _gcList.Count > 0)
                    {
                        wpList.Length = 0;
                        batch = 0;
                        while (batch < 200 && _gcList.Count > 0)
                        {
                            if (batch > 0)
                            {
                                wpList.Append(",");
                            }
                            wpList.Append(_gcList[0].Code);
                            _gcList.RemoveAt(0);
                            pos++;
                            batch++;
                        }
                        string usrName;
                        if (string.IsNullOrEmpty(PluginSettings.Instance.GCVoteUsername))
                        {
                            usrName = "uglyDUMMYusernamesolution";
                        }
                        else
                        {
                            usrName = PluginSettings.Instance.GCVoteUsername;
                        }
                        string postData = String.Format("version=3.1b&userName={0}&waypoints={1}&password={2}", HttpUtility.UrlEncode(PluginSettings.Instance.GCVoteUsername), wpList.ToString(), HttpUtility.UrlEncode(PluginSettings.Instance.GCVotePassword));
                        System.Net.WebRequest webRequest = System.Net.WebRequest.Create("http://gcvote.com/getVotes.php") as System.Net.HttpWebRequest;
                        webRequest.Method = "POST";
                        webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                        string doc;
                        using (System.IO.StreamWriter requestWriter = new System.IO.StreamWriter(webRequest.GetRequestStream()))
                        {
                            requestWriter.Write(postData);
                        }
                        using (System.IO.StreamReader responseReader = new System.IO.StreamReader(webRequest.GetResponse().GetResponseStream()))
                        {
                            // and read the response
                            doc = responseReader.ReadToEnd();
                        }
                        /*
                            <votes userName='...' currentVersion='2.0c' securityState='locked' loggedIn='true'>
                            <vote userName='...' cacheId='26984595-b3a1-4aa2-9638-7612a3bf3d5f' voteMedian='4' voteAvg='3.75' voteCnt='4' voteUser='0' waypoint='GC12RBN' vote1='0' vote2='1' vote3='0' vote4='2' vote5='1' rawVotes='(2.0:1)(4.0:2)(5.0:1)'/>
                            <vote userName='...' cacheId='55d02838-01f6-4181-a080-517a3339ad40' voteMedian='4.5' voteAvg='4.0555555555556' voteCnt='9' voteUser='0' waypoint='GC12YQJ' vote1='1' vote2='0' vote3='1' vote4='3' vote5='4' rawVotes='(1.0:1)(3.0:1)(4.0:2)(4.5:1)(5.0:4)'/>
                            <vote userName='...' cacheId='562829a6-a111-4ccb-a511-76370b8005d2' voteMedian='3' voteAvg='2.7777777777778' voteCnt='9' voteUser='0' waypoint='GC135AX' vote1='1' vote2='1' vote3='7' vote4='0' vote5='0' rawVotes='(1.0:1)(2.5:1)(3.0:6)(3.5:1)'/>

                            <errorstring></errorstring>
                            </votes>
                         */
                        if (doc != null)
                        {
                            try
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("<?xml version=\"1.0\"?>");
                                sb.Append(doc);

                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(sb.ToString());
                                XmlElement root = xmlDoc.DocumentElement;

                                XmlNodeList wpt = root.SelectNodes("vote");
                                if (wpt != null)
                                {
                                    foreach (XmlNode n in wpt)
                                    {
                                        double avg = Utils.Conversion.StringToDouble(n.Attributes["voteAvg"].Value);
                                        double median = Utils.Conversion.StringToDouble(n.Attributes["voteMedian"].Value);
                                        double usrVote = Utils.Conversion.StringToDouble(n.Attributes["voteUser"].Value);
                                        int cnt = int.Parse(n.Attributes["voteCnt"].Value);
                                        string wp = n.Attributes["waypoint"].Value;

                                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, wp);
                                        if (gc != null)
                                        {
                                            bool saved = gc.Saved;
                                            if (usrVote > 0.1)
                                            {
                                                gc.SetCustomAttribute(CUSTOM_ATTRIBUTE, string.Format("{0:0.0}/{1} ({2:0.0})", avg, cnt, usrVote));
                                            }
                                            else
                                            {
                                                gc.SetCustomAttribute(CUSTOM_ATTRIBUTE, string.Format("{0:0.0}/{1}", avg, cnt));
                                            }
                                            gc.Saved = saved;
                                        }
                                        Repository.Instance.StoreGCVote(wp, median, avg, cnt, usrVote);
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                _errorMessage = e.Message;
                            }
                        }
                        else
                        {
                            _errorMessage = Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOADING);
                        }
                        if (!prog.UpdateProgress(STR_IMPORT, STR_IMPORT, max, pos))
                        {
                            break;
                        }
                        if (_gcList.Count > 0)
                        {
                            //Thread.Sleep(500);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errorMessage = e.Message;
            }
        }
    }
}
