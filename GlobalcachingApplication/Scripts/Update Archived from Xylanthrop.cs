using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;

//Dialog with ListBox and ok+cancel
static class MyURLPrompt
    {       
        public class LbLink
        {
            private string myURL ;
            private string myLabel ;
        
            public LbLink(string strURL, string strLabel)
            {
                this.myURL = strURL;
                this.myLabel = strLabel;
            }

            public string Label
            {            
                get
                {
                    return this.myLabel;
                }
            }

            public override string ToString()
            {
                return this.myURL;
            }
        }
        
        public static void FillLbCounty(ListBox lb,string baseSite)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient()) {
                string html = wc.DownloadString(baseSite+"state_link_page.html");
                //Check if page is meant to contain archived caches
                Regex ar = new Regex(@"archived-GLX");
                if (ar.IsMatch(html)) {
                    //parse country/county name and link to glx, both terms separated by non-greedy match .*?
                    //previous version: @"cachelink[^>]+href[^>]+\.html[^>]+>([^<]*)<.*?<a[^>]+href=""([^""]*\.glx)""",
                    Regex rgc = new Regex(
                            @"<tr(?:\n|.)*?<td.*?CacheList.*?(?:\n|.)*?/td>(?:\n|.)*?<td[^>]*>([^<]*)</td>(?:\n|.)*?<a[^>]*cachelink[^>]+href[^>]+\.html[^>]+>(?:\n|.)*?<a[^>]+href=""([^""]*\.glx)"""
                            ,RegexOptions.Singleline);
                    MatchCollection mc = rgc.Matches(html);
                    
                    foreach (Match m in mc) {
                        //htmldecode translates html entities 
                        LbLink cl = new LbLink(m.Groups[2].Value,
                                            System.Net.WebUtility.HtmlDecode(m.Groups[1].Value));
                        lb.Items.Add(cl);
                    }
                    if (lb.Items.Count>0) {
                        lb.SelectedIndex=0;
                    }
                } else {
                    System.Windows.Forms.MessageBox.Show("Sorry: The directory of archived Caches on xylanthrop.de is of unknown format.");
                }
            }
        }
        
        public static string ShowDialog(string baseSite,string text, string caption)
        {
            Form prompt = new Form();
            string resultURL="";
            prompt.Width = 600;
            prompt.Height = 420;
            prompt.Text = caption;
            
            Label textLabel = new Label() { Left = 10, Top=10, Width=580, Text=text };
            ListBox lbCounty = new ListBox() { Left = 10, Top=40, Width=250, Height = 340, DisplayMember = "Label" };
            Button confirmation = new Button() { Text = "Ok", Left=370, Width=100, Top=350 };
            Button cancel = new Button() { Text = "Cancel", Left=480, Width=100, Top=350 };
            
            confirmation.Click += (sender, e) => {
                    if(lbCounty.SelectedItem!=null) {resultURL=lbCounty.SelectedItem.ToString();}
                    prompt.Close();
                };
            lbCounty.DoubleClick += (sender, e) => { confirmation.PerformClick();};
            cancel.Click += (sender, e) => { resultURL = "";prompt.Close(); };
            
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(lbCounty);
            
            FillLbCounty(lbCounty,baseSite);
            
            prompt.ShowDialog();
            return resultURL;
        }
    }

    
//Script which downloads+parses file and updates the caches status
class Script
{
    public static bool Run(Plugin plugin, ICore core)
    {
        string baseSite="http://xylanthrop.de/";
        string promptValue = MyURLPrompt.ShowDialog(baseSite,"2 Select a count(r)y to update the archived caches:", "Update Archived Caches from xylanthrop.de");
//        System.Windows.Forms.MessageBox.Show("promptValue: "+promptValue);
        if (promptValue!="") {
            //get page
            using (System.Net.WebClient wc = new System.Net.WebClient()) {
                string html = wc.DownloadString(baseSite+promptValue);
                int prog;
                int numgc=0;
                int updc=0;
                int foundc=0;
                int nonarc=0;//counter for caches "not" archived
                //parse GC-Codes from links
                Regex rgc = new Regex(@"<waypoint>(GC[A-Z0-9]*)<.*?<archived>([^<]*)<",RegexOptions.Singleline);
                MatchCollection mc = rgc.Matches(html);
                StringBuilder gcs = new StringBuilder();
                
                prog=0;
                using (ProgressBlock progress = new ProgressBlock(plugin,
                        "Retrieving GC-Codes", "", mc.Count, 0))
                {
                    foreach (Match m in mc) {
                        if (m.Groups[2].Value=="archived") {
                            gcs.Append(m.Groups[1].Value);
                            gcs.Append(" ");
                        } else {
                            nonarc++;
                        }
                        prog++;
                        if (prog % 10 == 0) {
                            progress.UpdateProgress("Retrieving GC-Codes", "", mc.Count, prog);
                        }

                    }
                    numgc=prog;
                }
                
                if (numgc>0) {
                    string gcstr=gcs.ToString();//searchable string of gc-codes
                    prog=0;
                    core.Geocaches.BeginUpdate();
                    
                    using (ProgressBlock progress = new ProgressBlock(plugin,
                        "Updating Caches", "", core.Geocaches.Count, 0))
                    {                                
                        foreach (Geocache gc in core.Geocaches)
                        {
                            if (gc.Code!="") {
                                if (gcstr.IndexOf(gc.Code+" ")>=0) {
                                    foundc++;
                                    if (!gc.Archived) {
                                        updc++;
                                        gc.Archived=true;//Set Cache to Archived state
                                        gc.Available=false;
                                    }
                                }
                            }
                            prog++;
                            if (prog % 100 == 0) {progress.UpdateProgress("Updating Caches", String.Format("Found: {0}",updc), core.Geocaches.Count, prog);}
                        }
                    }
                    core.Geocaches.EndUpdate();
                }
                System.Windows.Forms.MessageBox.Show(
                    "Update completed:\r"+
                    "Source URL: "+promptValue+"\r"+
                    String.Format("#Caches on page: {0}\r",numgc)+
                    String.Format("#Caches found in DB: {0}\r",foundc)+
                    String.Format("#Caches updated: {0}\r",updc)+
                    String.Format("#Caches non archived in src: {0}",nonarc));
                //System.Windows.Forms.MessageBox.Show("Val: "+html);
            }
        }
        //System.Windows.Forms.MessageBox.Show("Val: "+promptValue);
        return true;
    }
}
