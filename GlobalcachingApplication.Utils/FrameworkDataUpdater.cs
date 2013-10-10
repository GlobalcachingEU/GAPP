using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class FrameworkDataUpdater: IDisposable
    {
        private bool _inUpdate = false;
        private Framework.Interfaces.ICore _core = null;

        public FrameworkDataUpdater(Framework.Interfaces.ICore core)
        {
            _core = core;
            _core.LogImages.BeginUpdate();
            _core.GeocacheImages.BeginUpdate();
            _core.Logs.BeginUpdate();
            _core.Waypoints.BeginUpdate();
            _core.UserWaypoints.BeginUpdate();
            _core.Geocaches.BeginUpdate();
            _inUpdate = true;
        }

        public void Close()
        {
            if (_inUpdate)
            {
                _core.LogImages.EndUpdate();
                _core.GeocacheImages.EndUpdate();
                _core.Logs.EndUpdate();
                _core.Waypoints.EndUpdate();
                _core.UserWaypoints.EndUpdate();
                _core.Geocaches.EndUpdate();
                _inUpdate = false;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
