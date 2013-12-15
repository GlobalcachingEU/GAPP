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
            return (_ignoredGeocacheCodes[gcData.Code] != null || _ignoredGeocacheOwners[gcData.Owner ?? ""] != null || _ignoredGeocacheNames[gcData.Name ?? ""] != null);
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
            IgnoreGeocachesUpdateCounter++;
        }
        public void AddIgnoreGeocacheCodes(List<string> codes)
        {
            _settingsStorage.AddIgnoreGeocacheCodes(codes);
            IgnoreGeocachesUpdateCounter++;
        }
        public void AddIgnoreGeocacheNames(List<string> names)
        {
            _settingsStorage.AddIgnoreGeocacheNames(names);
            IgnoreGeocachesUpdateCounter++;
        }

        public void AddIgnoreGeocacheOwners(List<string> owners)
        {
            _settingsStorage.AddIgnoreGeocacheOwners(owners);
            IgnoreGeocachesUpdateCounter++;
        }
        public void DeleteIgnoreGeocacheCodes(List<string> codes)
        {
            _settingsStorage.DeleteIgnoreGeocacheCodes(codes);
            IgnoreGeocachesUpdateCounter++;
        }
        public void DeleteIgnoreGeocacheNames(List<string> names)
        {
            _settingsStorage.DeleteIgnoreGeocacheNames(names);
            IgnoreGeocachesUpdateCounter++;
        }

        public void DeleteIgnoreGeocacheOwners(List<string> owners)
        {
            _settingsStorage.DeleteIgnoreGeocacheOwners(owners);
            IgnoreGeocachesUpdateCounter++;
        }

    }
}
