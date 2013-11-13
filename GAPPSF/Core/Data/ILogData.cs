using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public interface ILogData
    {
        string ID { get; set; }
        LogType LogType { get; set; }
        string GeocacheCode { get; set; }
        string TBCode { get; set; }
        DateTime Date { get; set; }
        DateTime DataFromDate { get; set; }
        string FinderId { get; set; }
        string Finder { get; set; }
        string Text { get; set; }
        bool Encoded { get; set; }
    }
}
