using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Storage
{
    public class RecordInfo
    {
        //reserved fields:
        //0 =  length (long)
        //8 = 1 field type 0=free, 1=geocache (byte)
        //9..49 = reserved
        //50...149 = ID (string)
        //150 = record data
        public Database Database { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
        public string ID { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
