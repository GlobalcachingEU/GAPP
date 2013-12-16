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
            bool result = (_ignoredGeocacheCodes[gcData.Code] != null || _ignoredGeocacheOwners[gcData.Owner ?? ""] != null || _ignoredGeocacheNames[gcData.Name ?? ""] != null);
            if (result)
            {
                if (_ignoredGeocacheCodes[gcData.Code] == null)
                {
                    DeleteIgnoreGeocacheCodes(new string[] { gcData.Code }.ToList());
                }
            }
            return result;
        }

        public List<string> IgnoredGeocacheCodes
        {
            get { return (from string a in _ignoredGeocacheCodes.Keys select a).ToList(); }
        }
        public List<string> IgnoredGeocacheNames
        {
            get { return (from string a in _ignoredGeocacheNames.Keys select a).ToList(); }
        }
        public List<string> IgnoredGeocacheOwners
        {
            get { return (from string a in _ignoredGeocacheOwners.Keys select a).ToList(); }
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
            _settingsStorage.AddIgnoreGeocacheCodes(codes);
            foreach(string s in codes)
            {
                _ignoredGeocacheCodes[s] = true;
            }
            IgnoreGeocachesUpdateCounter++;
        }
        public void AddIgnoreGeocacheNames(List<string> names)
        {
            _settingsStorage.AddIgnoreGeocacheNames(names);
            foreach (string s in names)
            {
                _ignoredGeocacheNames[s] = true;
            }
            IgnoreGeocachesUpdateCounter++;
        }

        public void AddIgnoreGeocacheOwners(List<string> owners)
        {
            _settingsStorage.AddIgnoreGeocacheOwners(owners);
            foreach (string s in owners)
            {
                _ignoredGeocacheOwners[s] = true;
            }
            IgnoreGeocachesUpdateCounter++;
        }
        public void DeleteIgnoreGeocacheCodes(List<string> codes)
        {
            _settingsStorage.DeleteIgnoreGeocacheCodes(codes);
            foreach (string s in codes)
            {
                if (_ignoredGeocacheCodes[s] !=null)
                {
                    _ignoredGeocacheCodes.Remove(s);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }
        public void DeleteIgnoreGeocacheNames(List<string> names)
        {
            _settingsStorage.DeleteIgnoreGeocacheNames(names);
            foreach (string s in names)
            {
                if (_ignoredGeocacheNames[s] != null)
                {
                    _ignoredGeocacheNames.Remove(s);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }

        public void DeleteIgnoreGeocacheOwners(List<string> owners)
        {
            _settingsStorage.DeleteIgnoreGeocacheOwners(owners);
            foreach (string s in owners)
            {
                if (_ignoredGeocacheOwners[s] != null)
                {
                    _ignoredGeocacheOwners.Remove(s);
                }
            }
            IgnoreGeocachesUpdateCounter++;
        }

    }
}
