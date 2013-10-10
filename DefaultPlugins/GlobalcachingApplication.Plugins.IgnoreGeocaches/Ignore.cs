using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.IgnoreGeocaches
{
    public class Ignore : Utils.BasePlugin.Plugin, Framework.Interfaces.IGeocacheIgnoreFilter
    {
        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheIgnoreFilter;
            }
        }

        public bool IgnoreGeocache(string code)
        {
            return IgnoreService.Instance(Core).IgnoreGeocache(code);
        }

        public bool IgnoreGeocache(Framework.Data.Geocache gc)
        {
            return IgnoreService.Instance(Core).IgnoreGeocache(gc);
        }

        public List<string> FilterGeocaches(List<string> codes)
        {
            return IgnoreService.Instance(Core).FilterGeocaches(codes);
        }
    }
}
