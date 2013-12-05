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

        public long OffsetIdx { get; set; } //offset in index file. size is fixed
        public byte FieldType { get; set; }
        //pos:
        //0: OffsetIdx (long)
        //8: Offset (long)
        //16: Length (long)
        //24: field type (byte)
        //25: ID (100 bytes max) 

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
