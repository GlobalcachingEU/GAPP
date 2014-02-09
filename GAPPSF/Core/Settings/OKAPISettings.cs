using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string OKAPIActiveSiteID
        {
            get { return GetProperty(""); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoGermanyUserID
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoGermanyToken
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoGermanyTokenSecret
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoNetherlandsUserID
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoNetherlandsToken
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoNetherlandsTokenSecret
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoPolandUserID
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoPolandToken
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string OKAPISiteInfoPolandTokenSecret
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

    }
}
