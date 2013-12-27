using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class Tag
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
