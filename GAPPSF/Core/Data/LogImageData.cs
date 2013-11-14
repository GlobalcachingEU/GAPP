using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class LogImageData: ILogImageData
    {
        public string ID { get; set; }
        public DateTime DataFromDate { get; set; }
        public string LogId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }

        public static void Copy(ILogImageData src, ILogImageData dest)
        {
            dest.ID = src.ID;
            dest.DataFromDate = src.DataFromDate;
            dest.LogId = src.LogId;
            dest.Url = src.Url;
            dest.Name = src.Name;
        }
    }
}
