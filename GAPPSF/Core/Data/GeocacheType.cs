using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class GeocacheType: IComparable
    {
        public int ID { get; set; }
        public string Name { get; set; }
        private string _gpxTag;

        public GeocacheType()
        {
            _gpxTag = null;
        }

        public GeocacheType(string gpxTag)
        {
            _gpxTag = gpxTag;
        }

        public override string ToString()
        {
            return Name;
        }

        public string GPXTag
        {
            get { return _gpxTag ?? Name; }
            set { _gpxTag = value; }
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ToString(), obj.ToString());
        }
    }
}
