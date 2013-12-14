using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.GeocacheFilter
{
    public enum GeocacheStatus
    {
        Enabled,
        Disabled,
        Archived
    }

    public enum BooleanEnum
    {
        True,
        False
    }

    public enum AttributeFilter
    {
        ContainsAll,
        ContainsAtLeastOne,
        ContainsNone
    }
}
