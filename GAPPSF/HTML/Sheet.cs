using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.HTML
{
    public class Sheet
    {
        public string Name { get; set; }
        public List<PropertyItem> SelectedItems { get; private set; }

        public Sheet()
        {
            SelectedItems = new List<PropertyItem>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
