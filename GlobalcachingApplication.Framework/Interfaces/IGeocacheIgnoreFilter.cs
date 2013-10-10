using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IGeocacheIgnoreFilter
    {
        bool IgnoreGeocache(string code);
        bool IgnoreGeocache(Data.Geocache gc);
        List<string> FilterGeocaches(List<string> codes);
    }
}
