using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.GCEU
{
    public class SelectMissing : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SELECTMISSING = "Globalcaching.eu - select missing";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_SELECTMISSING);

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SELECTMISSING)
                {
                    using (Utils.FrameworkDataUpdater updater = new Utils.FrameworkDataUpdater(Core))
                    {
                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        {
                            List<string> gcList = new List<string>();

                            string doc = wc.DownloadString("http://www.globalcaching.eu/Service/GeocacheCodes.aspx?country=Netherlands");
                            if (doc != null)
                            {
                                string[] lines = doc.Replace("\r", "").Split(new char[] { '\n' });
                                char[] sep = new char[] { ',' };
                                string[] parts;
                                foreach (string s in lines)
                                {
                                    parts = s.Split(sep);
                                    if (parts.Length > 2)
                                    {
                                        gcList.Add(parts[0]);
                                    }
                                }
                            }

                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = gc.Country=="Netherlands" && !gcList.Contains(gc.Code);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
