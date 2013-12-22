using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int ActionBuilderWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int ActionBuilderWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int ActionBuilderWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int ActionBuilderWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public string ActionBuilderFlowsXml
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public string ActionBuilderActiveFlowName
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public long ActionBuilderFlowID
        {
            get { return long.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
