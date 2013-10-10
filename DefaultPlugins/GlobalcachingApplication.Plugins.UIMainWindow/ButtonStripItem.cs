using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip |
                                           ToolStripItemDesignerAvailability.ContextMenuStrip |
                                           ToolStripItemDesignerAvailability.StatusStrip)]
    public class ButtonStripItem : ToolStripControlHost
    {
        private Button _button;

        public ButtonStripItem()
            : base(new Button())
        {
            this._button = this.Control as Button;
        }

        public Button Button
        {
            get { return _button; }
        }
    }
}
