using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework
{
    public enum PluginType
    {
        Unknown,
        ImportData,
        ExportData,
        InternalStorage,
        UIMainWindow,
        UIChildWindow,
        Process,
        LiveAPI,
        LanguageSupport,
        General,
        GeocacheSelectFilter,
        ImageResource,
        Geometry,
        PluginManager,
        Action,
        Debug,
        GenericWindow,
        Script,
        Map,
        GeocacheIgnoreFilter,
        Account,
        GeocacheCollection
    }
}
