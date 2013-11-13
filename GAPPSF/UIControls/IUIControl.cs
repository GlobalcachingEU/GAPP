using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls
{
    public interface IUIControl
    {
        int WindowWidth { get; set; }
        int WindowHeight { get; set; }
        int WindowLeft { get; set; }
        int WindowTop { get; set; }
    }
}
