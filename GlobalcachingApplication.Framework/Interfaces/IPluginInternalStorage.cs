using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IPluginInternalStorage: IPlugin
    {
        event EventArguments.PluginEventHandler DataSourceNameChanged;

        bool SaveAllData();
        string DataSourceName { get; }

        Framework.Data.InternalStorageDestination ActiveStorageDestination { get; }
        bool SetStorageDestination(Framework.Data.InternalStorageDestination dst);

        void StartReleaseForCopy();
        void EndReleaseForCopy();
    }
}
