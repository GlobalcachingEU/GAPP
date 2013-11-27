using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.OfflineImages
{
    public class OfflineImagesManager
    {
        private static OfflineImagesManager _uniqueInstance = null;
        private static object _lockObject = new object();

        private OfflineImagesManager()
        {

        }

        public OfflineImagesManager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance==null)
                        {
                            _uniqueInstance = new OfflineImagesManager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }
    }
}
