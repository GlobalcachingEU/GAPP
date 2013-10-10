using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class WaypointType
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public WaypointType()
        {
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
