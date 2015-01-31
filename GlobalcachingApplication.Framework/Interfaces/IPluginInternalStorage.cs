using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IPluginInternalStorage: IPlugin
    {
        event EventArguments.PluginEventHandler DataSourceNameChanged;

        Task<bool> SaveAllData();
        string DataSourceName { get; }

        Framework.Data.InternalStorageDestination ActiveStorageDestination { get; }
        Task<bool> SetStorageDestination(Framework.Data.InternalStorageDestination dst);

        void StartReleaseForCopy();
        void EndReleaseForCopy();
    }
}
