using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoUpdater
{
    public class AreaItemInfo
    {
        public int Code { get; private set; }
        public string Name { get; private set; }
        public string Level { get; private set; }

        public AreaItemInfo(string info)
        {
            var parts = info.Split(new char[] { ',' });
            Code = int.Parse(parts[2]);
            Name = parts[1];
            Level = parts[0];
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
