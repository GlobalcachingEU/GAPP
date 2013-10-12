using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteManager
    {
        private static SiteManager _uniqueInstance = null;
        private static object _lockObject = new object();

        public List<SiteInfo> AvailableSites { get; private set; }
        public SiteInfo _activeSite = null;

        private SiteManager()
        {
            AvailableSites = new List<SiteInfo>();
            AvailableSites.Add(new SiteInfoGermany());
            AvailableSites.Add(new SiteInfoNetherlands());

            foreach (SiteInfo si in AvailableSites)
            {
                si.LoadSettings();
            }
            _activeSite = (from a in AvailableSites where a.ID == Properties.Settings.Default.ActiveSiteID select a).FirstOrDefault();
        }

        public static SiteManager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new SiteManager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public SiteInfo ActiveSite
        {
            get { return _activeSite; }
            set
            {
                _activeSite = value;
                if (_activeSite != null)
                {
                    Properties.Settings.Default.ActiveSiteID = _activeSite.ID;
                }
                else
                {
                    Properties.Settings.Default.ActiveSiteID = "";
                }
                Properties.Settings.Default.Save();
            }
        }
    }
}
