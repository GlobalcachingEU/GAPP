using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace GAPPSF.GCVote
{
    public class Import
    {
        public async Task ImporGCVotesAsync(List<Core.Data.Geocache> gcList)
        {
            await Task.Run(new Action(() => ImporGCVotes(gcList)));
        }

        public void ImporGCVotes(List<Core.Data.Geocache> gcList)
        {
            try
            {
                int max = gcList.Count;
                int pos = 0;
                int batch;
                StringBuilder wpList = new StringBuilder();
                string usrName = null;

                var ai = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC");
                if (ai != null)
                {
                    usrName = ai.AccountName;
                }
                if (string.IsNullOrEmpty(usrName))
                {
                    usrName = "uglyDUMMYusernamesolution";
                }
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock("ImportingGCVotes", "ImportingGCVotes", max, pos, true))
                {
                    while (gcList.Count > 0)
                    {
                        wpList.Length = 0;
                        batch = 0;
                        while (batch < 200 && gcList.Count > 0)
                        {
                            if (batch > 0)
                            {
                                wpList.Append(",");
                            }
                            wpList.Append(gcList[0].Code);
                            gcList.RemoveAt(0);
                            pos++;
                            batch++;
                        }
                        string postData = String.Format("version=3.1b&userName={0}&waypoints={1}&password={2}", HttpUtility.UrlEncode(usrName), wpList.ToString(), ""); //"" => password: todo
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
                                        double? usrVote = Utils.Conversion.StringToDouble(n.Attributes["voteUser"].Value);
                                        int cnt = int.Parse(n.Attributes["voteCnt"].Value);
                                        string wp = n.Attributes["waypoint"].Value;
                                        if (usrVote <= 0.1)
                                        {
                                            usrVote = null;
                                        }

                                        Core.Settings.Default.SetGCVote(wp, median, avg, cnt, usrVote);
                                    }
                                }
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, "UnableToGetTheDataFromGCVoteCom");
                            break;
                        }
                        if (!prog.Update("ImportingGCVotes", max, pos))
                        {
                            break;
                        }
                        if (gcList.Count > 0)
                        {
                            //Thread.Sleep(500);
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
