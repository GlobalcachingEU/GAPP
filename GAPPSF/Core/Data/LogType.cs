using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class LogType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool AsFound { get; set; }

        public LogType()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
