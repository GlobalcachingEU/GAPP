using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Favorites
{
    public class Manager
    {
        private static Manager _uniqueInstance = null;
        private static object _lockObject = new object();

        private Hashtable _favoritesCodes;

        private Manager()
        {
            _favoritesCodes = new Hashtable();
            try
            {
                if (!string.IsNullOrEmpty(Core.Settings.Default.FavoritesGCCodes))
                {
                    string[] parts = Core.Settings.Default.FavoritesGCCodes.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string s in parts)
                    {
                        _favoritesCodes.Add(s, true);
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        private void saveCodes()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string s in _favoritesCodes.Keys)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.FavoritesGCCodes = sb.ToString();
        }

        public static Manager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new Manager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public bool GeocacheFavorited(string gcCode)
        {
            return _favoritesCodes[gcCode] != null;
        }
        public void AddFavoritedGeocache(string gcCode)
        {
            if (!GeocacheFavorited(gcCode))
            {
                _favoritesCodes.Add(gcCode, true);
                saveCodes();
            }
        }
        public void AddFavoritedGeocaches(List<string> gcCodes)
        {
            foreach (string s in gcCodes)
            {
                if (!GeocacheFavorited(s))
                {
                    _favoritesCodes.Add(s, true);
                }
            }
            saveCodes();
        }
        public void RemoveFavoritedGeocache(string gcCode)
        {
            if (GeocacheFavorited(gcCode))
            {
                _favoritesCodes.Remove(gcCode);
                saveCodes();
            }
        }

    }
}
