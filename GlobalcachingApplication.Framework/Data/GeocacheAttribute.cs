using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
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
