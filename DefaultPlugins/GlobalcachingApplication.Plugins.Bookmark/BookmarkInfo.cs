using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GlobalcachingApplication.Plugins.Bookmark
{
    public class BookmarkInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Hashtable GeocacheCodes { get; private set; }

        public BookmarkInfo()
        {
            GeocacheCodes = new Hashtable();
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
