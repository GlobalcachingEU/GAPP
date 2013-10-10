using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class NotificationEventArgs: EventArgs
    {
        public Framework.Interfaces.IPlugin Plugin { get; private set; }
        public System.Windows.Forms.UserControl MessageBox { get; private set; }

        public NotificationEventArgs(Framework.Interfaces.IPlugin plugin, System.Windows.Forms.UserControl _messageBox)
        {
            Plugin = plugin;
            MessageBox = _messageBox;
        }
    }

    public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);
}
