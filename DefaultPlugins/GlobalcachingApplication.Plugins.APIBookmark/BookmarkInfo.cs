using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.APIBookmark
{
    public class BookmarkInfo
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public List<string> GeocacheCodes { get; set; }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
