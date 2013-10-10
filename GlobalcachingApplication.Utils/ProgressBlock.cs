using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class ProgressBlock : IDisposable
    {
        private bool _inProgress = false;
        private BasePlugin.Plugin _plugin = null;

        public ProgressBlock(BasePlugin.Plugin plugin, string actionTitle, string actionText, int max, int position)
        {
            _plugin = plugin;
            plugin.OnStartProgress(actionTitle, actionText, max, position);
            _inProgress = true;
        }

        public ProgressBlock(BasePlugin.Plugin plugin, string actionTitle, string actionText, int max, int position, bool canCancel)
        {
            _plugin = plugin;
            plugin.OnStartProgress(actionTitle, actionText, max, position, canCancel);
            _inProgress = true;
        }

        public bool UpdateProgress(string actionTitle, string actionText, int max, int position)
        {
            return _plugin.OnUpdateProgress(actionTitle, actionText, max, position);
        }

        public void Close()
        {
            if (_inProgress)
            {
                _plugin.OnEndProgress("", "", 1, 1);
                _inProgress = false;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
