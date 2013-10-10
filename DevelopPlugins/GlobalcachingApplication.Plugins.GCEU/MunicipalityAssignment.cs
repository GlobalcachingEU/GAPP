using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Data.SqlClient;

namespace GlobalcachingApplication.Plugins.GCEU
{
    public class MunicipalityAssignment : Utils.BasePlugin.Plugin
    {
        public const string ACTION_ASSIGNMANUCIPALITY = "Globalcaching.eu - Assign municipality";
        private AutoResetEvent _actionReady = null;

        public class GeocacheInfo
        {
            public string Code { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_ASSIGNMANUCIPALITY);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            return base.Initialize(core);
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            SettingsPanel sp = new SettingsPanel();
            sp.textBox1.Text = Properties.Settings.Default.ConnectionString;
            pnls.Add(sp);
            return pnls;
        }

        public override bool ApplySettings(List<UserControl> configPanels)
        {
            SettingsPanel sp = (from s in configPanels where s is SettingsPanel select s as SettingsPanel).FirstOrDefault();
            if (sp!=null)
            {
                Properties.Settings.Default.ConnectionString = sp.textBox1.Text;
                Properties.Settings.Default.Save();
            }
            return true;
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
                if (action == ACTION_ASSIGNMANUCIPALITY)
                {
                    _actionReady = new AutoResetEvent(false);
                    Thread thrd = new Thread(new ThreadStart(this.assignMunicipalityThreadMethod));
                    thrd.Start();
                    while (!_actionReady.WaitOne(100))
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    thrd.Join();
                    _actionReady.Dispose();
                    _actionReady = null;
                }
            }
            return result;
        }

        private void assignMunicipalityThreadMethod()
        {
            try
            {
                using (DBConSqlServer dbcon = new DBConSqlServer())
                {
                    using (Utils.ProgressBlock upd = new Utils.ProgressBlock(this, "Updating municipalty for geocaches...", "Updating municipalty for geocaches...", 1, 0))
                    {
                        bool goon = true;
                        List<GeocacheInfo> gcInfos = new List<GeocacheInfo>();
                        using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, "Getting available geocaches...", "Getting available geocaches...", 1, 0, true))
                        {
                            SqlDataReader dr = dbcon.ExecuteReader("select Waypoint, Lat, Lon from Caches where Country='Netherlands' and Archived=0");
                            while (dr.Read())
                            {
                                GeocacheInfo gi = new GeocacheInfo();
                                gi.Code = (string)dr["Waypoint"];
                                gi.Lat = (double)dr["Lat"];
                                gi.Lon = (double)dr["Lon"];
                                gcInfos.Add(gi);
                            }
                            goon = prog.UpdateProgress("Getting available geocaches...", "Getting available geocaches...", 1, 0);
                        }
                        if (goon)
                        {
                            Framework.Data.Location l = new Framework.Data.Location();
                            List<Framework.Data.AreaInfo> municipalities = Utils.GeometrySupport.Instance.GetAreasByLevel(Framework.Data.AreaType.Municipality);
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, "Assigning municipalities geocaches...", "Assigning municipalities geocaches...", gcInfos.Count, 0, true))
                            {
                                for (int i = 0; i < gcInfos.Count; i++)
                                {
                                    l.SetLocation(gcInfos[i].Lat, gcInfos[i].Lon);
                                    List<Framework.Data.AreaInfo> ais = Utils.GeometrySupport.Instance.GetAreasOfLocation(l, municipalities);
                                    if (ais.Count > 0)
                                    {
                                        dbcon.ExecuteNonQuery(string.Format("update Caches set County='{1}' where Waypoint='{0}'", gcInfos[i].Code, ais[0].Name.Replace("'", "''")));
                                    }
                                    else
                                    {
                                        dbcon.ExecuteNonQuery(string.Format("update Caches set County='' where Waypoint='{0}'", gcInfos[i].Code));
                                    }

                                    if (!prog.UpdateProgress("Assigning municipalities geocaches...", "Assigning municipalities geocaches...", gcInfos.Count, i))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
    }
}
