using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public interface ITempDirProvider
    {
        string GetPluginTempDirectory();
    }
}
