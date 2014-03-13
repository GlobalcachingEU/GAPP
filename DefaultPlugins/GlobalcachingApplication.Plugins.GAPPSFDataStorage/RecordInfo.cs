using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GAPPSFDataStorage
{
    public class RecordInfo
    {
        //reserved fields:
        //0 =  length (long)
        //8 = 1 field type 0=free, 1=geocache (byte)
        //9..49 = reserved
        //50...149 = ID (string)
        //150 = record data
        public long Offset { get; set; }
        public long Length { get; set; }
        public string ID { get; set; }
        public string SubID { get; set; }

        public long OffsetIdx { get; set; } //offset in index file. size is fixed
        public byte FieldType { get; set; }
        //pos:
        //0: Offset (long)
        //8: Length (long)
        //16: field type (byte)
        //17: ID (100 bytes max) 

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
