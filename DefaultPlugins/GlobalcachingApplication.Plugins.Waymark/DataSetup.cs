using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Waymark
{
    public class DataSetup : Utils.BasePlugin.Plugin
    {
        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.General;
            }
        }
        public override bool Prerequisite
        {
            get
            {
                return true;
            }
        }
        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            bool result = base.Initialize(core);
            if (result)
            {
                addCacheType(63542, "Waymark");
            }
            return result;
        }

        protected void addCacheType(int id, string name)
        {
            Framework.Data.GeocacheType ct = new Framework.Data.GeocacheType();
            ct.ID = id;
            ct.Name = name;
            Core.GeocacheTypes.Add(ct);
        }

    }
}
