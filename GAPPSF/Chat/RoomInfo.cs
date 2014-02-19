using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Chat
{
    public class RoomInfo
    {
        public string Name { get; set; }
        public bool present { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
