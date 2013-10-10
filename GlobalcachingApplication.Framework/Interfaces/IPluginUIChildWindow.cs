using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IPluginUIChildWindow: IPlugin
    {
        System.Windows.Forms.Form ChildForm { get; }
    }
}
