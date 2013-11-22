using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class GeocacheAttribute
    {
        public enum State
        {
            NotSelected,
            Yes,
            No,
        }

        public int ID { get; set; }
        public string Name { get; set; }

        public GeocacheAttribute()
        {
        }
    }
}
