using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class InternalStorageDestination: DataObject
    {
        public string Name { get; set; }
        public string PluginType { get; set; }
        public string[] StorageInfo { get; set; }

        public bool SameDestination(InternalStorageDestination dst)
        {
            bool result = false;
            if (dst != null)
            {
                if (string.Compare(Name, dst.Name, true)==0 && string.Compare(PluginType, dst.PluginType, true)==0)
                {
                    if (StorageInfo != null && dst.StorageInfo != null)
                    {
                        if (StorageInfo.Length == dst.StorageInfo.Length)
                        {
                            result = true;
                            for (int i = 0; i < StorageInfo.Length; i++)
                            {
                                if (string.Compare(StorageInfo[i], dst.StorageInfo[i], true) != 0)
                                {
                                    result = false;
                                    break;
                                }
                            }
                        }
                    }
                    else if (StorageInfo == null && dst.StorageInfo == null)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
