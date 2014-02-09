using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GAPPSF.OKAPI
{
    public class SiteManager : INotifyPropertyChanged
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
            AvailableSites.Add(new SiteInfoPoland());
            //AvailableSites.Add(new SiteInfoUSA());
            //AvailableSites.Add(new SiteInfoUK());

            foreach (SiteInfo si in AvailableSites)
            {
                si.LoadSettings();
            }
            _activeSite = (from a in AvailableSites where a.ID == Core.Settings.Default.OKAPIActiveSiteID select a).FirstOrDefault();
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
                SetProperty(ref _activeSite, value);
                if (_activeSite != null)
                {
                    Core.Settings.Default.OKAPIActiveSiteID = _activeSite.ID;
                }
                else
                {
                    Core.Settings.Default.OKAPIActiveSiteID = "";
                }
            }
        }

        public bool CheckAPIAccess()
        {
            bool result = false;
            if (ActiveSite == null)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, Localization.TranslationManager.Instance.Translate("SelectOpencachingSite") as string);
            }
            else if (string.IsNullOrEmpty(ActiveSite.UserID))
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, Localization.TranslationManager.Instance.Translate("ObtainUserID") as string);
            }
            else
            {
                result = true;
            }
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
