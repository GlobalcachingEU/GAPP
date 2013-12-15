using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public interface ISettingsStorage: IDisposable
    {
        void StoreSetting(string name, string value);
        Hashtable LoadSettings();

        //geocache ignore filter
        Hashtable LoadIgnoredGeocacheCodes();
        Hashtable LoadIgnoredGeocacheNames();
        Hashtable LoadIgnoredGeocacheOwners();
        void ClearGeocacheIgnoreFilters();
        void AddIgnoreGeocacheCodes(List<string> codes);
        void AddIgnoreGeocacheNames(List<string> names);
        void AddIgnoreGeocacheOwners(List<string> owners);
        void DeleteIgnoreGeocacheCodes(List<string> codes);
        void DeleteIgnoreGeocacheNames(List<string> names);
        void DeleteIgnoreGeocacheOwners(List<string> owners);
    }
}
