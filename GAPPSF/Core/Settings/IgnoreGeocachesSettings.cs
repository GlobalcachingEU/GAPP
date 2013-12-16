using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int IgnoreGeocachesWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int IgnoreGeocachesWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int IgnoreGeocachesWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int IgnoreGeocachesWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public int IgnoreGeocachesUpdateCounter
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

        public List<string> filterIgnoredGeocacheCodes(List<string> codes)
        {
            int i = 0;
            while (i < codes.Count)
            {
                if (_ignoredGeocacheCodes[codes[i]] != null)
                {
                    codes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            return codes;
        }
        public bool GeocacheCodeIgnored(string code)
        {
            return (_ignoredGeocacheCodes[code] != null);
        }
        public bool GeocacheIgnored(Core.Data.IGeocacheData gcData)
        {
            bool result = (_ignoredGeocacheCodes[gcData.Code] != null || geocacheOwnerIgnored(gcData.Owner ?? "") || geocacheNameIgnored(gcData.Name ?? ""));
            if (result)
            {
                if (_ignoredGeocacheCodes[gcData.Code] == null)
                {
                    DeleteIgnoreGeocacheCodes(new string[] { gcData.Code }.ToList());
                }
            }
            return result;
        }
        private bool geocacheOwnerIgnored(string owner)
        {
            return _ignoredGeocacheOwners.Contains(owner.ToLower());
        }
        private bool geocacheNameIgnored(string name)
        {
            string n = name.ToLower();
            return (from a in _ignoredGeocacheNames where a.IndexOf(n)>=0 select a).FirstOrDefault()!=null;
        }

        public List<string> IgnoredGeocacheCodes
        {
            get { return (from string a in _ignoredGeocacheCodes.Keys select a).ToList(); }
        }
        public List<string> IgnoredGeocacheNames
        {
            get { return (from string a in _ignoredGeocacheNames select a).ToList(); }
        }
        public List<string> IgnoredGeocacheOwners
        {
            get { return (from string a in _ignoredGeocacheOwners select a).ToList(); }
        }

        public void ClearGeocacheIgnoreFilters()
        {
            _settingsStorage.ClearGeocacheIgnoreFilters();
            _ignoredGeocacheCodes.Clear();
            _ignoredGeocacheNames.Clear();
            _ignoredGeocacheOwners.Clear();
            IgnoreGeocachesUpdateCounter++;
        }
        public void AddIgnoreGeocacheCodes(List<string> codes)
        {
            foreach(string s in codes)
            {
                string u = s.ToUpper();
                _settingsStorage.AddIgnoreGeocacheCode(u);
                _ignoredGeocacheCodes[u] = true;
            }
            IgnoreGeocachesUpdateCounter++;
        }
        public void AddIgnoreGeocacheNames(List<string> names)
        {
            foreach (string s in names)
            {
                string u = s.ToLower();
                if (!_ignoredGeocacheNames.Contains(u))
                {
                    _settingsStorage.AddIgnoreGeocacheName(u);
                    _ignoredGeocacheNames.Add(u);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }

        public void AddIgnoreGeocacheOwners(List<string> owners)
        {
            foreach (string s in owners)
            {
                string u = s.ToLower();
                if (!_ignoredGeocacheOwners.Contains(u))
                {
                    _settingsStorage.AddIgnoreGeocacheOwner(u);
                    _ignoredGeocacheOwners.Add(u);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }
        public void DeleteIgnoreGeocacheCodes(List<string> codes)
        {
            foreach (string s in codes)
            {
                string u = s.ToUpper();
                if (_ignoredGeocacheCodes[u] != null)
                {
                    _settingsStorage.DeleteIgnoreGeocacheCode(u);
                    _ignoredGeocacheCodes.Remove(u);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }
        public void DeleteIgnoreGeocacheNames(List<string> names)
        {
            foreach (string s in names)
            {
                string u = s.ToLower();
                if (_ignoredGeocacheNames.Contains(u))
                {
                    _settingsStorage.DeleteIgnoreGeocacheName(u);
                    _ignoredGeocacheNames.Remove(u);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }

        public void DeleteIgnoreGeocacheOwners(List<string> owners)
        {
            foreach (string s in owners)
            {
                string u = s.ToLower();
                if (_ignoredGeocacheOwners.Contains(u))
                {
                    _settingsStorage.DeleteIgnoreGeocacheOwner(u);
                    _ignoredGeocacheOwners.Remove(u);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }

    }
}
