using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class LogData: ILogData
    {
        public string ID { get; set; }
        public LogType LogType { get; set; }
        public string GeocacheCode { get; set; }
        public string TBCode { get; set; }
        public DateTime Date { get; set; }
        public DateTime DataFromDate { get; set; }
        public string FinderId { get; set; }
        public string Finder { get; set; }
        public string Text { get; set; }
        public bool Encoded { get; set; }

        public static void Copy(ILogData src, ILogData dest)
        {
            dest.ID = src.ID;
            dest.LogType = src.LogType;
            dest.GeocacheCode = src.GeocacheCode;
            dest.TBCode = src.TBCode;
            dest.Date = src.Date;
            dest.DataFromDate = src.DataFromDate;
            dest.FinderId = src.FinderId;
            dest.Finder = src.Finder;
            dest.Text = src.Text;
            dest.Encoded = src.Encoded;
        }
    }
}
