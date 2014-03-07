using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.Trackables
{
    public class LogItem
    {
        public string TrackableCode { get; set; }
        public string LogCode { get; set; }
        public string GeocacheCode { get; set; }
        public int ID { get; set; }
        public bool IsArchived { get; set; }
        public string LoggedBy { get; set; }
        public string LogGuid { get; set; }
        public bool LogIsEncoded { get; set; }
        public string LogText { get; set; }
        public int WptLogTypeId { get; set; }
        public string Url { get; set; }
        public DateTime UTCCreateDate { get; set; }
        public DateTime VisitDate { get; set; }
    }
}
