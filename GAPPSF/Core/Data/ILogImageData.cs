using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public interface ILogImageData
    {
        string ID { get; set; }
        DateTime DataFromDate { get; set; }
        string LogId { get; set; }
        string Url { get; set; }
        string Name { get; set; }
    }
}
