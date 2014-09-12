using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string GeoSpyGPXTagCivil
        {
            get { return GetProperty("Virtual Cache"); }
            set { SetProperty(value); }
        }
        public string GeoSpyGPXTagHistoricAndReligious
        {
            get { return GetProperty("Virtual Cache"); }
            set { SetProperty(value); }
        }
        public string GeoSpyGPXTagNatural
        {
            get { return GetProperty("Virtual Cache"); }
            set { SetProperty(value); }
        }
        public string GeoSpyGPXTagTechnical
        {
            get { return GetProperty("Virtual Cache"); }
            set { SetProperty(value); }
        }
        public string GeoSpyGPXTagMilitary
        {
            get { return GetProperty("Virtual Cache"); }
            set { SetProperty(value); }
        }
    }
}
