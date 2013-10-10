using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.StatsGen
{
    public class ScriptInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public ScriptType ScriptType { get; set; }
        public string Content { get; set; }
        public bool ReadOnly { get; set; }
        public bool Enabled { get; set; }
        public int Order { get; set; }
    }
}
