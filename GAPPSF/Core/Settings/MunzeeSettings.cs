using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string MunzeeDFXAtUrls
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagMunzee
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagVirtual
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagMaintenance
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagBusiness
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagMystery
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagNFC
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
        public string MunzeeGPXTagPremium
        {
            get { return GetProperty("Traditional Cache"); }
            set { SetProperty(value); }
        }
    }
}
