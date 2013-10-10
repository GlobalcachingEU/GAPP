using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class ShortcutInfo: DataObject
    {
        public string PluginType { get; set; }
        public string PluginAction { get; set; }
        public string PluginSubAction { get; set; }
        public System.Windows.Forms.Keys ShortcutKeys { get; set; }
        public string ShortcutKeyString { get; set; }
    }
}
